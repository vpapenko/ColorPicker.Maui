namespace ColorPicker.Core.Tests;

public class RgbChannelSlidersTests
{
    [Fact]
    public void RedSlider_ReadsRedChannel()
    {
        var s = new RedSlider();
        var hsl = ColorConversions.RgbToHsl(new RgbaColor(0.4, 0.6, 0.2, 0.9));
        Assert.Equal(0.4f, s.ColorToPoint(hsl).X, 1e-4);
    }

    [Fact]
    public void GreenSlider_ReadsGreenChannel()
    {
        var s = new GreenSlider();
        var hsl = ColorConversions.RgbToHsl(new RgbaColor(0.4, 0.6, 0.2, 0.9));
        Assert.Equal(0.6f, s.ColorToPoint(hsl).X, 1e-4);
    }

    [Fact]
    public void BlueSlider_ReadsBlueChannel()
    {
        var s = new BlueSlider();
        var hsl = ColorConversions.RgbToHsl(new RgbaColor(0.4, 0.6, 0.2, 0.9));
        Assert.Equal(0.2f, s.ColorToPoint(hsl).X, 1e-4);
    }

    [Fact]
    public void UpdateColor_ChangesOnlyTargetChannelInRgbSpace()
    {
        var s = new RedSlider();
        var orig = ColorConversions.RgbToHsl(new RgbaColor(0.2, 0.6, 0.4, 0.5));
        var after = s.UpdateColor(new UnitPoint(0.8f, 0.5f), orig);
        var afterRgb = ColorConversions.HslToRgb(after);
        Assert.Equal(0.8, afterRgb.R, 1e-4);
        Assert.Equal(0.6, afterRgb.G, 1e-4);
        Assert.Equal(0.4, afterRgb.B, 1e-4);
        Assert.Equal(0.5, after.A, 1e-6);
    }

    [Fact]
    public void UpdateColor_GrayscaleResult_PreservesCallerHue()
    {
        // Start from a grayscale color but with an explicit non-zero hue
        // stored on the HslaColor (Core stores H independently of S/L).
        // Sliding R to keep it grayscale should leave hue intact rather
        // than snapping it to 0.
        var s = new RedSlider();
        var orig = new HslaColor(h: 0.42, s: 0, l: 0.5, a: 1.0);
        var after = s.UpdateColor(new UnitPoint(0.5f, 0.5f), orig);
        Assert.Equal(0.42, after.H, 1e-6);
        var afterRgb = ColorConversions.HslToRgb(after);
        Assert.Equal(0.5, afterRgb.R, 1e-4);
        Assert.Equal(0.5, afterRgb.G, 1e-4);
        Assert.Equal(0.5, afterRgb.B, 1e-4);
    }

    [Fact]
    public void Vertical_UsesYAxis()
    {
        var s = new RedSlider(vertical: true);
        var hsl = ColorConversions.RgbToHsl(new RgbaColor(0.4, 0.6, 0.2, 1.0));
        var p = s.ColorToPoint(hsl);
        Assert.Equal(0.5f,  p.X, 1e-5);
        Assert.Equal(0.4f,  p.Y, 1e-4);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void RoundTrip(double v)
    {
        foreach (var s in new RgbChannelSlider[] { new RedSlider(), new GreenSlider(), new BlueSlider() })
        {
            var rgb = new RgbaColor(0.3, 0.5, 0.7, 0.8);
            var newRgb = s switch
            {
                RedSlider   => rgb.WithR(v),
                GreenSlider => rgb.WithG(v),
                BlueSlider  => rgb.WithB(v),
                _ => throw new InvalidOperationException(),
            };
            var hsl = ColorConversions.RgbToHsl(newRgb);
            var p = s.ColorToPoint(hsl);
            var back = s.UpdateColor(p, hsl);
            var backRgb = ColorConversions.HslToRgb(back);
            Assert.Equal(newRgb.R, backRgb.R, 1e-4);
            Assert.Equal(newRgb.G, backRgb.G, 1e-4);
            Assert.Equal(newRgb.B, backRgb.B, 1e-4);
        }
    }
}
