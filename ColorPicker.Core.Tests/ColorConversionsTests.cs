namespace ColorPicker.Core.Tests;

public class ColorConversionsTests
{
    const double Precision = 1e-9;
    const double RgbTolerance = 1e-6;

    public static IEnumerable<object[]> RgbHslSamples => new[]
    {
        // r, g, b, h, s, l
        new object[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },     // black
        new object[] { 1.0, 1.0, 1.0, 0.0, 0.0, 1.0 },     // white
        new object[] { 0.5, 0.5, 0.5, 0.0, 0.0, 0.5 },     // gray
        new object[] { 1.0, 0.0, 0.0, 0.0,       1.0, 0.5 }, // red
        new object[] { 0.0, 1.0, 0.0, 1.0 / 3.0, 1.0, 0.5 }, // green
        new object[] { 0.0, 0.0, 1.0, 2.0 / 3.0, 1.0, 0.5 }, // blue
        new object[] { 1.0, 1.0, 0.0, 1.0 / 6.0, 1.0, 0.5 }, // yellow
        new object[] { 0.0, 1.0, 1.0, 0.5,       1.0, 0.5 }, // cyan
        new object[] { 1.0, 0.0, 1.0, 5.0 / 6.0, 1.0, 0.5 }, // magenta
    };

    [Theory]
    [MemberData(nameof(RgbHslSamples))]
    public void RgbToHsl_KnownColors(double r, double g, double b, double h, double s, double l)
    {
        var hsl = ColorConversions.RgbToHsl(new RgbaColor(r, g, b));
        Assert.Equal(h, hsl.H, Precision);
        Assert.Equal(s, hsl.S, Precision);
        Assert.Equal(l, hsl.L, Precision);
    }

    [Theory]
    [MemberData(nameof(RgbHslSamples))]
    public void HslToRgb_KnownColors(double r, double g, double b, double h, double s, double l)
    {
        var rgb = ColorConversions.HslToRgb(new HslaColor(h, s, l));
        Assert.Equal(r, rgb.R, RgbTolerance);
        Assert.Equal(g, rgb.G, RgbTolerance);
        Assert.Equal(b, rgb.B, RgbTolerance);
    }

    [Theory]
    [InlineData(0.0, 0.0, 0.0)]
    [InlineData(0.1, 0.5, 0.2)]
    [InlineData(0.7, 0.3, 0.4)]
    [InlineData(0.99, 0.5, 0.6)]
    public void HslRgbRoundTrip_PreservesValues(double h, double s, double l)
    {
        var orig = new HslaColor(h, s, l, 0.8);
        var rt = orig.ToRgba().ToHsla();
        // Hue is meaningless when saturation collapses (it shouldn't for these inputs)
        Assert.Equal(orig.H, rt.H, RgbTolerance);
        Assert.Equal(orig.S, rt.S, RgbTolerance);
        Assert.Equal(orig.L, rt.L, RgbTolerance);
        Assert.Equal(orig.A, rt.A);
    }

    [Theory]
    [InlineData(0.0, 0.0, 0.0)]
    [InlineData(0.1, 0.5, 0.2)]
    [InlineData(0.7, 0.3, 0.9)]
    [InlineData(0.99, 1.0, 0.5)]
    public void HslHsvRoundTrip_PreservesValues(double h, double s, double l)
    {
        var orig = new HslaColor(h, s, l);
        var rt = orig.ToHsva().ToHsla();
        Assert.Equal(orig.H, rt.H, RgbTolerance);
        Assert.Equal(orig.S, rt.S, RgbTolerance);
        Assert.Equal(orig.L, rt.L, RgbTolerance);
    }

    [Fact]
    public void HslToRgb_NegativeHue_WrapsAround()
    {
        var a = ColorConversions.HslToRgb(new HslaColor(-0.25, 1, 0.5));
        var b = ColorConversions.HslToRgb(new HslaColor(0.75, 1, 0.5));
        Assert.Equal(a.R, b.R, RgbTolerance);
        Assert.Equal(a.G, b.G, RgbTolerance);
        Assert.Equal(a.B, b.B, RgbTolerance);
    }

    [Fact]
    public void HslToRgb_HueOver1_WrapsAround()
    {
        var a = ColorConversions.HslToRgb(new HslaColor(1.25, 1, 0.5));
        var b = ColorConversions.HslToRgb(new HslaColor(0.25, 1, 0.5));
        Assert.Equal(a.R, b.R, RgbTolerance);
        Assert.Equal(a.G, b.G, RgbTolerance);
        Assert.Equal(a.B, b.B, RgbTolerance);
    }

    [Fact]
    public void HsvToRgb_AllSaturationZero_IsGrayscale()
    {
        var rgb = ColorConversions.HsvToRgb(new HsvaColor(0.7, 0, 0.4));
        Assert.Equal(0.4, rgb.R, RgbTolerance);
        Assert.Equal(0.4, rgb.G, RgbTolerance);
        Assert.Equal(0.4, rgb.B, RgbTolerance);
    }

    [Fact]
    public void AlphaIsPreservedThroughAllConversions()
    {
        var hsl = new HslaColor(0.5, 0.5, 0.5, 0.42);
        Assert.Equal(0.42, hsl.ToRgba().A);
        Assert.Equal(0.42, hsl.ToHsva().A);
        Assert.Equal(0.42, hsl.ToRgba().ToHsla().A);
    }
}
