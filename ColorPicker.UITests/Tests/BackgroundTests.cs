using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// (e) Background / transparency tests.
///
/// The Skia canvases used by ColorWheel and ColorTriangle only paint
/// inside their geometric shapes (disc / triangle). Everything else in
/// the canvas rectangle is left transparent, so the host's background
/// colour shows through at the corners. These tests prove that contract
/// end-to-end by sampling host corners after applying a known
/// <c>bg=&lt;color&gt;</c> scenario.
///
/// HslSlider and RgbSlider fill their canvas with horizontal slider
/// tracks so the host bg is not observable at the corners — they are
/// intentionally excluded.
///
/// Spec syntax: append <c>bg=&lt;color&gt;</c> to the opts list. Colours
/// can be named (red, blue, …) or hex (#RRGGBB / #AARRGGBB).
///
/// Note: <c>CanvasBackgroundColor</c> on ColorWheel/ColorTriangle paints a
/// disc-shaped backdrop INSIDE the picker (not the corners), so it isn't
/// observable from the corner samples used here. A dedicated, more
/// targeted test for that property is intentionally deferred.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class BackgroundTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public BackgroundTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("wheel:400x400:bg=red", 255, 0, 0)]
    [InlineData("wheel:400x400:bg=blue", 0, 0, 255)]
    [InlineData("wheel:400x400:bg=yellow", 255, 255, 0)]
    [InlineData("wheel:400x400:bg=black", 0, 0, 0)]
    [InlineData("triangle:400x400:bg=red", 255, 0, 0)]
    [InlineData("triangle:400x400:bg=green", 0, 128, 0)]
    public void Host_Background_Shows_Through_Canvas_Corners(
        string scenario, int r, int g, int b)
    {
        var (corner, center) = SampleCornerAndCenter(scenario);
        AssertNear(corner, new Pixel((byte)r, (byte)g, (byte)b, 255),
                   tol: 16, scenario);
        // Sanity: center sample should differ from corner (a control is
        // drawn on top of the background).
        Assert.True(Screenshot.ColorDistance(center, corner) > 30,
            $"Center {center} ≈ corner {corner} for {scenario}; nothing drawn?");
    }

    [Fact]
    public void Host_Background_Hex_Color_Is_Accepted()
    {
        // #FF8000 = pure orange. Sanity-checks the hex parser path.
        var (corner, _) = SampleCornerAndCenter("wheel:400x400:bg=#FF8000");
        AssertNear(corner, new Pixel(255, 128, 0, 255), tol: 16,
                   "wheel:400x400:bg=#FF8000");
    }

    private (Pixel corner, Pixel center) SampleCornerAndCenter(string scenario)
    {
        var page = _fixture.Page;
        page.Apply(scenario);

        using var img = page.CaptureCanvasImage();
        const int Inset = 5;
        var corner = Screenshot.SampleBox(img, Inset, Inset);
        var center = Screenshot.SampleBox(img, img.Width / 2, img.Height / 2);
        return (corner, center);
    }

    private static void AssertNear(Pixel actual, Pixel expected, int tol, string scenario)
    {
        int d = Screenshot.ColorDistance(actual, expected);
        Assert.True(d <= tol,
            $"Color {actual} not near expected {expected} (Δ={d}, tol={tol}, scenario={scenario})");
    }
}
