using System.Runtime.InteropServices;
using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 6 — Layer 2 golden-image regression tests.
///
/// Each scenario captures the host rectangle, crops it from the desktop
/// screenshot, and compares against a committed reference PNG (per DPI).
/// Catches subtle rendering regressions (gradient direction, picker dot
/// position, anti-aliasing changes, blend mode shifts) that the bounds-
/// based tiers can't see.
///
/// Workflow when visuals change intentionally:
///   1. Set <c>REGEN_REFS=1</c> and run the suite.
///   2. Inspect the new PNGs under
///      <c>ColorPicker.UITests/References/dpi-{N}/</c>.
///   3. Commit them.
///
/// Tolerance: per-pixel sum-of-RGB diff &lt;= 30 counts as "match";
/// up to 1.5% of pixels may exceed that (covers edge AA + cursor drift).
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class Tier6_GoldenImageTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public Tier6_GoldenImageTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    [DllImport("user32.dll")] private static extern int GetDpiForWindow(IntPtr hwnd);

    // perPixelTol: sum-of-RGB diff per pixel that counts as a "match".
    // maxBadFrac:  fraction of pixels allowed to exceed perPixelTol.
    [Theory]
    [InlineData("wheel-400-default",         "wheel:400x400",                  30, 0.015)]
    [InlineData("wheel-400-alpha",           "wheel:400x400:alpha",            30, 0.015)]
    [InlineData("wheel-400-vertical-alpha",  "wheel:400x400:alpha,vertical",   30, 0.015)]
    [InlineData("triangle-400-default",      "triangle:400x400",               30, 0.015)]
    [InlineData("wheel-400-bg-red",          "wheel:400x400:bg=red",           30, 0.015)]
    [InlineData("wheel-400-nolumwheel",      "wheel:400x400:nolumwheel",       30, 0.015)]
    public void Matches_Reference(string id, string scenario, int perPixelTol, double maxBadFrac)
    {
        var page = _fixture.Page;
        page.Apply(scenario);

        using var crop = page.CaptureCanvasImage();

        // References are keyed by DPI; canvas-capture output is logical
        // pixels × the runtime DPI scale, so the dpi key here remains valid
        // (different DPI → different reference file).
        int dpi = GetDpiForWindow(_fixture.AppHwnd);
        var refPath = ReferenceImage.ResolvePath(dpi, id);

        if (ReferenceImage.RegenRequested || !File.Exists(refPath))
        {
            ReferenceImage.Save(crop, refPath);
            return;
        }

        using var golden = ReferenceImage.Load(refPath);
        if (golden.Width != crop.Width || golden.Height != crop.Height)
        {
            int w = Math.Min(golden.Width,  crop.Width);
            int h = Math.Min(golden.Height, crop.Height);
            using var goldenAdj = ReferenceImage.Crop(golden, new PixelRect(0, 0, w, h, 1));
            using var cropAdj   = ReferenceImage.Crop(crop,   new PixelRect(0, 0, w, h, 1));
            AssertWithin(goldenAdj, cropAdj, perPixelTol, maxBadFrac, id, refPath);
            return;
        }
        AssertWithin(golden, crop, perPixelTol, maxBadFrac, id, refPath);
    }

    private static void AssertWithin(
        Image<Rgba32> golden, Image<Rgba32> actual,
        int perPixelTol, double maxBadFrac, string id, string refPath)
    {
        double bad = ReferenceImage.FractionMismatched(golden, actual, perPixelTol);
        Assert.True(bad <= maxBadFrac,
            $"Golden mismatch for '{id}': {bad:P2} of pixels exceed Δ={perPixelTol} " +
            $"(allowed: {maxBadFrac:P2}). Reference: {refPath}. " +
            $"Re-run with {ReferenceImage.RegenEnvVar}=1 to regenerate if change is intentional.");
    }
}
