namespace ColorPicker.Core.Tests;

public class SaturationValueTriangleTests
{
    // Cross-validation: for many (s, v, hue, rotate) combinations the Core
    // unit-square implementation must agree with the MAUI pixel-space
    // reference within float precision.

    public static IEnumerable<object[]> EncodeCases()
    {
        double[] svs = { 0.0, 0.1, 0.3, 0.5, 0.7, 0.9, 1.0 };
        double[] hues = { 0.0, 0.13, 0.25, 0.5, 0.77, 0.99 };
        foreach (var rot in new[] { true, false })
        foreach (var h in hues)
        foreach (var s in svs)
        foreach (var v in svs)
            yield return new object[] { s, v, h, rot };
    }

    [Theory]
    [MemberData(nameof(EncodeCases))]
    public void Encode_MatchesMauiPixelReference(double s, double v, double hue, bool rotate)
    {
        var tri = new SaturationValueTriangle(rotateByHue: rotate);
        var hsv = new HsvaColor(hue, s, v, 1.0);
        var hsl = ColorConversions.HsvToHsl(hsv);

        var corePoint = tri.ColorToPoint(hsl);

        // Reference: use a synthetic pixel canvas where SvRadius = canvasRadius,
        // i.e. the triangle exactly fills the unit square (canvasRadius = 0.5).
        var (refX, refY) = MauiTriangleReference.EncodeSvPixel(
            s, v, hue, canvasRadius: 0.5f, svRadius: 0.5f, rotateByHue: rotate);

        Assert.Equal(refX, corePoint.X, 5e-5);
        Assert.Equal(refY, corePoint.Y, 5e-5);
    }

    [Theory]
    [MemberData(nameof(EncodeCases))]
    public void Decode_MatchesMauiPixelReference(double s, double v, double hue, bool rotate)
    {
        var tri = new SaturationValueTriangle(rotateByHue: rotate);

        // Encode via reference to get a known-good pixel point, then decode
        // both ways and compare.
        var (px, py) = MauiTriangleReference.EncodeSvPixel(
            s, v, hue, canvasRadius: 0.5f, svRadius: 0.5f, rotateByHue: rotate);

        var hsv = new HsvaColor(hue, s, v, 1.0);
        var hsl = ColorConversions.HsvToHsl(hsv);
        var coreNew = tri.UpdateColor(new UnitPoint(px, py), hsl);
        var coreHsv = ColorConversions.HslToHsv(coreNew);

        var (refS, refV) = MauiTriangleReference.DecodeSvPixel(
            px, py, hue, canvasRadius: 0.5f, svRadius: 0.5f, rotateByHue: rotate);

        Assert.Equal(refS, coreHsv.S, 1e-4);
        Assert.Equal(refV, coreHsv.V, 1e-4);
        Assert.Equal(hue, coreHsv.H, 1e-6); // hue preserved
    }

    [Theory]
    [InlineData(0.0,  1.0)]
    [InlineData(0.5,  1.0)]
    [InlineData(1.0,  1.0)]
    [InlineData(0.25, 0.6)]
    [InlineData(0.99, 0.99)]
    public void RoundTrip_PreservesSV(double s, double v)
    {
        var tri = new SaturationValueTriangle(rotateByHue: true);
        var orig = ColorConversions.HsvToHsl(new HsvaColor(0.3, s, v, 0.7));
        var p = tri.ColorToPoint(orig);
        var back = tri.UpdateColor(p, orig);
        var backHsv = ColorConversions.HslToHsv(back);
        Assert.Equal(s, backHsv.S, 1e-4);
        Assert.Equal(v, backHsv.V, 1e-4);
        Assert.Equal(0.3, backHsv.H, 1e-6);
        Assert.Equal(orig.A, back.A);
    }

    [Fact]
    public void IsInActiveArea_CenterIsActive()
    {
        var tri = new SaturationValueTriangle();
        Assert.True(tri.IsInActiveArea(new UnitPoint(0.5f, 0.5f), default));
    }

    [Fact]
    public void IsInActiveArea_CornerIsNotActive()
    {
        var tri = new SaturationValueTriangle();
        Assert.False(tri.IsInActiveArea(new UnitPoint(0f, 0f), default));
    }

    [Fact]
    public void FitToActiveArea_ProjectsExteriorOntoBoundingDisc()
    {
        var tri = new SaturationValueTriangle();
        var f = tri.FitToActiveArea(new UnitPoint(0f, 0f), default);
        Assert.Equal(0.5f, f.ToCentered().ToPolar().Radius, 1e-5);
    }
}
