namespace ColorPicker.Core;

/// <summary>
/// The unified abstraction for every interactive picker shape — wheel, disc,
/// luminosity ring, triangle, slider. All math is expressed on the unit
/// square [0,1] × [0,1] and is independent of pixel size, rendering framework,
/// and platform.
///
/// Implementations are expected to be pure (no hidden mutable state) so they
/// can be exercised with deterministic unit tests.
/// </summary>
public interface IColorPickerArea
{
    /// <summary>True if <paramref name="point"/> falls inside this picker's interactive area.</summary>
    bool IsInActiveArea(UnitPoint point, HslaColor color);

    /// <summary>Project <paramref name="point"/> onto the interactive area (clamps out-of-bounds).</summary>
    UnitPoint FitToActiveArea(UnitPoint point, HslaColor color);

    /// <summary>Compute the new color that corresponds to <paramref name="point"/>, starting from <paramref name="color"/>.</summary>
    HslaColor UpdateColor(UnitPoint point, HslaColor color);

    /// <summary>Compute the unit-square location of the current <paramref name="color"/>'s indicator.</summary>
    UnitPoint ColorToPoint(HslaColor color);
}
