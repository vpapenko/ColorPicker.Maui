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
    /// For each test color, capture the window and assert that the picker
    /// indicators on MasterWheel and C_TriRotate are at the EXACT positions
    /// captured in <see cref="ColorSyncExpectedPickerOffsets"/>. Positions are
    /// stored as (relX, relY) inside each cell's wheel-area bounds, so this
    /// test contains zero hue/sat/angle math — just lookup + sample.
    ///
    /// Desaturated colors (gray/white/black) skip the visual check because the
    /// picker sits at the desaturated center where there is no reliable dark
    /// pixel to detect; the programmatic test above already covers them.
    /// </summary>
    [Theory]
    [MemberData(nameof(SyncColors))]
    public void Pickers_Visually_Move_To_Expected_Positions(
        string hex, int r, int g, int b, double a, string label)
    {
        _ = a; _ = label;

        _fx.Page.SetHex(hex);
        Thread.Sleep(200); // SkiaSharp redraw flush
        using var img = _fx.Page.CaptureWindow();

        var (_, s, _) = RgbToHsl(r, g, b);
        if (s < 0.20) return; // see XML doc

        AssertPickerAt(img, "C_WheelDefault", hex);
        AssertPickerAt(img, "C_TriRotate",    hex);
    }

    /// <summary>
    /// Look up the empirical (relX, relY) for this (cell, color) and assert the
    /// rendered picker dark spot is within <see cref="TolerancePx"/> of it.
    /// </summary>
    private void AssertPickerAt(Image<Rgba32> img, string cellId, string hex)
    {
        if (!ColorSyncExpectedPickerOffsets.Offsets.TryGetValue((cellId, hex), out var rel))
            throw new InvalidOperationException(
                $"No empirical picker offset recorded for ({cellId}, {hex}). " +
                "Re-run ColorSyncDataGenerator with GEN_PICKER_DATA=1.");

        var b = _fx.Page.GetWheelAreaBounds(cellId);
        int px = b.X + (int)Math.Round(rel.Rx * b.Width);
        int py = b.Y + (int)Math.Round(rel.Ry * b.Height);

        var (found, minLuma, fx, fy) = ScanForDarkPixel(img, px, py, TolerancePx, DarkLuma);
        Assert.True(found,
            $"No picker-dark pixel near ({px},{py}) for {cellId} {hex} " +
            $"(expected rel=({rel.Rx:F4},{rel.Ry:F4}) in bounds {b}). " +
            $"Min luma in {TolerancePx}px window: {minLuma} at ({fx},{fy}).");
    }

    private const int TolerancePx = 18;
    private const int DarkLuma    = 200;

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
