namespace ColorPicker.Core;

/// <summary>
/// HSL hue-saturation disc — the inner color disc of the ColorWheel control.
/// The active area is a circle of radius 0.5 centered at (0.5, 0.5) of the
/// unit square.
///
/// Encoding (matches the existing MAUI ColorDisc implementation 1:1):
/// <list type="bullet">
///   <item>angle  = (0.5 − hue) · 2π   (so hue 0 lands at angle π = unit-square left edge)</item>
///   <item>radius = 0.5 · saturation   (saturation 1 → on the disc boundary)</item>
/// </list>
/// Hue is always wrapped to [0, 1] on read-back. Luminosity and alpha are
/// passed through unchanged — they belong to the luminosity ring / alpha
/// slider, not to this surface.
///
/// This type is stateless and the methods are pure.
/// </summary>
public sealed class HueSaturationDisc : IColorPickerArea
{
    public bool IsInActiveArea(UnitPoint point, HslaColor color)
    {
        var r = point.ToCentered().ToPolar().Radius;
        return r <= 0.5f;
    }

    public UnitPoint FitToActiveArea(UnitPoint point, HslaColor color)
    {
        var polar = point.ToCentered().ToPolar();
        if (polar.Radius > 0.5f)
            polar = polar.WithRadius(0.5f);
        return polar.ToCartesian().FromCentered();
    }

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
    {
        var fit = FitToActiveArea(point, color).ToCentered().ToPolar();
        double hue = (Math.PI - fit.Angle) / (2.0 * Math.PI);
        hue = WrapHue(hue);
        double sat = fit.Radius * 2.0;
        if (sat > 1.0) sat = 1.0;
        if (sat < 0.0) sat = 0.0;
        return color.WithH(hue).WithS(sat);
    }

    public UnitPoint ColorToPoint(HslaColor color)
    {
        double angle = (0.5 - color.H) * (2.0 * Math.PI);
        double radius = 0.5 * Clamp01(color.S);
        var centered = new PolarPoint((float)radius, (float)angle).ToCartesian();
        return centered.FromCentered();
    }

    static double WrapHue(double h)
    {
        h %= 1.0;
        return h < 0 ? h + 1.0 : h;
    }

    static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
}
