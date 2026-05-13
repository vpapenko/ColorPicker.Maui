using ColorPicker.UITests.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// UI-driven sync tests: simulate the user TAPPING on a child control's canvas
/// at a known (hue,sat) location and verify
///   (a) OutputHex/OutputRgba update to a color whose hue/sat is in the expected
///       neighbourhood (proves the tapped control wrote its color to the master), and
///   (b) other controls' picker indicators visually moved to match the new color
///       (proves the master propagated to bound controls).
///
/// Strategy: starting from a known SetHex baseline, tap a target polar position
/// on the master wheel disc, then read OutputHex back and assert hue/sat.
/// </summary>
[Collection(AppiumServerCollection.Name)]
public sealed class ColorSyncUiDrivenTests : IClassFixture<SyncTestAppFixture>
{
    private readonly SyncTestAppFixture _fx;
    public ColorSyncUiDrivenTests(SyncTestAppFixture fx) => _fx = fx;

    // ============================== A: TAP CENTER ==============================

    /// <summary>
    /// Tapping the center of the master wheel disc must move the picker to sat=0
    /// (any hue / center of disc), so the resulting color must be (near-)gray.
    /// We start from saturated red so we know the tap actually moved the picker.
    /// </summary>
    [Fact]
    public void Tap_MasterWheelCenter_ProducesDesaturatedColor()
    {
        _fx.Page.SetHex("#FF0000FF");
        var before = _fx.Page.OutputHexText;

        _fx.Page.TapPolar("C_WheelDefault", hue: 0, sat: 0);
        var after = _fx.Page.WaitForOutputHexChange(before);

        var (_, s, _) = ParseHexToHsl(after);
        Assert.True(s < 0.20,
            $"After tapping wheel center, expected near-gray (sat<0.20). Got {after} sat={s:F2}.");
    }

    // ============================== B: TAP RIM (HUE) ==============================

    public static IEnumerable<object[]> RimHues() => new[]
    {
        // hue (0..1)   tolerance   label
        new object[] { 0.00, 0.08, "right=red"     },
        new object[] { 0.50, 0.08, "left=cyan"     },
    };

    /// <summary>
    /// Tapping near the rim of the master wheel disc at angle = hue should
    /// produce an output color whose hue lands in a tolerance band around the
    /// tapped hue (sat must be high since we tapped near the rim).
    /// </summary>
    [Theory]
    [MemberData(nameof(RimHues))]
    public void Tap_MasterWheelRim_AtHueAngle_ProducesExpectedHue(
        double hue, double hueTol, string label)
    {
        _ = label;
        // Mid-gray baseline: lum=0.5, sat=0. Tapping the rim sets sat=1 at the
        // tapped hue while keeping lum=0.5 so the resulting color is clearly
        // saturated (vs. starting from white where lum=1 keeps everything white).
        _fx.Page.SetHex("#808080FF");
        var before = _fx.Page.OutputHexText;

        _fx.Page.TapPolar("C_WheelDefault", hue: hue, sat: 0.95);
        var after = _fx.Page.WaitForOutputHexChange(before);

        var (h, s, _) = ParseHexToHsl(after);
        Assert.True(s > 0.50,
            $"Expected high sat after tapping rim. Got {after} sat={s:F2}.");
        var dh = HueDistance(h, hue);
        Assert.True(dh <= hueTol,
            $"Hue mismatch: tapped {hue:F2}, got {h:F2} ({after}), distance {dh:F2} > tol {hueTol:F2}.");
    }

    // ============================== C: CROSS-CONTROL PROPAGATION ==============================

    /// <summary>
    /// Tap the master wheel rim at a known hue, then capture the window and
    /// verify that C_TriRotate (a different, attached control) drew its own
    /// rim-picker mark at that same hue angle. This is the key sync assertion:
    /// the master->child propagation actually reaches a non-master control.
    /// </summary>
    [Fact]
    public void Tap_MasterWheel_VisuallyMoves_TrianglePicker()
    {
        _fx.Page.SetHex("#808080FF");
        var before = _fx.Page.OutputHexText;

        // Tap rim at hue=0 (3 o'clock = red).
        _fx.Page.TapPolar("C_WheelDefault", hue: 0, sat: 0.95);
        _ = _fx.Page.WaitForOutputHexChange(before);
        Thread.Sleep(200); // allow SkiaSharp redraws to flush

        using var img = _fx.Page.CaptureWindow();
        var (h, _, _) = ParseHexToHsl(_fx.Page.OutputHexText);

        // Sample C_TriRotate's rim at the actual hue we ended up with.
        AssertRimDarkSpot(img, "C_TriRotate", h, innerFraction: 0.46,
            tolerancePx: 18, darkLuma: 200);
    }

    // ============================== HELPERS ==============================

    private void AssertRimDarkSpot(
        Image<Rgba32> img, string cellId, double hue,
        double innerFraction, int tolerancePx, int darkLuma)
    {
        var b = _fx.Page.GetWheelAreaBounds(cellId);
        var side = Math.Min(b.Width, b.Height);
        var ox = b.X + (b.Width  - side) / 2;
        var oy = b.Y + (b.Height - side) / 2;
        var cx = ox + side / 2.0;
        var cy = oy + side / 2.0;
        var radius = side / 2.0 * innerFraction * 1.0 * 2.0; // sat=1
        var theta = Math.PI - hue * 2 * Math.PI;
        var px = (int)Math.Round(cx + radius * Math.Cos(theta));
        var py = (int)Math.Round(cy + radius * Math.Sin(theta));

        int minLuma = 255;
        bool found = false;
        for (int dy = -tolerancePx; dy <= tolerancePx; dy++)
        {
            int y = py + dy; if (y < 0 || y >= img.Height) continue;
            for (int dx = -tolerancePx; dx <= tolerancePx; dx++)
            {
                int x = px + dx; if (x < 0 || x >= img.Width) continue;
                var p = img[x, y];
                int luma = (p.R * 299 + p.G * 587 + p.B * 114) / 1000;
                if (luma < minLuma) minLuma = luma;
                if (luma < darkLuma) { found = true; break; }
            }
            if (found) break;
        }
        Assert.True(found,
            $"No picker-dark pixel near ({px},{py}) for {cellId} (hue={hue:F2}). " +
            $"Min luma in {tolerancePx}px window: {minLuma}.");
    }

    /// <summary>Parse "#RRGGBBAA" or "#RRGGBB" -> HSL.</summary>
    private static (double h, double s, double l) ParseHexToHsl(string hex)
    {
        hex = hex.TrimStart('#');
        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        double dr = r / 255.0, dg = g / 255.0, db = b / 255.0;
        double max = Math.Max(dr, Math.Max(dg, db));
        double min = Math.Min(dr, Math.Min(dg, db));
        double l = (max + min) / 2.0;
        double h = 0, s = 0;
        if (max != min)
        {
            double d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
            if      (max == dr) h = (dg - db) / d + (dg < db ? 6 : 0);
            else if (max == dg) h = (db - dr) / d + 2;
            else                h = (dr - dg) / d + 4;
            h /= 6.0;
        }
        return (h, s, l);
    }

    /// <summary>Shortest distance between two normalized hues (0..1, wraps).</summary>
    private static double HueDistance(double a, double b)
    {
        double d = Math.Abs(a - b) % 1.0;
        return Math.Min(d, 1.0 - d);
    }
}
