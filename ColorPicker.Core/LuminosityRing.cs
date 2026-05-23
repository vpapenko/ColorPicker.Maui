namespace ColorPicker.Core;

/// <summary>
/// Luminosity ring — the outer angular ring of the ColorWheel control. It
/// lives at radius 0.5 of the unit square (centered at (0.5, 0.5)) and
/// encodes L purely angularly.
///
/// Encoding (matches the existing MAUI ColorDisc 1:1):
/// <list type="bullet">
///   <item>L = 0  → 12 o'clock (top)</item>
///   <item>L = 1  → 6 o'clock  (bottom)</item>
///   <item>The point can be on either half (left or right) for any L in
///   (0, 1). The side is a UI-only preference; the encoded L value is the
///   same. Use <see cref="ColorToPoint(HslaColor, UnitPoint)"/> to preserve
///   the side from a previous indicator location, otherwise the default
///   right-hand side is used.</item>
/// </list>
///
/// Hue, saturation, alpha are passed through unchanged.
/// </summary>
public sealed class LuminosityRing : IColorPickerArea
{
    /// <summary>Half-thickness (in unit-square units) used by hit testing.</summary>
    public float HitTolerance { get; }

    public LuminosityRing(float hitTolerance = 0.05f)
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
        var centered = point.ToCentered();
        var polar = centered.ToPolar();
        // Degenerate (exact center): default to right side.
        if (polar.Radius == 0f)
            return new PolarPoint(0.5f, 0f).ToCartesian().FromCentered();
        return polar.WithRadius(0.5f).ToCartesian().FromCentered();
    }

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
    {
        var polar = point.ToCentered().ToPolar();
        // Shift so 12 o'clock = 0, then |angle| / π = L (mirrored across vertical axis).
        double shifted = polar.Angle + Math.PI / 2.0;
        // Re-wrap into [-π, π] using atan2 of sin/cos (matches the MAUI
        // FromPolar→ToPolar round-trip).
        shifted = Math.Atan2(Math.Sin(shifted), Math.Cos(shifted));
        double l = Math.Abs(shifted) / Math.PI;
        if (l > 1.0) l = 1.0;
        if (l < 0.0) l = 0.0;
        return color.WithL(l);
    }

    public UnitPoint ColorToPoint(HslaColor color) => ColorToPoint(color, sign: +1);

    /// <summary>
    /// Encode color using <paramref name="previousPoint"/> to determine
    /// which half of the ring the indicator should be placed on.
    /// </summary>
    public UnitPoint ColorToPoint(HslaColor color, UnitPoint previousPoint)
    {
        var prevPolar = previousPoint.ToCentered().ToPolar();
        // Re-wrap to [-π, π] after the shift; this matches MAUI's
        // FromPolar→ToPolar round-trip and is what makes left/right
        // detection correct at the angular discontinuity.
        double shifted = prevPolar.Angle - Math.PI / 2.0;
        shifted = Math.Atan2(Math.Sin(shifted), Math.Cos(shifted));
        int sign = shifted <= 0 ? +1 : -1;
        return ColorToPoint(color, sign);
    }

    UnitPoint ColorToPoint(HslaColor color, int sign)
    {
        double l = color.L;
        if (l < 0) l = 0;
        if (l > 1) l = 1;
        double angle = l * Math.PI * sign - Math.PI / 2.0;
        return new PolarPoint(0.5f, (float)angle).ToCartesian().FromCentered();
    }
}
