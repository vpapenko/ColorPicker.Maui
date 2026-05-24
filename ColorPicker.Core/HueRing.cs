namespace ColorPicker.Core;

/// <summary>
/// Hue ring — the outer angular hue picker of ColorTriangle. Encodes hue
/// purely angularly around a circle of radius 0.5 centered at (0.5, 0.5).
///
/// Encoding (matches MAUI <c>ColorTriangleArea</c> 1:1):
/// <list type="bullet">
///   <item>angle  = π − 2π · hue   (so hue 0 → angle π = left edge, same convention as HueSaturationDisc)</item>
///   <item>radius = 0.5</item>
/// </list>
///
/// Saturation, luminosity, and alpha pass through unchanged.
/// </summary>
public sealed class HueRing : IColorPickerArea
{
    /// <summary>Half-thickness (in unit-square units) used by hit testing.</summary>
    public float HitTolerance { get; }

    public HueRing(float hitTolerance = 0.05f)
    {
        if (hitTolerance < 0f) throw new ArgumentOutOfRangeException(nameof(hitTolerance));
        HitTolerance = hitTolerance;
    }

    public bool IsInActiveArea(UnitPoint point, HslaColor color)
    {
        var r = point.ToCentered().ToPolar().Radius;
        return Math.Abs(r - 0.5f) <= HitTolerance;
    }

    public UnitPoint FitToActiveArea(UnitPoint point, HslaColor color)
    {
        var polar = point.ToCentered().ToPolar();
        if (polar.Radius == 0f)
            return new PolarPoint(0.5f, 0f).ToCartesian().FromCentered();
        return polar.WithRadius(0.5f).ToCartesian().FromCentered();
    }

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
    {
        var polar = point.ToCentered().ToPolar();
        double h = (Math.PI - polar.Angle) / (2.0 * Math.PI);
        h %= 1.0;
        if (h < 0) h += 1.0;
        return color.WithH(h);
    }

    public UnitPoint ColorToPoint(HslaColor color)
    {
        double angle = Math.PI - (2.0 * Math.PI * color.H);
        return new PolarPoint(0.5f, (float)angle).ToCartesian().FromCentered();
    }
}
