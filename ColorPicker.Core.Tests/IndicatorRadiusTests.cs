using ColorPicker.Core;
using Xunit;

namespace ColorPicker.Core.Tests;

public class IndicatorRadiusTests
{
    [Theory]
    [InlineData(0F)]
    [InlineData(-1F)]
    public void ComputeDefaultScale_NonPositiveCanvas_Throws(float canvas)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => IndicatorRadius.ComputeDefaultScale(canvas, 96F));
    }

    [Theory]
    [InlineData(0F)]
    [InlineData(-1F)]
    public void ComputeDefaultScale_NonPositiveDpi_Throws(float dpi)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => IndicatorRadius.ComputeDefaultScale(400F, dpi));
    }

    [Fact]
    public void ComputeDefaultScale_TinyCanvas_ClampsToMax()
    {
        // 100px @ 96dpi → desired ≈ 11.3px → scale ≈ 0.113 → clamped to 0.08.
        var scale = IndicatorRadius.ComputeDefaultScale(100F, 96F);
        Assert.Equal(IndicatorRadius.MaxScale, scale);
    }

    [Fact]
    public void ComputeDefaultScale_HugeCanvas_ClampsToMin()
    {
        // 5000px @ 96dpi → desired ≈ 11.3px → scale ≈ 0.00226 → clamped to 0.025.
        var scale = IndicatorRadius.ComputeDefaultScale(5000F, 96F);
        Assert.Equal(IndicatorRadius.MinScale, scale);
    }

    [Fact]
    public void ComputeDefaultScale_TypicalCanvas_IsWithinBounds()
    {
        // 400px @ 96dpi → desired ≈ 11.3px → scale ≈ 0.0283 → in range.
        var scale = IndicatorRadius.ComputeDefaultScale(400F, 96F);
        Assert.InRange(scale, IndicatorRadius.MinScale, IndicatorRadius.MaxScale);
        // Within ±0.5px of the 3mm target.
        var radiusPx = 400F * scale;
        Assert.InRange(radiusPx, 10.5F, 12.5F);
    }

    [Fact]
    public void ComputeDefaultScale_HigherDpi_GivesLargerPixelRadius_SameCanvas()
    {
        var rLow  = 400F * IndicatorRadius.ComputeDefaultScale(400F, 96F);
        var rHigh = 400F * IndicatorRadius.ComputeDefaultScale(400F, 192F);
        Assert.True(rHigh > rLow, $"Expected higher DPI to yield larger radius: low={rLow}, high={rHigh}");
    }

    [Fact]
    public void ComputeDefaultScale_DpiIndependentPhysicalSize_WhenInRange()
    {
        // The physical radius (px / dpi) should be roughly constant at
        // ≈3mm whenever the scale is not clamped.
        const float canvas = 400F;
        var dpis = new[] { 96F, 120F, 150F, 192F };
        foreach (var dpi in dpis)
        {
            var scale    = IndicatorRadius.ComputeDefaultScale(canvas, dpi);
            var radiusPx = canvas * scale;
            // Only check unclamped cases: at canvas=400 and these DPIs the
            // scale stays in range up to 192dpi (≈22.6px → 0.0566 ≤ 0.08).
            if (scale > IndicatorRadius.MinScale && scale < IndicatorRadius.MaxScale)
            {
                var mm = radiusPx / dpi * 25.4F;
                Assert.InRange(mm, IndicatorRadius.TargetMillimeters - 0.01F,
                                   IndicatorRadius.TargetMillimeters + 0.01F);
            }
        }
    }

    [Fact]
    public void ComputePixels_IsLinearProduct()
    {
        Assert.Equal(20F, IndicatorRadius.ComputePixels(400F, 0.05F), 4);
    }

    [Fact]
    public void ComputeDefaultPixels_ConsistentWithScale()
    {
        var pxA = IndicatorRadius.ComputeDefaultPixels(400F, 96F);
        var pxB = 400F * IndicatorRadius.ComputeDefaultScale(400F, 96F);
        Assert.Equal(pxA, pxB, 4);
    }
}
