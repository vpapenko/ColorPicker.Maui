using System.Runtime.InteropServices;
using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// (d) Padding / borders tests.
///
/// Validates that disc-shaped pickers (ColorWheel, ColorTriangle) leave
/// their host's four geometric corners untouched. A square host that
/// inscribes a circle MUST have all four corners showing the page's
/// background colour (white), and a colored centre. Catches regressions
/// where:
///   - The disc is sized larger than the inscribed circle (paints into
///     corners).
///   - The disc fails to render at all.
///   - The disc is offset from centre.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class PaddingTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public PaddingTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    private static readonly Pixel White = new(255, 255, 255, 255);

    [Theory]
    [InlineData("wheel:300x300:bg=white")]
    [InlineData("wheel:400x400:bg=white")]
    [InlineData("wheel:600x600:bg=white")]
    [InlineData("triangle:300x300:bg=white")]
    [InlineData("triangle:400x400:bg=white")]
    public void Disc_Pickers_Leave_Host_Corners_Empty(string scenario)
    {
        var page = _fixture.Page;
        page.Apply(scenario);

        using var img = page.CaptureCanvasImage();

        // HostBorder is anchored top-left at (0,0) of the captured image and
        // sized to the scenario's W×H. Sample within that region — sampling
        // the whole image would hit empty page area beyond the cell.
        var (cellW, cellH) = ParseSize(scenario);
        // Captured image is in physical pixels. Scale logical cell size by DPI.
        double scale = GetDpiForWindow(_fixture.AppHwnd) / 96.0;
        int pxW = (int)(cellW * scale);
        int pxH = (int)(cellH * scale);

        // Inset 5 px from each corner so we don't sample exactly at the
        // host edge (where anti-aliasing of the disc may bleed).
        const int Inset = 5;
        const int Tol   = 8; // sum-of-channel-deltas vs reference white
        var tl = Screenshot.SampleBox(img, Inset,            Inset);
        var tr = Screenshot.SampleBox(img, pxW - 1 - Inset,  Inset);
        var bl = Screenshot.SampleBox(img, Inset,            pxH - 1 - Inset);
        var br = Screenshot.SampleBox(img, pxW - 1 - Inset,  pxH - 1 - Inset);

        Assert.True(Screenshot.ColorDistance(tl, White) <= Tol, $"TL not white: {tl} ({scenario})");
        Assert.True(Screenshot.ColorDistance(tr, White) <= Tol, $"TR not white: {tr} ({scenario})");
        Assert.True(Screenshot.ColorDistance(bl, White) <= Tol, $"BL not white: {bl} ({scenario})");
        Assert.True(Screenshot.ColorDistance(br, White) <= Tol, $"BR not white: {br} ({scenario})");

        var center = Screenshot.SampleBox(img, pxW / 2, pxH / 2);
        Assert.True(Screenshot.ColorDistance(center, White) > 30,
            $"Center is ~white ({center}); disc didn't render ({scenario})");
    }

    [DllImport("user32.dll")] private static extern int GetDpiForWindow(System.IntPtr hwnd);

    private static (int W, int H) ParseSize(string scenario)
    {
        // scenario format: "<type>:<W>x<H>[:opts...]"
        var parts = scenario.Split(':');
        var size = parts[1].Split('x');
        return (int.Parse(size[0]), int.Parse(size[1]));
    }
}
