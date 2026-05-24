using System.Globalization;
using System.Text.RegularExpressions;
using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Invariant guard for the rotating-triangle hue-drag S/L behavior. PR #36
/// fixed a visual drift where, during a slow human drag on the hue ring,
/// each touch event re-decoded both the hue ring AND the SV indicator
/// through the triangle's roundtrip — accumulating tiny per-event noise.
///
/// IMPORTANT: This UI test cannot reproduce the original bug
/// deterministically. The drift was sub-percent per event and only became
/// visible after many human-paced events with intervening repaints; the
/// HSLA readout label rounds to whole percent and the MAUI Color HSL↔RGB
/// roundtrip on this code path is bit-stable in floats, so simulated
/// Appium drags (even slow, with explicit repaint windows between 1°
/// segments and varied SV seed points) report exactly 0.0000 drift even
/// when the buggy UpdateColorsFromH is reintroduced.
///
/// Instead, this test enforces the **invariant** the fix establishes:
/// dragging the hue ring must leave Saturation and Luminosity within a
/// tight tolerance of their pre-drag values. It catches gross regressions
/// (e.g. re-merging the two touch paths in a way that visibly perturbs
/// S/L). Deterministic coverage of the original quantization-noise bug
/// would require refactoring ColorTriangleArea so its touch logic is
/// unit-testable without the SkiaSharp runtime.
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

            // Seed at an asymmetric SV position so the encode/decode path
            // hits a non-degenerate point in HSL space.
            _fx.Page.TapInsideSquare(_fx.Page.ColorTriangle, 0.40, 0.62);
            Thread.Sleep(200);

            var hsla0 = ParseHsla(_fx.Page.SelectedColorHsla);

            // Drag a long arc along the hue ring (rim ≈ 0.95 of half-side).
            // Many short segments ⇒ many touch samples ⇒ the buggy code
            // accumulates measurable drift; the fixed code stays stable.
            // Sweep almost two full revolutions to maximize per-event noise.
            // Slow drag along the hue ring (rim ≈ 0.95 of half-side).
            // Each 1° step is a separate W3C action call with a real-time
            // pause after it, so the SkiaSharp surface repaints between
            // events — the timing condition that triggered the original
            // S/L drift on the rotating triangle.
            for (int pass = 0; pass < 2; pass++)
            {
                _fx.Page.DragArcInsideSquare(_fx.Page.ColorTriangle,
                    radius: 0.95, startDeg: 20, endDeg: 340,
                    segments: 80, pauseMsBetweenSegments: 30);
                Thread.Sleep(100);
            }
            Thread.Sleep(300);

            var hsla1 = ParseHsla(_fx.Page.SelectedColorHsla);

            // Sanity: hue actually moved (otherwise we didn't hit the ring).
            Assert.True(Math.Abs(hsla1.H - hsla0.H) > 10,
                $"Hue did not change as expected: H0={hsla0.H}, H1={hsla1.H} (HSLA0={hsla0.Raw}, HSLA1={hsla1.Raw})");

            // The original drift bug (PR #36) showed up visually as the SV
            // indicator dancing under a slow human hue-ring drag. The HSLA
            // readout label rounds to whole percent, and on this code path
            // the MAUI Color HSL↔RGB round-trip is bit-stable in floats,
            // so the cumulative quantization noise the user observed does
            // not surface through the label. That means this UI test cannot
            // FAIL deterministically when the bug is reintroduced — it
            // serves as an **invariant guard**: it asserts that a hue-ring
            // drag does not perturb S or L beyond a tight tolerance, which
            // would catch any regression that grossly re-couples the two
            // touch paths or otherwise leaks SV state into hue handling.
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
