namespace ColorPicker.Core.Tests;

public class HslChannelSlidersTests
{
    static readonly HslaColor Base = new(0.25, 0.6, 0.4, 0.8);

    [Fact]
    public void HueSlider_RoundTrip()
    {
        var s = new HueSlider();
        var p = s.ColorToPoint(Base);
        Assert.Equal(0.25f, p.X, 1e-5);
        Assert.Equal(0.5f,  p.Y, 1e-5);

        var c = s.UpdateColor(new UnitPoint(0.75f, 0.5f), Base);
        Assert.Equal(0.75, c.H, 1e-5);
        Assert.Equal(Base.S, c.S);
        Assert.Equal(Base.L, c.L);
        Assert.Equal(Base.A, c.A);
    }

    [Fact]
    public void SaturationSlider_ReadsSaturation()
    {
        var s = new SaturationSlider();
        Assert.Equal(0.6f, s.ColorToPoint(Base).X, 1e-5);
        var c = s.UpdateColor(new UnitPoint(0.2f, 0.5f), Base);
        Assert.Equal(0.2, c.S, 1e-5);
        Assert.Equal(Base.H, c.H);
    }

    [Fact]
    public void LuminositySlider_ReadsLuminosity()
    {
        var s = new LuminositySlider();
        Assert.Equal(0.4f, s.ColorToPoint(Base).X, 1e-5);
        var c = s.UpdateColor(new UnitPoint(0.9f, 0.5f), Base);
        Assert.Equal(0.9, c.L, 1e-5);
    }

    [Fact]
    public void AlphaSlider_ReadsAlpha()
    {
        var s = new AlphaSlider();
        Assert.Equal(0.8f, s.ColorToPoint(Base).X, 1e-5);
        var c = s.UpdateColor(new UnitPoint(0.1f, 0.5f), Base);
        Assert.Equal(0.1, c.A, 1e-5);
    }

    [Fact]
    public void VerticalSlider_UsesYAxis()
    {
        var s = new HueSlider(vertical: true);
        var p = s.ColorToPoint(Base);
        Assert.Equal(0.5f,  p.X, 1e-5);
        Assert.Equal(0.25f, p.Y, 1e-5);
        var c = s.UpdateColor(new UnitPoint(0.5f, 0.7f), Base);
        Assert.Equal(0.7, c.H, 1e-5);
    }

    [Fact]
    public void FitToActiveArea_SnapsCrossAxisToCenter()
    {
        var s = new HueSlider();
        var p = s.FitToActiveArea(new UnitPoint(0.3f, 0.9f), Base);
        Assert.Equal(0.3f, p.X, 1e-5);
        Assert.Equal(0.5f, p.Y, 1e-5);
    }

    [Fact]
    public void IsInActiveArea_AlwaysTrue()
    {
        var s = new HueSlider();
        Assert.True(s.IsInActiveArea(new UnitPoint(0f, 0f), Base));
        Assert.True(s.IsInActiveArea(new UnitPoint(2f, -3f), Base));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.33)]
    [InlineData(0.67)]
    [InlineData(1.0)]
    public void AllChannelSliders_RoundTrip(double v)
    {
        var c = new HslaColor(v, v, v, v);
        foreach (var s in new HslChannelSlider[]
        {
            new HueSlider(), new SaturationSlider(),
            new LuminositySlider(), new AlphaSlider(),
        })
        {
            var p = s.ColorToPoint(c);
            var back = s.UpdateColor(p, c);
            // each slider mutates its own channel; the round-trip leaves
            // every other channel untouched, so back == c.
            Assert.Equal(c.H, back.H, 1e-6);
            Assert.Equal(c.S, back.S, 1e-6);
            Assert.Equal(c.L, back.L, 1e-6);
            Assert.Equal(c.A, back.A, 1e-6);
        }
    }
}
