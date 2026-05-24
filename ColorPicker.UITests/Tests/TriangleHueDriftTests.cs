using System.Globalization;
using System.Text.RegularExpressions;
using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Regression test for the rotating-triangle hue-drag S/L drift bug fixed in
/// PR #36. Symptom: when <c>RotateTriangleByHue</c> is enabled, every touch
/// event on the hue ring used to re-decode BOTH H and S/V through the
/// triangle's pixel-quantized round-trip. Slow drags accumulated quantization
/// noise into Saturation/Luminosity. Fix: split UpdateColors so a hue-ring
/// drag only re-decodes hue, leaving S/L untouched.
/// </summary>
[Collection(AppiumServerCollection.Name)]
public sealed class TriangleHueDriftTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public TriangleHueDriftTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void DraggingHueRing_DoesNotDriftSaturationAndLuminosity()
    {
        var triangleWasOn = _fx.Page.IsToggleOn("ShowTriangleSwitch");
        if (!triangleWasOn) _fx.Page.Toggle(_fx.Page.ShowTriangleSwitch);

        // The RotateTriangleByHue switch is only visible when ShowTriangle is on,
        // so we must read it after toggling the triangle on.
        var rotateWasOn = _fx.Page.IsToggleOn("RotateTriangleByHue");

        try
        {
            if (!rotateWasOn) _fx.Page.Toggle(_fx.Page.RotateTriangleByHueSwitch);

            // Seed a mid-range S/L by tapping the SV triangle interior.
            // A point a bit below the hue ring centre lands well inside the
            // active triangle area regardless of current hue rotation.
            _fx.Page.TapInsideSquare(_fx.Page.ColorTriangle, 0.50, 0.55);
            Thread.Sleep(200);

            var hsla0 = ParseHsla(_fx.Page.SelectedColorHsla);

            // Drag a long arc along the hue ring (rim ≈ 0.95 of half-side).
            // Many short segments ⇒ many touch samples ⇒ the buggy code
            // accumulates measurable drift; the fixed code stays stable.
            // Sweep almost two full revolutions to maximize per-event noise.
            for (int pass = 0; pass < 3; pass++)
            {
                _fx.Page.DragArcInsideSquare(_fx.Page.ColorTriangle,
                    radius: 0.95, startDeg: 20, endDeg: 340,
                    segments: 80, totalMs: 800);
                Thread.Sleep(50);
            }
            Thread.Sleep(300);

            var hsla1 = ParseHsla(_fx.Page.SelectedColorHsla);

            // Sanity: hue actually moved (otherwise we didn't hit the ring).
            Assert.True(Math.Abs(hsla1.H - hsla0.H) > 10,
                $"Hue did not change as expected: H0={hsla0.H}, H1={hsla1.H} (HSLA0={hsla0.Raw}, HSLA1={hsla1.Raw})");

            // Bug repro produced multi-percent S/L drift; the fix keeps S/L
            // pixel-stable. Tolerance set to catch any non-trivial regression
            // while tolerating sub-percent rounding in the HSLA readout.
            const double tol = 0.015;
            var dS = Math.Abs(hsla1.S - hsla0.S);
            var dL = Math.Abs(hsla1.L - hsla0.L);
            Assert.True(dS <= tol,
                $"Saturation drifted: S0={hsla0.S:F3}, S1={hsla1.S:F3} (HSLA0={hsla0.Raw}, HSLA1={hsla1.Raw})");
            Assert.True(dL <= tol,
                $"Luminosity drifted: L0={hsla0.L:F3}, L1={hsla1.L:F3} (HSLA0={hsla0.Raw}, HSLA1={hsla1.Raw})");
        }
        finally
        {
            if (_fx.Page.IsToggleOn("RotateTriangleByHue") != rotateWasOn)
                _fx.Page.Toggle(_fx.Page.RotateTriangleByHueSwitch);
            if (_fx.Page.IsToggleOn("ShowTriangleSwitch") != triangleWasOn)
                _fx.Page.Toggle(_fx.Page.ShowTriangleSwitch);
        }
    }

    private readonly record struct Hsla(double H, double S, double L, double A, string Raw);

    /// <summary>Parse MAUI's Color.ToHslaString() output, e.g. "hsla(354, 100%, 50%, 1)".</summary>
    private static Hsla ParseHsla(string text)
    {
        var m = Regex.Match(text ?? string.Empty,
            @"hsla?\(\s*([\d.+-]+)\s*,\s*([\d.+-]+)%?\s*,\s*([\d.+-]+)%?\s*(?:,\s*([\d.+-]+))?\s*\)",
            RegexOptions.IgnoreCase);
        if (!m.Success)
            throw new FormatException($"Unrecognized HSLA string: '{text}'");

        var ci = CultureInfo.InvariantCulture;
        double h = double.Parse(m.Groups[1].Value, ci);
        double s = double.Parse(m.Groups[2].Value, ci) / 100.0;
        double l = double.Parse(m.Groups[3].Value, ci) / 100.0;
        double a = m.Groups[4].Success ? double.Parse(m.Groups[4].Value, ci) : 1.0;
        return new Hsla(h, s, l, a, text!);
    }
}
