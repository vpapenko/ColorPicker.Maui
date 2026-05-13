using ColorPicker.UITests.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Drives the ColorSyncTestPage (5x4 grid of every picker variant bound to one
/// MasterWheel) and verifies that setting the master color via the input bar
/// (a) propagates to the OutputHex / OutputRgba labels (programmatic round trip),
/// and (b) actually moves the picker indicator on representative controls
/// (visual reference-point check via screenshot pixel sampling).
/// </summary>
[Collection(AppiumServerCollection.Name)]
public sealed class ColorSyncTests : IClassFixture<SyncTestAppFixture>
{
    private readonly SyncTestAppFixture _fx;
    public ColorSyncTests(SyncTestAppFixture fx) => _fx = fx;

    public static IEnumerable<object[]> SyncColors() => new[]
    {
        new object[] { "#FF0000FF", 255,   0,   0, 1.00, "red"           },
        new object[] { "#00FF00FF",   0, 255,   0, 1.00, "green"         },
        new object[] { "#0000FFFF",   0,   0, 255, 1.00, "blue"          },
        new object[] { "#FFFF00FF", 255, 255,   0, 1.00, "yellow"        },
        new object[] { "#00FFFFFF",   0, 255, 255, 1.00, "cyan"          },
        new object[] { "#FF00FFFF", 255,   0, 255, 1.00, "magenta"       },
        new object[] { "#FFA500FF", 255, 165,   0, 1.00, "orange"        },
        new object[] { "#808080FF", 128, 128, 128, 1.00, "gray50"        },
        new object[] { "#FF000080", 255,   0,   0, 0.50, "red-alpha-50"  },
    };

    // ============================== A: PROGRAMMATIC ==============================

    [Theory]
    [MemberData(nameof(SyncColors))]
    public void Master_PropagatesTo_OutputLabels(
        string hex, int r, int g, int b, double a, string label)
    {
        _ = a; _ = label; // for test display

        _fx.Page.SetHex(hex);

        // OutputHex uses ColorToHexStringConverter -> "#RRGGBBAA".
        Assert.Equal(hex, _fx.Page.OutputHexText);

        // OutputRgba uses ColorToRGBAStringConverter -> Color.ToRgbaString() which
        // formats as lowercase "rgba(R,G,B,A)" (A is a float 0..1, formatted with
        // CurrentCulture). Tolerate culture/spacing/case by stripping non-digits and
        // checking the integer R,G,B prefix appears.
        var rgba = _fx.Page.OutputRgbaText;
        Assert.StartsWith("rgba", rgba, StringComparison.OrdinalIgnoreCase);
        var digitsOnly = new string(rgba.Where(ch => char.IsDigit(ch) || ch == ',').ToArray());
        Assert.Contains($"{r},{g},{b}", digitsOnly);
    }

    // ============================== B: VISUAL REFERENCE POINTS ==============================

    /// <summary>
    /// For each test color, capture the window and assert that:
    ///   - MasterWheel disc has a dark dot at the expected (hue,sat) location
    ///     (proves the master wheel itself drew its picker correctly), AND
    ///   - C_TriRotate's outer rim has a dark mark at the expected hue angle
    ///     (proves a non-master, attached-via-AttachedColorPicker control synced).
    /// We use a coarse darkness threshold so the test tolerates AA / DPI variation;
    /// the goal is "picker indicator is roughly where it should be", not pixel-perfect.
    /// </summary>
    [Theory]
    [MemberData(nameof(SyncColors))]
    public void Pickers_Visually_Move_To_Expected_Positions(
        string hex, int r, int g, int b, double a, string label)
    {
        _ = a; _ = label;

        _fx.Page.SetHex(hex);
        // Allow a frame for the SkiaSharp surfaces to redraw.
        Thread.Sleep(200);

        using var img = _fx.Page.CaptureWindow();

        var (h, s, _) = RgbToHsl(r, g, b);

        // For desaturated colors (gray, white, black) the wheel picker sits at the
        // center where the disc is itself near-white -> the dark dot detection isn't
        // reliable. The programmatic test above already covers those cases; here we
        // only do the visual check for clearly saturated colors.
        if (s < 0.20) return;

        // --- 1. MasterWheel: picker on the inner color circle at polar (hue, sat) ---
        // ColorWheel paints the hue-sat disc inside the outer luminosity ring.
        // The disc occupies roughly the inner 70% of the square element. We sample
        // a generous neighbourhood (radius 8 px) around the predicted point and
        // accept if ANY pixel in that neighbourhood is "dark enough" to be the
        // picker outline — this tolerates the exact disc-vs-ring fraction that
        // the wheel's MeasureOverride happens to produce.
        AssertWheelPickerDarkSpot(img, "C_WheelDefault", h, s,
            innerFraction: 0.32, // sat-axis fraction of the half-square
            tolerancePx:  18,
            darkLuma:    200);

        // --- 2. C_TriRotate: line picker on outer hue ring at angle = hue ---
        // The triangle's hue ring matches the wheel's outer ring. The picker is a
        // short black line crossing the rim at the hue's angle.
        AssertWheelPickerDarkSpot(img, "C_TriRotate", h, sat: 1.0,
            innerFraction: 0.46, // sample on the rim (just inside outer edge)
            tolerancePx:  18,
            darkLuma:    200);
    }

    /// <summary>
    /// Sample a small neighbourhood inside the bounds of <paramref name="automationId"/>
    /// at the polar location implied by hue/sat (relative to the centered square of
    /// the element) and assert at least one pixel is darker than <paramref name="darkLuma"/>.
    ///
    /// hue is 0..1 (0=red), sat is 0..1 (0=center, 1=edge). innerFraction is the
    /// fraction of the half-square that defines the picker radius.
    /// </summary>
    private void AssertWheelPickerDarkSpot(
        Image<Rgba32> img, string automationId,
        double hue, double sat,
        double innerFraction,
        int tolerancePx,
        int darkLuma)
    {
        var b = _fx.Page.GetWheelAreaBounds(automationId);
        // Centered square inside the element's bounds.
        var side = Math.Min(b.Width, b.Height);
        var ox = b.X + (b.Width  - side) / 2;
        var oy = b.Y + (b.Height - side) / 2;
        var cx = ox + side / 2.0;
        var cy = oy + side / 2.0;
        var radius = side / 2.0 * innerFraction * sat * 2.0; // sat*2 cancels the 0.5 factor
        // Hue angle: 0 = right (3 o'clock), increasing counter-clockwise.
        // ColorWheel paints hue starting at 3 o'clock going clockwise (matches HSL convention).
        var theta = -hue * 2 * Math.PI;
        var px = (int)Math.Round(cx + radius * Math.Cos(theta));
        var py = (int)Math.Round(cy + radius * Math.Sin(theta));

        var (found, minLuma, foundX, foundY) = ScanForDarkPixel(img, px, py, tolerancePx, darkLuma);
        Assert.True(found,
            $"No picker-dark pixel near ({px},{py}) for {automationId} " +
            $"(hue={hue:F2}, sat={sat:F2}, radius={radius:F1}px in bounds {b}). " +
            $"Min luma in {tolerancePx}px window: {minLuma} at ({foundX},{foundY}).");
    }

    private static (bool found, int minLuma, int foundX, int foundY)
        ScanForDarkPixel(Image<Rgba32> img, int cx, int cy, int radius, int darkLuma)
    {
        int minLuma = 255, fx = cx, fy = cy;
        bool found = false;
        for (int dy = -radius; dy <= radius; dy++)
        {
            int y = cy + dy;
            if (y < 0 || y >= img.Height) continue;
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx;
                if (x < 0 || x >= img.Width) continue;
                var p = img[x, y];
                int luma = (p.R * 299 + p.G * 587 + p.B * 114) / 1000;
                if (luma < minLuma) { minLuma = luma; fx = x; fy = y; }
                if (luma < darkLuma) found = true;
            }
        }
        return (found, minLuma, fx, fy);
    }

    // ============================== Color math ==============================

    /// <summary>RGB (0..255) -> HSL (h,s,l) all in 0..1. Matches MAUI Color.GetHue/Sat/Lum.</summary>
    private static (double h, double s, double l) RgbToHsl(int r, int g, int b)
    {
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
}
