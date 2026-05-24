namespace ColorPicker.Core.Tests;

public class HueRingTests
{
    readonly HueRing _ring = new();

    [Theory]
    [InlineData(0.0,  0.0f, 0.5f)]   // hue 0   → left
    [InlineData(0.25, 0.5f, 1.0f)]   // hue 0.25→ bottom
    [InlineData(0.5,  1.0f, 0.5f)]   // hue 0.5 → right
    [InlineData(0.75, 0.5f, 0.0f)]   // hue 0.75→ top
    public void ColorToPoint_KnownHues(double hue, float ex, float ey)
    {
        var p = _ring.ColorToPoint(new HslaColor(hue, 0.5, 0.5));
        Assert.Equal(ex, p.X, 1e-5);
        Assert.Equal(ey, p.Y, 1e-5);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.13)]
    [InlineData(0.5)]
    [InlineData(0.99)]
    public void RoundTrip(double hue)
    {
        var orig = new HslaColor(hue, 0.6, 0.4, 0.7);
        var p = _ring.ColorToPoint(orig);
        var back = _ring.UpdateColor(p, orig);
        double dh = Math.Abs(orig.H - back.H);
        dh = Math.Min(dh, 1.0 - dh);
        Assert.True(dh < 1e-5);
        Assert.Equal(orig.S, back.S);
        Assert.Equal(orig.L, back.L);
        Assert.Equal(orig.A, back.A);
    }

    [Fact]
    public void IsInActiveArea_OnRing() =>
        Assert.True(_ring.IsInActiveArea(new UnitPoint(1f, 0.5f), default));

    [Fact]
    public void IsInActiveArea_AtCenter_False() =>
        Assert.False(_ring.IsInActiveArea(new UnitPoint(0.5f, 0.5f), default));

    [Fact]
    public void UpdateColor_AllAngles_ReturnHueInUnitInterval()
    {
        var c = new HslaColor(0, 0.5, 0.5);
        for (int i = 0; i < 360; i++)
        {
            double ang = i * Math.PI / 180.0;
            var p = new PolarPoint(0.4f, (float)ang).ToCartesian().FromCentered();
            var result = _ring.UpdateColor(p, c);
            Assert.InRange(result.H, 0.0, 1.0);
        }
    }
}
