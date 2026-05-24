namespace ColorPicker.Core.Tests;

public class HueSaturationDiscTests
{
    const double Precision = 1e-6;
    readonly HueSaturationDisc _disc = new();

    // ---- IsInActiveArea ----------------------------------------------------

    [Theory]
    [InlineData(0.5f, 0.5f, true)]   // center
    [InlineData(0.0f, 0.5f, true)]   // left edge (radius 0.5)
    [InlineData(1.0f, 0.5f, true)]   // right edge
    [InlineData(0.5f, 0.0f, true)]   // top edge
    [InlineData(0.5f, 1.0f, true)]   // bottom edge
    [InlineData(0.0f, 0.0f, false)]  // top-left corner (radius √0.5 ≈ 0.707)
    [InlineData(1.0f, 1.0f, false)]  // bottom-right corner
    public void IsInActiveArea_MatchesUnitDisc(float x, float y, bool expected)
    {
        var inside = _disc.IsInActiveArea(new UnitPoint(x, y), default);
        Assert.Equal(expected, inside);
    }

    // ---- FitToActiveArea ---------------------------------------------------

    [Theory]
    [InlineData(0.5f, 0.5f)]
    [InlineData(0.25f, 0.5f)]
    [InlineData(0.5f, 0.25f)]
    public void FitToActiveArea_LeavesInteriorUntouched(float x, float y)
    {
        var p = new UnitPoint(x, y);
        var f = _disc.FitToActiveArea(p, default);
        Assert.Equal(p.X, f.X, Precision);
        Assert.Equal(p.Y, f.Y, Precision);
    }

    [Theory]
    [InlineData(0.0f, 0.0f)] // top-left corner clamps to disc boundary along same direction
    [InlineData(1.0f, 1.0f)]
    [InlineData(1.0f, 0.0f)]
    [InlineData(0.0f, 1.0f)]
    public void FitToActiveArea_ProjectsExteriorOntoBoundary(float x, float y)
    {
        var fit = _disc.FitToActiveArea(new UnitPoint(x, y), default);
        var rOut = fit.ToCentered().ToPolar().Radius;
        Assert.Equal(0.5f, rOut, 1e-5);
    }

    // ---- ColorToPoint ------------------------------------------------------

    [Theory]
    [InlineData(0.0,  1.0, 0.0f, 0.5f)]    // hue 0   → angle π   → left  edge
    [InlineData(0.25, 1.0, 0.5f, 1.0f)]    // hue 0.25→ angle π/2 → bottom edge (screen +Y)
    [InlineData(0.5,  1.0, 1.0f, 0.5f)]    // hue 0.5 → angle 0   → right edge
    [InlineData(0.75, 1.0, 0.5f, 0.0f)]    // hue 0.75→ angle −π/2→ top    edge
    public void ColorToPoint_HueOnSaturatedBoundary(double h, double s, float expX, float expY)
    {
        var p = _disc.ColorToPoint(new HslaColor(h, s, 0.5));
        Assert.Equal(expX, p.X, 1e-5);
        Assert.Equal(expY, p.Y, 1e-5);
    }

    [Fact]
    public void ColorToPoint_ZeroSaturation_LandsAtCenter()
    {
        var p = _disc.ColorToPoint(new HslaColor(0.42, 0.0, 0.5));
        Assert.Equal(0.5f, p.X, 1e-5);
        Assert.Equal(0.5f, p.Y, 1e-5);
    }

    [Fact]
    public void ColorToPoint_HalfSaturation_LandsAtHalfRadius()
    {
        var p = _disc.ColorToPoint(new HslaColor(0.5, 0.5, 0.5));
        // hue 0.5 → angle 0 → +X axis, radius 0.25 → (0.75, 0.5)
        Assert.Equal(0.75f, p.X, 1e-5);
        Assert.Equal(0.5f,  p.Y, 1e-5);
    }

    // ---- Round trips -------------------------------------------------------

    [Theory]
    [InlineData(0.00, 1.00)]
    [InlineData(0.10, 0.80)]
    [InlineData(0.25, 0.50)]
    [InlineData(0.50, 1.00)]
    [InlineData(0.75, 0.30)]
    [InlineData(0.99, 1.00)]
    public void RoundTrip_ColorToPointToColor_PreservesHueAndSaturation(double h, double s)
    {
        var orig = new HslaColor(h, s, 0.5, 0.7);
        var p = _disc.ColorToPoint(orig);
        var back = _disc.UpdateColor(p, orig);
        // Hue wraps at 1, so compare as circular distance.
        var dh = Math.Abs(orig.H - back.H);
        dh = Math.Min(dh, 1.0 - dh);
        Assert.True(dh < 1e-5, $"hue drifted: orig={orig.H} back={back.H}");
        Assert.Equal(orig.S, back.S, 1e-5);
        // L and A are passed through unchanged
        Assert.Equal(orig.L, back.L);
        Assert.Equal(orig.A, back.A);
    }

    [Fact]
    public void UpdateColor_HueWrapsToUnitInterval()
    {
        // A point near top of disc could mathematically give a negative
        // intermediate hue from (π − angle)/(2π); ensure we wrap to [0, 1).
        var color = new HslaColor(0, 0.5, 0.5);
        for (int i = 0; i < 360; i++)
        {
            double ang = i * Math.PI / 180.0;
            var p = new PolarPoint(0.3f, (float)ang).ToCartesian().FromCentered();
            var result = _disc.UpdateColor(p, color);
            Assert.InRange(result.H, 0.0, 1.0);
            Assert.InRange(result.S, 0.0, 1.0);
        }
    }

    [Fact]
    public void UpdateColor_OutOfBoundsPoint_ClampsSaturationAtMost1()
    {
        var color = new HslaColor(0, 0.5, 0.5);
        // Way outside the unit square
        var p = new UnitPoint(2f, 2f);
        var result = _disc.UpdateColor(p, color);
        Assert.Equal(1.0, result.S, 1e-5);
    }

    [Fact]
    public void UpdateColor_PreservesLuminosityAndAlpha()
    {
        var color = new HslaColor(0.1, 0.2, 0.85, 0.4);
        var p = new UnitPoint(0.7f, 0.3f);
        var result = _disc.UpdateColor(p, color);
        Assert.Equal(color.L, result.L);
        Assert.Equal(color.A, result.A);
    }
}
