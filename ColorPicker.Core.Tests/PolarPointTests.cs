namespace ColorPicker.Core.Tests;

public class PolarPointTests
{
    const float Precision = 1e-6f;

    [Fact]
    public void Constructor_StoresRadius_AndNormalizesAngleToCanonicalRange()
    {
        var p = new PolarPoint(0, 0);
        Assert.Equal(0, p.Radius);
        Assert.Equal(0, p.Angle);
    }

    [Theory]
    [InlineData(1f, 0f, 0f)]                  // +X axis
    [InlineData(1f, (float)Math.PI, (float)Math.PI)]    // -X axis (±π collapses to π or -π)
    [InlineData(1f, 2 * (float)Math.PI, 0f)]            // wraps to 0
    [InlineData(1f, 3 * (float)Math.PI, (float)Math.PI)] // wraps to π
    public void Constructor_NormalizesAngle(float r, float angle, float expectedAbsAngle)
    {
        var p = new PolarPoint(r, angle);
        Assert.Equal(r, p.Radius);
        Assert.Equal(expectedAbsAngle, Math.Abs(p.Angle), Precision);
    }

    [Fact]
    public void FromCartesian_Origin_RadiusZero()
    {
        var p = PolarPoint.FromCartesian(0, 0);
        Assert.Equal(0, p.Radius);
        Assert.Equal(0, p.Angle);
    }

    [Theory]
    [InlineData(1f, 0f, 1f, 0f)]
    [InlineData(0f, 1f, 1f, (float)(Math.PI / 2))]
    [InlineData(0f, -1f, 1f, -(float)(Math.PI / 2))]
    [InlineData(-1f, 0f, 1f, (float)Math.PI)]
    public void FromCartesian_KnownPoints(float x, float y, float expectedR, float expectedA)
    {
        var p = PolarPoint.FromCartesian(x, y);
        Assert.Equal(expectedR, p.Radius, Precision);
        Assert.Equal(Math.Abs(expectedA), Math.Abs(p.Angle), Precision);
    }

    [Fact]
    public void ToCartesian_AndBack_RoundTrips()
    {
        var input = new UnitPoint(0.3f, -0.4f);
        var polar = input.ToPolar();
        var back = polar.ToCartesian();
        Assert.Equal(input.X, back.X, Precision);
        Assert.Equal(input.Y, back.Y, Precision);
    }

    [Fact]
    public void AddAngle_NormalizesResult()
    {
        var p = new PolarPoint(1, (float)Math.PI).AddAngle((float)Math.PI);
        Assert.Equal(0, p.Angle, Precision);
    }

    [Fact]
    public void WithMethods_AreImmutable()
    {
        var a = new PolarPoint(1, 0);
        var b = a.WithRadius(2);
        Assert.Equal(1, a.Radius);
        Assert.Equal(2, b.Radius);
    }

    [Fact]
    public void Equality_StructuralAndOperator()
    {
        var a = new PolarPoint(1, 0);
        var b = new PolarPoint(1, 0);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
