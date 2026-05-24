namespace ColorPicker.Core;

/// <summary>
/// Pure helpers for sizing a color-picker's indicator (the small circle that
/// marks the currently-selected location inside a wheel, disc, triangle or
/// slider). The math is rendering-agnostic: callers pass in the canvas
/// edge length in pixels and the device DPI, and get back a scale factor
/// (fraction of canvas size) or a pixel radius.
///
/// <para>
/// The default strategy aims for an indicator that is physically the same
/// size on screens of different densities — roughly the size of a finger
/// tip — while still being clamped on tiny or huge canvases so the picker
/// remains usable.
/// </para>
/// </summary>
public static class IndicatorRadius
{
    /// <summary>
    /// Lower bound on the indicator scale (as a fraction of the canvas
    /// edge). Prevents indicators from disappearing on very large
    /// canvases. Value: 0.025 (a 1000px canvas yields a 25px radius).
    /// </summary>
    public const float MinScale = 0.025F;

    /// <summary>
    /// Upper bound on the indicator scale. Prevents indicators from
    /// dominating very small canvases. Value: 0.08 (a 100px canvas
    /// yields an 8px radius).
    /// </summary>
    public const float MaxScale = 0.08F;

    /// <summary>
    /// Target physical radius of an indicator, in millimeters. Picked so
    /// the indicator is roughly the size of a fingertip touch contact
    /// (≈3 mm), large enough to see but small enough not to obscure the
    /// color sample under it.
    /// </summary>
    public const float TargetMillimeters = 3.0F;

    const float MillimetersPerInch = 25.4F;

    /// <summary>
    /// Compute a sensible default indicator scale (fraction of the canvas
    /// edge) for a square color picker. The scale aims for a physical
    /// radius of <see cref="TargetMillimeters"/> mm at the given DPI,
    /// then clamps to <see cref="MinScale"/>..<see cref="MaxScale"/>.
    /// </summary>
    /// <param name="canvasSizePx">Canvas edge length in physical pixels.
    /// Must be positive.</param>
    /// <param name="dpi">Display density in pixels per inch (e.g. 96 for
    /// a 1× display, 192 for a 2× display). Must be positive.</param>
    /// <returns>Scale in (0, 1) that, multiplied by <paramref name="canvasSizePx"/>,
    /// gives a pixel radius.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If either argument is
    /// not strictly positive.</exception>
    public static float ComputeDefaultScale(float canvasSizePx, float dpi)
    {
        if (canvasSizePx <= 0F)
            throw new ArgumentOutOfRangeException(nameof(canvasSizePx), canvasSizePx, "Canvas size must be > 0.");
        if (dpi <= 0F)
            throw new ArgumentOutOfRangeException(nameof(dpi), dpi, "DPI must be > 0.");

        var targetPx = dpi * (TargetMillimeters / MillimetersPerInch);
        var scale    = targetPx / canvasSizePx;

        if (scale < MinScale) return MinScale;
        if (scale > MaxScale) return MaxScale;
        return scale;
    }

    /// <summary>
    /// Convert an indicator scale and canvas edge length to a pixel
    /// radius. Equivalent to <c>canvasSizePx * scale</c>.
    /// </summary>
    public static float ComputePixels(float canvasSizePx, float scale)
        => canvasSizePx * scale;

    /// <summary>
    /// Convenience overload: compute the default pixel radius directly
    /// for the given canvas size and DPI.
    /// </summary>
    public static float ComputeDefaultPixels(float canvasSizePx, float dpi)
        => ComputePixels(canvasSizePx, ComputeDefaultScale(canvasSizePx, dpi));
}
