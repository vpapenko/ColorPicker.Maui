namespace ColorPicker.Core.Interaction;

/// <summary>
/// Pure-math interaction controller for the ColorDisc control —
/// holds the indicator state in unit-space and exposes per-region touch
/// methods so the MAUI shell stays a thin pixel adapter.
///
/// Mirrors <see cref="TriangleAreaInteraction"/>: HS and L decoding are
/// kept separated so a luminosity drag never re-decodes HS, and an HS
/// drag never re-decodes L. This is the analogous fix to PR #36 applied
/// pre-emptively to ColorDisc.
/// </summary>
public sealed class ColorDiscInteraction
{
    static readonly HueSaturationDisc _hsDisc = new();
    static readonly LuminosityRing    _lRing  = new();

    public ColorDiscInteraction()
    {
        Color = new HslaColor(0, 0, 0, 1);
        LocationHs = _hsDisc.ColorToPoint(Color);
        LocationL = _lRing.ColorToPoint(Color);
    }

    /// <summary>Current selected color.</summary>
    public HslaColor Color { get; private set; }

    /// <summary>HS indicator location in unit-space (HS disc active area).</summary>
    public UnitPoint LocationHs { get; private set; }

    /// <summary>L indicator location in unit-space (L ring active area).</summary>
    public UnitPoint LocationL { get; private set; }

    /// <summary>
    /// Re-sync the controller from an externally-set color. Re-encodes the HS
    /// indicator unconditionally and preserves the L-ring side (left/right) by
    /// rounding through the previous L location.
    /// </summary>
    public void SyncFromColor(HslaColor color)
    {
        Color = color;

        // MAUI ColorDisc.UpdateLocations preserves HS when the color is pure
        // black (L=0) AND the cached HS location is still inside the disc;
        // otherwise it re-encodes. Mirror that behavior here.
        if (color.L != 0 || !_hsDisc.IsInActiveArea(LocationHs, color))
        {
            LocationHs = _hsDisc.ColorToPoint(color);
        }
        LocationL = _lRing.ColorToPoint(color, LocationL);
    }

    /// <summary>Update from a touch in the HS (disc) region.</summary>
    public HslaColor UpdateFromHs(UnitPoint hsUnit)
    {
        LocationHs = _hsDisc.FitToActiveArea(hsUnit, Color);

        // Only decode HS; L is left untouched.
        Color = _hsDisc.UpdateColor(LocationHs, Color);
        return Color;
    }

    /// <summary>Update from a touch in the L (luminosity ring) region.</summary>
    public HslaColor UpdateFromL(UnitPoint lUnit)
    {
        LocationL = _lRing.FitToActiveArea(lUnit, Color);

        // Only decode L; HS is left untouched so the L drag never
        // re-quantizes hue/saturation through the encode/decode roundtrip.
        Color = _lRing.UpdateColor(LocationL, Color);
        return Color;
    }

    public bool IsInHs(UnitPoint hsUnit) => _hsDisc.IsInActiveArea(hsUnit, Color);

    public bool IsInL(UnitPoint lUnit, float tolerance)
        => new LuminosityRing(tolerance).IsInActiveArea(lUnit, Color);

    public UnitPoint FitToHs(UnitPoint hsUnit) => _hsDisc.FitToActiveArea(hsUnit, Color);
    public UnitPoint FitToL(UnitPoint lUnit) => _lRing.FitToActiveArea(lUnit, Color);
}
