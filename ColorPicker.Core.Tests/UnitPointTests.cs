namespace ColorPicker.Core.Tests;

public class UnitPointTests
{
    const float Precision = 1e-6f;

    [Fact]
    public void Constructor_StoresCoordinates()
    {
        var p = new UnitPoint(0.25f, 0.75f);
        Assert.Equal(0.25f, p.X);
        Assert.Equal(0.75f, p.Y);
    }

    [Fact]
    public void ToCentered_ShiftsByMinusHalf()
    {
        var p = new UnitPoint(0.5f, 0.5f).ToCentered();
        Assert.Equal(0f, p.X);
        Assert.Equal(0f, p.Y);
    }

    [Fact]
    public void FromCentered_ShiftsByPlusHalf()
    {
        var p = new UnitPoint(0f, 0f).FromCentered();
        Assert.Equal(0.5f, p.X);
        Assert.Equal(0.5f, p.Y);
    }

    [Fact]
    public void Centered_RoundTripIsIdentity()
    {
        var p = new UnitPoint(0.3f, 0.7f);
        var rt = p.ToCentered().FromCentered();
        Assert.Equal(p.X, rt.X, Precision);
        Assert.Equal(p.Y, rt.Y, Precision);
    }

    [Fact]
    public void WithMethods_AreImmutable()
    {
        var a = new UnitPoint(0.1f, 0.2f);
        var b = a.WithX(0.9f);
        Assert.Equal(0.1f, a.X);
        Assert.Equal(0.9f, b.X);
        Assert.Equal(0.2f, b.Y);
    }

    [Fact]
    public void Translate_AddsDeltas()
    {
        var p = new UnitPoint(0.1f, 0.2f).Translate(0.4f, -0.1f);
        Assert.Equal(0.5f, p.X, Precision);
        Assert.Equal(0.1f, p.Y, Precision);
    }

    [Fact]
    public void Equality_Structural()
    {
        var a = new UnitPoint(0.1f, 0.2f);
        var b = new UnitPoint(0.1f, 0.2f);
        var c = new UnitPoint(0.1f, 0.3f);
        Assert.True(a == b);
        Assert.True(a != c);
    }
}
