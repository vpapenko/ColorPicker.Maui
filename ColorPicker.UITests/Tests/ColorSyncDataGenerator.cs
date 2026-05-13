using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// One-shot generator for ColorSyncExpectedPickerOffsets.
///
/// Empirically captures the actual picker dark-spot position for each
/// (color, cellAutomationId) pair by scanning the rendered screenshot for the
/// lowest-luma pixel inside that cell's wheel-area. Emits a ready-to-paste C#
/// initializer to the test output (and to a file under the project's TestData
/// folder) so the production assertion tests can stay free of any picker-position
/// math.
///
/// Skipped by default. Enable by setting env var GEN_PICKER_DATA=1.
/// </summary>
[Collection(AppiumServerCollection.Name)]
public sealed class ColorSyncDataGenerator : IClassFixture<SyncTestAppFixture>
{
    private readonly SyncTestAppFixture _fx;
    private readonly ITestOutputHelper _out;

    public ColorSyncDataGenerator(SyncTestAppFixture fx, ITestOutputHelper output)
    {
        _fx = fx;
        _out = output;
    }

    // (hex, label) — desaturated colors are excluded because they don't produce
    // a reliable dark picker dot (gray/white/black sit at the desaturated center).
    private static readonly (string Hex, string Label)[] Colors = new[]
    {
        ("#FF0000FF", "red"),
        ("#00FF00FF", "green"),
        ("#0000FFFF", "blue"),
        ("#FFFF00FF", "yellow"),
        ("#00FFFFFF", "cyan"),
        ("#FF00FFFF", "magenta"),
        ("#FFA500FF", "orange"),
        ("#FF000080", "red-alpha-50"),
    };

    // Cells we want baseline data for, with their search annulus (radius range
    // as fraction of half-square) where the *interesting* picker lives.
    //   - WheelDefault has both an outer luminosity-ring picker AND an inner
    //     hue-sat disc picker. The lum picker doesn't move with hue (sync test
    //     would be trivial), so restrict to the inner disc only.
    //   - TriRotate has a rim hue-ring picker (moves with hue) and an inner
    //     triangle sat-picker — we want the rim picker.
    private static readonly (string Cell, double InnerR, double OuterR)[] CellSpecs = new[]
    {
        // Lum-ring picker sits at ~0.82 radius; disc picker sits out to ~0.70.
        ("C_WheelDefault", 0.00, 0.72),  // inner disc only (excludes lum ring at 0.82)
        ("C_TriRotate",    0.78, 0.98),  // outer hue ring only (excludes inner triangle)
    };

    [Fact]
    public void Generate_ExpectedPickerOffsets()
    {
        // Gate: this test mutates a source file in the repo; only run on demand.
        if (Environment.GetEnvironmentVariable("GEN_PICKER_DATA") != "1")
        {
            _out.WriteLine("Skipped. Set GEN_PICKER_DATA=1 to regenerate.");
            return;
        }

        var entries = new List<string>();
        foreach (var (hex, label) in Colors)
        {
            _fx.Page.SetHex(hex);
            Thread.Sleep(250); // SkiaSharp redraw flush
            using var img = _fx.Page.CaptureWindow();

            foreach (var (cell, innerR, outerR) in CellSpecs)
            {
                var b = _fx.Page.GetWheelAreaBounds(cell);
                var (px, py, luma) = FindDarkestPixel(img, b, innerR, outerR);
                if (px < 0)
                {
                    _out.WriteLine($"// SKIP {cell} {hex} ({label}) — no candidate found");
                    continue;
                }
                double rx = (px - b.X) / (double)b.Width;
                double ry = (py - b.Y) / (double)b.Height;
                entries.Add(
                    $"        [(\"{cell}\", \"{hex}\")] = ({rx:F4}, {ry:F4}), // {label,-13} luma={luma}");
            }
        }

        var generated = string.Join(Environment.NewLine, entries);
        _out.WriteLine("// === Generated picker offsets (paste into ColorSyncExpectedPickerOffsets.cs) ===");
        _out.WriteLine(generated);

        // Also write to a file for easy capture even if the test runner truncates output.
        var outDir = Path.Combine(AppContext.BaseDirectory, "TestOutput");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir, "ColorSyncExpectedPickerOffsets.generated.txt");
        File.WriteAllText(outFile, generated + Environment.NewLine);
        _out.WriteLine($"// Written to: {outFile}");
    }

    /// <summary>
    /// Find the darkest pixel inside an annulus (inner..outer fraction of the
    /// half-square) inside the cell's wheel-area. Excludes a small border to
    /// avoid the LightGray cell stroke.
    /// </summary>
    private static (int x, int y, int luma) FindDarkestPixel(
        Image<Rgba32> img, Bounds b, double innerFrac, double outerFrac)
    {
        int side = Math.Min(b.Width, b.Height);
        int ox = b.X + (b.Width  - side) / 2;
        int oy = b.Y + (b.Height - side) / 2;
        double cx = ox + side / 2.0;
        double cy = oy + side / 2.0;
        double half = side / 2.0;
        double minR2 = (innerFrac * half) * (innerFrac * half);
        double maxR2 = (outerFrac * half) * (outerFrac * half);

        int bestX = -1, bestY = -1, bestLuma = 256;
        for (int y = oy; y < oy + side; y++)
        {
            if (y < 0 || y >= img.Height) continue;
            for (int x = ox; x < ox + side; x++)
            {
                if (x < 0 || x >= img.Width) continue;
                double dx = x - cx, dy = y - cy;
                double r2 = dx * dx + dy * dy;
                if (r2 < minR2 || r2 > maxR2) continue;
                var p = img[x, y];
                int luma = (p.R * 299 + p.G * 587 + p.B * 114) / 1000;
                if (luma < bestLuma)
                {
                    bestLuma = luma;
                    bestX = x;
                    bestY = y;
                }
            }
        }
        return (bestX, bestY, bestLuma);
    }
}
