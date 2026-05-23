namespace ColorPicker.Core.Tests;

public class LuminosityRingTests
{
    readonly LuminosityRing _ring = new();

    // ---- ColorToPoint: known positions ------------------------------------

    [Fact]
    public void ColorToPoint_L0_LandsAtTop()
    {
        var p = _ring.ColorToPoint(new HslaColor(0.3, 0.5, 0.0));
        Assert.Equal(0.5f, p.X, 1e-5);
        Assert.Equal(0.0f, p.Y, 1e-5); // top edge (screen y=0)
    }

    [Fact]
    public void ColorToPoint_L1_LandsAtBottom()
    {
        var p = _ring.ColorToPoint(new HslaColor(0.3, 0.5, 1.0));
        Assert.Equal(0.5f, p.X, 1e-5);
        Assert.Equal(1.0f, p.Y, 1e-5);
    }

    [Fact]
    public void ColorToPoint_LHalf_DefaultsToRightSide()
    {
        var p = _ring.ColorToPoint(new HslaColor(0.3, 0.5, 0.5));
        Assert.Equal(1.0f, p.X, 1e-5);
        Assert.Equal(0.5f, p.Y, 1e-5);
    }

    [Fact]
    public void ColorToPoint_WithPrevPointOnLeft_StaysOnLeft()
    {
        // Previous indicator at 9 o'clock (left edge)
        var prev = new UnitPoint(0f, 0.5f);
        var p = _ring.ColorToPoint(new HslaColor(0.3, 0.5, 0.5), prev);
        Assert.Equal(0.0f, p.X, 1e-5);
        Assert.Equal(0.5f, p.Y, 1e-5);
    }

    [Fact]
    public void ColorToPoint_WithPrevPointOnRight_StaysOnRight()
    {
        var prev = new UnitPoint(1f, 0.5f);
        var p = _ring.ColorToPoint(new HslaColor(0.3, 0.5, 0.5), prev);
        Assert.Equal(1.0f, p.X, 1e-5);
        Assert.Equal(0.5f, p.Y, 1e-5);
    }

    // ---- UpdateColor: known positions -------------------------------------

    [Theory]
    [InlineData(0.5f, 0.0f, 0.0)]   // top   → L=0
    [InlineData(0.5f, 1.0f, 1.0)]   // bottom→ L=1
    [InlineData(1.0f, 0.5f, 0.5)]   // right → L=0.5
    [InlineData(0.0f, 0.5f, 0.5)]   // left  → L=0.5 (sign lost)
    public void UpdateColor_KnownPositions(float x, float y, double expL)
    {
        var c = _ring.UpdateColor(new UnitPoint(x, y), new HslaColor(0.3, 0.5, 0));
        Assert.Equal(expL, c.L, 1e-5);
    }

    [Fact]
    public void UpdateColor_PreservesHSAandWrapsLuminosity()
    {
        var orig = new HslaColor(0.42, 0.6, 0.0, 0.7);
        var c = _ring.UpdateColor(new UnitPoint(0.85f, 0.85f), orig);
        Assert.Equal(orig.H, c.H);
        Assert.Equal(orig.S, c.S);
        Assert.Equal(orig.A, c.A);
        Assert.InRange(c.L, 0.0, 1.0);
    }

    // ---- Round trips -------------------------------------------------------

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.1)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void RoundTrip_DefaultSide(double l)
    {
        var orig = new HslaColor(0.2, 0.7, l, 0.5);
        var p = _ring.ColorToPoint(orig);
        var back = _ring.UpdateColor(p, orig);
        Assert.Equal(orig.L, back.L, 1e-5);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    public void RoundTrip_WithLeftSideHint(double l)
    {
        var orig = new HslaColor(0.2, 0.7, l, 0.5);
        var p = _ring.ColorToPoint(orig, new UnitPoint(0f, 0.5f));
        Assert.True(p.X <= 0.5f, $"expected left half, got X={p.X}");
        var back = _ring.UpdateColor(p, orig);
        Assert.Equal(orig.L, back.L, 1e-5);
    }

    // ---- IsInActiveArea ---------------------------------------------------

    [Fact]
    public void IsInActiveArea_PointOnRing_IsActive()
    {
        // 3 o'clock, exactly on ring
        var p = new UnitPoint(1f, 0.5f);
        Assert.True(_ring.IsInActiveArea(p, default));
    }

    [Fact]
    public void IsInActiveArea_PointAtCenter_NotActive()
    {
        var p = new UnitPoint(0.5f, 0.5f);
        Assert.False(_ring.IsInActiveArea(p, default));
    }

    // ---- FitToActiveArea --------------------------------------------------

    [Fact]
    public void FitToActiveArea_OffRingPoint_ProjectsToRingRadius()
    {
        var p = new UnitPoint(0.75f, 0.5f); // radius 0.25
        var fit = _ring.FitToActiveArea(p, default);
        var r = fit.ToCentered().ToPolar().Radius;
        Assert.Equal(0.5f, r, 1e-5);
    }

    [Fact]
    public void FitToActiveArea_Center_DefaultsToRightSide()
    {
        var fit = _ring.FitToActiveArea(new UnitPoint(0.5f, 0.5f), default);
        Assert.Equal(1.0f, fit.X, 1e-5);
        Assert.Equal(0.5f, fit.Y, 1e-5);
    }
}
