namespace ColorPicker.Core.Tests;

public class LinearTrackTests
{
    [Theory]
    [InlineData(false, 0.0f, 0.5f, 0.0)]
    [InlineData(false, 0.5f, 0.5f, 0.5)]
    [InlineData(false, 1.0f, 0.5f, 1.0)]
    [InlineData(true,  0.5f, 0.0f, 0.0)]
    [InlineData(true,  0.5f, 0.5f, 0.5)]
    [InlineData(true,  0.5f, 1.0f, 1.0)]
    public void ValueAt_KnownPositions(bool vertical, float x, float y, double expected)
    {
        var t = new LinearTrack(vertical);
        Assert.Equal(expected, t.ValueAt(new UnitPoint(x, y)), 1e-6);
    }

    [Theory]
    [InlineData(false, 0.0, 0.0f, 0.5f)]
    [InlineData(false, 0.5, 0.5f, 0.5f)]
    [InlineData(false, 1.0, 1.0f, 0.5f)]
    [InlineData(true,  0.5, 0.5f, 0.5f)]
    public void PointFor_KnownValues(bool vertical, double v, float ex, float ey)
    {
        var t = new LinearTrack(vertical);
        var p = t.PointFor(v);
        Assert.Equal(ex, p.X, 1e-5);
        Assert.Equal(ey, p.Y, 1e-5);
    }

    [Theory]
    [InlineData(-1.0, 0.0)]
    [InlineData( 0.5, 0.5)]
    [InlineData( 2.0, 1.0)]
    public void PointFor_ClampsOutOfRange(double v, double expected)
    {
        var t = new LinearTrack(false);
        Assert.Equal(expected, t.PointFor(v).X, 1e-6);
    }

    [Theory]
    [InlineData(-0.5f, 0.0)]
    [InlineData( 1.5f, 1.0)]
    public void ValueAt_ClampsOutOfRange(float x, double expected)
    {
        var t = new LinearTrack(false);
        Assert.Equal(expected, t.ValueAt(new UnitPoint(x, 0.5f)), 1e-6);
    }
}
