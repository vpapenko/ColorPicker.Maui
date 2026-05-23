using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// UI-driven sync tests: simulate the user TAPPING on a child control's canvas
/// at a known location and verify
///   (a) OutputHex/OutputRgba update to a color in the expected neighbourhood
///       (proves the tapped control wrote its color to the master), and
///   (b) other controls' picker indicators visually moved to match
///       (proves the master propagated to bound controls).
///
/// Tap targets and assertion positions are looked up in
/// <see cref="ColorSyncExpectedPickerOffsets"/>, so this file contains zero
/// hue/sat/angle math.
/// </summary>
[Collection(AppiumServerCollection.Name)]
public sealed class ColorSyncUiDrivenTests : IClassFixture<SyncTestAppFixture>
{
    private readonly SyncTestAppFixture _fx;
    public ColorSyncUiDrivenTests(SyncTestAppFixture fx) => _fx = fx;

    private const int TolerancePx = 18;
    private const int DarkLuma    = 200;

    // ============================== A: TAP CENTER ==============================

    /// <summary>
    /// Tapping the center of the master wheel disc must move the picker to sat=0,
    /// so the resulting color must be (near-)gray. We start from saturated red so
    /// we know the tap actually moved the picker.
    /// </summary>
    [Fact]
    public void Tap_MasterWheelCenter_ProducesDesaturatedColor()
    {
        _fx.Page.SetHex("#FF0000FF");
        var before = _fx.Page.OutputHexText;

        // Center of the cell is sat=0 on every wheel — no math, no lookup needed.
        _fx.Page.TapAtRel("C_WheelDefault", 0.5, 0.5);
        var after = _fx.Page.WaitForOutputHexChange(before);

        var (_, s, _) = ParseHexToHsl(after);
        Assert.True(s < 0.20,
            $"After tapping wheel center, expected near-gray (sat<0.20). Got {after} sat={s:F2}.");
    }

    // ============================== B: TAP RIM (HUE) ==============================

    /// <summary>
    /// Pure rim hues we have lookup offsets for. Tapping at the recorded picker
    /// position for a fully-saturated color must produce that color (within a
    /// hue-distance tolerance — the pixel tolerance window can land us a few
    /// degrees off the precise hue, but not in a different sextant).
    /// </summary>
    public static IEnumerable<object[]> RimHues() => new[]
    {
        new object[] { "#FF0000FF", 0.00, 0.08, "red"  },
        new object[] { "#00FFFFFF", 0.50, 0.08, "cyan" },
    };

    [Theory]
    [MemberData(nameof(RimHues))]
    public void Tap_MasterWheelRim_AtRecordedPickerLocation_ProducesExpectedHue(
        string targetHex, double expectedHue, double hueTol, string label)
    {
        _ = label;

        // Mid-gray baseline so we know the tap actually moved the picker.
        _fx.Page.SetHex("#808080FF");
        var before = _fx.Page.OutputHexText;

        var rel = ColorSyncExpectedPickerOffsets.Offsets[("C_WheelDefault", targetHex)];
        _fx.Page.TapAtRel("C_WheelDefault", rel.Rx, rel.Ry);
        var after = _fx.Page.WaitForOutputHexChange(before);

        var (h, s, _) = ParseHexToHsl(after);
        Assert.True(s > 0.50, $"Expected high sat after tapping rim. Got {after} sat={s:F2}.");
        var dh = HueDistance(h, expectedHue);
        Assert.True(dh <= hueTol,
            $"Hue mismatch: tapped recorded position for {targetHex} (hue {expectedHue:F2}), " +
            $"got {after} (hue {h:F2}, distance {dh:F2} > tol {hueTol:F2}).");
    }

    // ============================== C: CROSS-CONTROL PROPAGATION ==============================

    /// <summary>
    /// Tap the master wheel at the recorded picker location for red, then
    /// capture the window and verify C_TriRotate (a different, attached control)
    /// drew its own picker mark at the recorded location for red. Both tap and
    /// assertion positions come from the lookup table, so no math here.
    /// </summary>
    [Fact]
    public void Tap_MasterWheel_VisuallyMoves_TrianglePicker()
    {
        const string TargetHex = "#FF0000FF";

        _fx.Page.SetHex("#808080FF");
        var before = _fx.Page.OutputHexText;

        var tapRel = ColorSyncExpectedPickerOffsets.Offsets[("C_WheelDefault", TargetHex)];
        _fx.Page.TapAtRel("C_WheelDefault", tapRel.Rx, tapRel.Ry);
        _ = _fx.Page.WaitForOutputHexChange(before);
        Thread.Sleep(200); // SkiaSharp redraw flush

        using var img = _fx.Page.CaptureWindow();
        AssertPickerAt(img, "C_TriRotate", TargetHex);
    }

    // ============================== HELPERS ==============================

    private void AssertPickerAt(PixelImage img, string cellId, string hex)
    {
        var rel = ColorSyncExpectedPickerOffsets.Offsets[(cellId, hex)];
        var b = _fx.Page.GetWheelAreaBounds(cellId);
        int px = b.X + (int)Math.Round(rel.Rx * b.Width);
        int py = b.Y + (int)Math.Round(rel.Ry * b.Height);

        int minLuma = 255;
        bool found = false;
        for (int dy = -TolerancePx; dy <= TolerancePx && !found; dy++)
        {
            int y = py + dy; if (y < 0 || y >= img.Height) continue;
            for (int dx = -TolerancePx; dx <= TolerancePx; dx++)
            {
                int x = px + dx; if (x < 0 || x >= img.Width) continue;
                var p = img[x, y];
                int luma = (p.R * 299 + p.G * 587 + p.B * 114) / 1000;
                if (luma < minLuma) minLuma = luma;
                if (luma < DarkLuma) { found = true; break; }
            }
        }
        Assert.True(found,
            $"No picker-dark pixel near ({px},{py}) for {cellId} {hex}. " +
            $"Min luma in {TolerancePx}px window: {minLuma}.");
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
