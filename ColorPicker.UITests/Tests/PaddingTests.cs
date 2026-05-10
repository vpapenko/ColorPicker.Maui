using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

    private static readonly Rgba32 White = new(255, 255, 255, 255);

    [Theory]
    [InlineData("wheel:300x300")]
    [InlineData("wheel:400x400")]
    [InlineData("wheel:600x600")]
    [InlineData("triangle:300x300")]
    [InlineData("triangle:400x400")]
    public void Disc_Pickers_Leave_Host_Corners_Empty(string scenario)
    {
        var page  = _fixture.Page;
        var state = page.Apply(scenario);

        // Convert host (which is in viewport-local coords) to absolute logical
        // coords inside the page.
        var hostAbsolute = new LogicalBounds(
            state.ViewportBounds.X + state.HostBounds.X,
            state.ViewportBounds.Y + state.HostBounds.Y,
            state.HostBounds.W,
            state.HostBounds.H);

        var hostPx = Screenshot.ToPixels(hostAbsolute, state, page.AppliedMarker);
        using var img = Screenshot.Capture(_fixture.Driver);

        // Inset 5 px from each corner so we don't sample exactly at the
        // host border (which may have anti-aliasing artefacts).
        const int Inset  = 5;
        const int Tol    = 8; // sum-of-channel-deltas vs reference white
        var tl = Screenshot.SampleBox(img, hostPx.X      + Inset, hostPx.Y      + Inset);
        var tr = Screenshot.SampleBox(img, hostPx.Right  - Inset, hostPx.Y      + Inset);
        var bl = Screenshot.SampleBox(img, hostPx.X      + Inset, hostPx.Bottom - Inset);
        var br = Screenshot.SampleBox(img, hostPx.Right  - Inset, hostPx.Bottom - Inset);

        Assert.True(Screenshot.ColorDistance(tl, White) <= Tol, $"TL not white: {tl} ({scenario})");
        Assert.True(Screenshot.ColorDistance(tr, White) <= Tol, $"TR not white: {tr} ({scenario})");
        Assert.True(Screenshot.ColorDistance(bl, White) <= Tol, $"BL not white: {bl} ({scenario})");
        Assert.True(Screenshot.ColorDistance(br, White) <= Tol, $"BR not white: {br} ({scenario})");

        // Centre of host should be inside the disc and therefore *not* white.
        var center = Screenshot.SampleBox(img, hostPx.CenterX, hostPx.CenterY);
        Assert.True(Screenshot.ColorDistance(center, White) > 30,
            $"Center is ~white ({center}); disc didn't render ({scenario})");
    }
}
