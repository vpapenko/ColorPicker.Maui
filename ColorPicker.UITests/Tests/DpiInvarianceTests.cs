using System.Runtime.InteropServices;
using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// (c) DPI / scale invariance.
///
/// MAUI reports layout in DPI-independent logical units. Windows then scales
/// to physical pixels per the monitor's DPI (96 = 100%, 120 = 125%, etc.).
/// This test takes an element whose logical width is known (ScenarioEntry,
/// WidthRequest = 320 in LayoutTestPage.xaml), reads its physical pixel
/// width via UIA, and asserts the ratio matches the system-reported DPI.
///
/// Catches regressions where the harness or the control mishandles DPI
/// scaling (e.g. requesting pixels instead of logical units, or the canvas
/// being sized with mismatched units).
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class DpiInvarianceTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private const int LogicalEntryWidth = 320;       // matches LayoutTestPage.xaml
    private const double Tol             = 1.5;       // px

    private readonly LayoutTestAppFixture _fixture;
    public DpiInvarianceTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    [Fact]
    public void Entry_Pixel_Width_Matches_LogicalWidth_Times_DpiScale()
    {
        var page  = _fixture.Page;
        page.Apply("wheel:400x400");

        var entry = page.ScenarioEntry;
        var pixelW = entry.Size.Width;

        var dpi   = GetDpiForWindow(_fixture.AppHwnd);
        Assert.True(dpi > 0, $"GetDpiForWindow returned {dpi}");
        var scale = dpi / 96.0;
        var expected = LogicalEntryWidth * scale;

        Assert.True(Math.Abs(pixelW - expected) <= Tol,
            $"Entry pixel width {pixelW} != logical {LogicalEntryWidth} * scale {scale:0.###} = {expected:0.##} (DPI={dpi})");
    }

    [Fact]
    public void Logical_Bounds_Are_Independent_Of_Pixel_DPI()
    {
        // Sanity: applying the same scenario twice must produce the same
        // logical bounds, regardless of any pixel-level rounding.
        var page = _fixture.Page;
        var a = page.Apply("wheel:400x400");
        var b = page.Apply("wheel:400x400");

        Assert.Equal(a.HostBounds.W, b.HostBounds.W, 1);
        Assert.Equal(a.HostBounds.H, b.HostBounds.H, 1);
        // And the marker reports them in logical units (== requested),
        // which holds regardless of monitor DPI.
        Assert.True(Math.Abs(a.HostBounds.W - 400) <= 1);
        Assert.True(Math.Abs(a.HostBounds.H - 400) <= 1);
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);
}
