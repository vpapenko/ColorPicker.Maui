namespace ColorPicker.Core;

/// <summary>
/// HSV saturation/value triangle — the inner picker of ColorTriangle.
/// Operates on the unit square; the reference triangle is the equilateral
/// triangle inscribed in a circle of radius 0.5 centered at (0.5, 0.5),
/// apex up.
///
/// The mapping between (S, V) and triangle position is the same one used
/// by the existing MAUI <c>ColorTriangleArea</c>, ported 1:1 from pixel
/// space to unit-square coordinates (SvRadius is replaced by 0.5
/// throughout). The barycentric formulas are kept verbatim.
///
/// H is preserved from the input color (Core stores H explicitly so the
/// MAUI <c>_lastHue</c> mechanism is unnecessary). A is also preserved.
///
/// <para>If <see cref="RotateByHue"/> is true the triangle is rotated by
/// the current hue, matching MAUI's <c>RotateTriangleByHue</c> mode.</para>
/// </summary>
public sealed class SaturationValueTriangle : IColorPickerArea
{
    public bool RotateByHue { get; }

    public SaturationValueTriangle(bool rotateByHue = true) { RotateByHue = rotateByHue; }

    // Reference triangle constants (MAUI ColorTriangleArea, lines 420-422).
    // Tiny epsilon offsets are preserved verbatim — they keep the barycentric
    // formula from going degenerate at the triangle boundary.
    const float TriangleHeight = 1.5000001F;
    const float TriangleSide = 0.8660244F;
    const float TriangleVerticalOffset = 0.5000001F;

    // Active area = the bounding disc of radius 0.5. This matches MAUI's
    // LimitToSvTriangle which actually clamps to the disc, not the triangle
    // itself (the visual triangle clip is render-only).
    public bool IsInActiveArea(UnitPoint point, HslaColor color)
        => point.ToCentered().ToPolar().Radius <= 0.5f;

    public UnitPoint FitToActiveArea(UnitPoint point, HslaColor color)
    {
        var polar = point.ToCentered().ToPolar();
        if (polar.Radius > 0.5f) polar = polar.WithRadius(0.5f);
        return polar.ToCartesian().FromCentered();
    }

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
    {
        var hsv = ColorConversions.HslToHsv(color);
        var (s, v) = DecodeSv(point, hsv.H);
        // Keep the caller's hue explicitly (don't round-trip through RGB);
        // this preserves hue across grayscale transitions for free.
        var newHsv = new HsvaColor(hsv.H, s, v, hsv.A);
        return ColorConversions.HsvToHsl(newHsv);
    }

    public UnitPoint ColorToPoint(HslaColor color)
    {
        var hsv = ColorConversions.HslToHsv(color);
        return EncodeSv(hsv.S, hsv.V, hsv.H);
    }

    UnitPoint EncodeSv(double s, double v, double hue)
    {
        // Mirror of MAUI ColorTriangleArea.UpdateLocations (lines 211-236),
        // with the pixel SvRadius replaced by 0.5.
        double lumX = TriangleSide * (1 - 2 * s);
        double lumY = TriangleHeight;

        var polar = new UnitPoint((float)lumX, (float)lumY).ToPolar();
        polar = polar.WithRadius((float)(polar.Radius * v));

        var local = polar.ToCartesian();
        local = new UnitPoint(-local.X, local.Y - 1f);
        local = new UnitPoint(local.X * 0.5f, local.Y * 0.5f);

        // Rotate -2π/3 (constant triangle orientation correction).
        local = local.ToPolar().AddAngle((float)(-2.0 * Math.PI / 3.0)).ToCartesian();

        if (RotateByHue)
        {
            local = local.ToPolar()
                .AddAngle((float)(-((2.0 * Math.PI * hue) + (Math.PI / 2.0))))
                .ToCartesian();
        }

        return local.FromCentered();
    }

    (double s, double v) DecodeSv(UnitPoint point, double hue)
    {
        // Convert to the unit-radius local frame (MAUI's ToSvCoordinates,
        // with SvRadius = 0.5 → multiply by 2).
        var local = point.ToCentered();
        local = new UnitPoint(local.X * 2f, local.Y * 2f);

        if (RotateByHue)
        {
            local = local.ToPolar()
                .AddAngle((float)((2.0 * Math.PI * hue) + (Math.PI / 2.0)))
                .ToCartesian();
        }

        // Barycentric math — verbatim from MAUI WheelPointToColor (lines 435-448).
        float svX = local.X + TriangleSide;
        float svY = -local.Y + TriangleVerticalOffset;

        const float x1 = TriangleSide;
        const float y1 = TriangleHeight;
        const float x2 = x1 * 2;
        const float y2 = 0F;

        double vCurrent = ((svX * (y2 - y1)) - (svY * (x2 - x1)) + (x2 * y1) - (y2 * x1))
                          / Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));
        double v = (y1 - vCurrent) / y1;
        double sMax = x2 - (vCurrent / Math.Sin(Math.PI / 3.0));
        double sCurrent = svY / Math.Sin(Math.PI / 3.0);
        double s = sMax == 0 ? 0 : sCurrent / sMax;

        if (s < 0) s = 0;
        if (s > 1) s = 1;
        if (v < 0) v = 0;
        if (v > 1) v = 1;
        return (s, v);
    }
}
