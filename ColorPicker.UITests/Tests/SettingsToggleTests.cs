using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class SettingsToggleTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public SettingsToggleTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void ShowAlphaSwitch_Toggles()
    {
        var sw = _fx.Page.ShowAlphaSwitch;
        var initial = _fx.Page.IsToggleOn("ShowAlphaSwitch");

        _fx.Page.Toggle(sw);
        Assert.NotEqual(initial, _fx.Page.IsToggleOn("ShowAlphaSwitch"));

        _fx.Page.Toggle(sw);
        Assert.Equal(initial, _fx.Page.IsToggleOn("ShowAlphaSwitch"));
    }

    /// <summary>
    /// Toggling ShowAlphaSwitch must not just flip the switch UI — it must also
    /// change what's rendered inside the wheel host (an alpha ring/slider
    /// becomes visible). Catches regressions where the binding to
    /// <c>ColorWheel.ShowAlphaSlider</c> is broken or removed.
    /// </summary>
    [Fact]
    public void ShowAlphaSwitch_ChangesWheelRendering()
    {
        var sw = _fx.Page.ShowAlphaSwitch;
        var startedOn = _fx.Page.IsToggleOn("ShowAlphaSwitch");
        try
        {
            // Get to a known state: alpha OFF.
            if (startedOn) { _fx.Page.Toggle(sw); Thread.Sleep(250); }

            var wheelOff = _fx.Page.ColorWheel;
            var locOff   = wheelOff.Location;
            var sizeOff  = wheelOff.Size;
            using var imgOff = LoadPng(_fx.Page.CaptureWindowBytes());

            // Toggle alpha ON and let the SkiaSharp surface redraw.
            _fx.Page.Toggle(sw);
            Thread.Sleep(400);

            var wheelOn = _fx.Page.ColorWheel;
            var locOn   = wheelOn.Location;
            var sizeOn  = wheelOn.Size;
            using var imgOn = LoadPng(_fx.Page.CaptureWindowBytes());

            // Crop to the intersection of the two host rectangles (the host
            // doesn't move when the alpha flag flips, but we guard against any
            // 1-2px reflow).
            int x = Math.Max(locOff.X, locOn.X);
            int y = Math.Max(locOff.Y, locOn.Y);
            int w = Math.Min(locOff.X + sizeOff.Width,  locOn.X + sizeOn.Width)  - x;
            int h = Math.Min(locOff.Y + sizeOff.Height, locOn.Y + sizeOn.Height) - y;
            Assert.True(w > 50 && h > 50, "Wheel host bounds too small to compare.");

            int diff = CountDifferentPixels(imgOff, imgOn, x, y, w, h, channelTol: 16);
            int total = w * h;
            double frac = (double)diff / total;

            // The alpha ring/slider occupies a noticeable portion of the wheel
            // surface; require at least 1% of pixels to differ. Empirical
            // observation: the binding-driven flag flips ~5-15% of host pixels.
            Assert.True(frac > 0.01,
                $"ShowAlphaSwitch toggle changed only {diff}/{total} ({frac:P2}) " +
                $"pixels in the wheel host. The switch state flips, but the " +
                $"binding to ColorWheel.ShowAlphaSlider appears to be broken.");
        }
        finally
        {
            // Restore.
            if (_fx.Page.IsToggleOn("ShowAlphaSwitch") != startedOn)
                _fx.Page.Toggle(sw);
        }
    }

    private static PixelImage LoadPng(byte[] bytes) => PixelImage.Load(bytes);

    private static int CountDifferentPixels(
        PixelImage a,
        PixelImage b,
        int x, int y, int w, int h, int channelTol)
    {
        int diff = 0;
        int xMax = Math.Min(x + w, Math.Min(a.Width,  b.Width));
        int yMax = Math.Min(y + h, Math.Min(a.Height, b.Height));
        for (int j = Math.Max(0, y); j < yMax; j++)
        for (int i = Math.Max(0, x); i < xMax; i++)
        {
            var pa = a[i, j]; var pb = b[i, j];
            if (Math.Abs(pa.R - pb.R) > channelTol ||
                Math.Abs(pa.G - pb.G) > channelTol ||
                Math.Abs(pa.B - pb.B) > channelTol)
                diff++;
        }
        return diff;
    }

    [Fact]
    public void ShowTriangleSwitch_SwapsWheelForTriangle()
    {
        var sw = _fx.Page.ShowTriangleSwitch;
        var wasOn = _fx.Page.IsToggleOn(sw);
        try
        {
            if (!wasOn) _fx.Page.Toggle(sw);

            // Wait for the triangle host to become visible.
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var triangle = _fx.Page.ColorTriangle;
                    Assert.True(triangle.Size.Width > 50, "Triangle should be visible & sized.");
                    return;
                }
                catch { Thread.Sleep(200); }
            }
            Assert.Fail("ColorTriangle host never appeared after toggling ShowTriangleSwitch.");
        }
        finally
        {
            if (!wasOn && _fx.Page.IsToggleOn(sw)) _fx.Page.Toggle(sw);
        }
    }
}
