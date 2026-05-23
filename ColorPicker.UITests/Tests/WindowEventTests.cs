using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 5 — WindowEventTests.
///
/// Validates that the picker layout responds correctly to window-level
/// events (resize, maximize, restore). Two invariants:
///   • Fill-mode hosts track the viewport when the window changes size.
///   • Fixed-mode hosts retain their requested size regardless of window
///     size (provided the window is still large enough to hold them).
///   • In all cases the picker control stays inside its host.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class WindowEventTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public WindowEventTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    [Fact]
    public void FillHost_Tracks_Window_Resize()
    {
        var page   = _fixture.Page;
        var window = _fixture.Driver.Manage().Window;

        page.Apply("wheel:fillxfill");

        // Shrink the window — viewport and host should both shrink.
        window.Size = new System.Drawing.Size(900, 700);
        var small = page.WaitForState(s =>
            Math.Abs(s.HostBounds.W - s.ViewportBounds.W) <= 2 &&
            s.ViewportBounds.W < 900);

        // Grow it back — viewport and host should both grow.
        window.Size = new System.Drawing.Size(1400, 900);
        var big   = page.WaitForState(s =>
            Math.Abs(s.HostBounds.W - s.ViewportBounds.W) <= 2 &&
            s.ViewportBounds.W > small.ViewportBounds.W);

        Assert.True(big.HostBounds.W > small.HostBounds.W,
            $"Host did not grow: small={small.HostBounds.W}, big={big.HostBounds.W}");
        Assert.True(big.ControlBounds.W > 0 && big.ControlBounds.W <= big.HostBounds.W + 1,
            $"Control did not adapt to new host size: {big.ControlBounds.W} / {big.HostBounds.W}");
    }

    [Fact]
    public void FixedHost_Survives_Window_Resize()
    {
        var page   = _fixture.Page;
        var window = _fixture.Driver.Manage().Window;

        // Start big enough to hold a 400×400 host.
        window.Size = new System.Drawing.Size(1400, 900);
        var initial = page.Apply("wheel:400x400");
        Assert.True(Math.Abs(initial.HostBounds.W - 400) <= 1);

        // Shrink the window — fixed host should keep its 400×400 size.
        window.Size = new System.Drawing.Size(900, 700);
        var resized = page.WaitForState(s =>
            s.ViewportBounds.W > 0 && s.ViewportBounds.W < 900,
            TimeSpan.FromSeconds(5));

        Assert.True(Math.Abs(resized.HostBounds.W - 400) <= 1,
            $"Fixed host W shifted on resize: {resized.HostBounds.W}");
        Assert.True(Math.Abs(resized.HostBounds.H - 400) <= 1,
            $"Fixed host H shifted on resize: {resized.HostBounds.H}");
    }

    [Fact]
    public void Maximize_Then_Restore_Roundtrip_Keeps_Layout_Consistent()
    {
        var page   = _fixture.Page;
        var window = _fixture.Driver.Manage().Window;

        // Start from a known restored size.
        window.Size = new System.Drawing.Size(1200, 800);
        page.Apply("wheel:fillxfill");
        var before = page.WaitForState(s =>
            Math.Abs(s.HostBounds.W - s.ViewportBounds.W) <= 2);

        // Maximize and assert host followed the viewport.
        window.Maximize();
        var maxed = page.WaitForState(s =>
            s.ViewportBounds.W > before.ViewportBounds.W &&
            Math.Abs(s.HostBounds.W - s.ViewportBounds.W) <= 2);
        Assert.True(maxed.HostBounds.W > before.HostBounds.W);

        // Restore to a smaller size and assert host shrank again.
        window.Size = new System.Drawing.Size(1100, 750);
        var restored = page.WaitForState(s =>
            s.ViewportBounds.W < maxed.ViewportBounds.W &&
            Math.Abs(s.HostBounds.W - s.ViewportBounds.W) <= 2);
        Assert.True(restored.HostBounds.W < maxed.HostBounds.W);
        Assert.True(restored.ControlBounds.W > 0 && restored.ControlBounds.W <= restored.HostBounds.W + 1);
    }
}
