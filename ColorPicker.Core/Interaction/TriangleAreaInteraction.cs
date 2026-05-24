namespace ColorPicker.Core.Interaction;

/// <summary>
/// Pure-math interaction controller for the ColorTriangleArea control —
/// holds the indicator state in unit-space and exposes per-region touch
/// methods so the MAUI shell stays a thin pixel adapter.
///
/// The controller deliberately keeps SV and H decoding separated: a hue
/// drag never re-decodes the SV indicator, and an SV drag never
/// re-decodes the hue indicator. This is the fix from PR #36 expressed
/// at the pure-math layer so it can be covered by deterministic tests.
/// </summary>
public sealed class TriangleAreaInteraction
{
    static readonly SaturationValueTriangle _triangleRotated = new(rotateByHue: true);
    static readonly SaturationValueTriangle _triangleFixed   = new(rotateByHue: false);
    static readonly HueRing                 _hueRing         = new();

    readonly bool _rotateByHue;

    SaturationValueTriangle Triangle => _rotateByHue ? _triangleRotated : _triangleFixed;

    public TriangleAreaInteraction(bool rotateByHue = true)
    {
        _rotateByHue = rotateByHue;
        Color = new HslaColor(0, 0, 0, 1);
        LocationSv = Triangle.ColorToPoint(Color);
        LocationH = _hueRing.ColorToPoint(Color);
        ZeroSL = true;
    }

    /// <summary>Current selected color, after the ZeroSL grayscale-hue-memory correction is applied.</summary>
    public HslaColor Color { get; private set; }

    /// <summary>Last non-grayscale hue. Stays put when the color becomes grayscale.</summary>
    public double LastHue { get; private set; }

    /// <summary>True when the color is grayscale; the SV indicator's hue dimension is then held at LastHue.</summary>
    public bool ZeroSL { get; private set; }

    /// <summary>SV indicator location in unit-space (SV active area).</summary>
    public UnitPoint LocationSv { get; private set; }

    /// <summary>Hue indicator location in unit-space (H ring active area).</summary>
    public UnitPoint LocationH { get; private set; }

    /// <summary>
    /// Re-sync the controller from an externally-set color (e.g. when the
    /// MAUI <c>SelectedColor</c> property is changed by data binding).
    /// Updates LastHue / ZeroSL, re-encodes indicator locations.
    /// </summary>
    public void SyncFromColor(HslaColor color)
    {
        // Threshold matches MAUI ColorTriangleArea.OnSelectedColorChanging
        // (0.00390625 = 1/256, i.e. effectively grayscale at 8-bit RGB).
        if (color.S > 0.00390625)
        {
            LastHue = color.H;
            ZeroSL = false;
        }
        else
        {
            ZeroSL = true;
        }

        Color = color;
        ReencodeLocations();
    }

    /// <summary>Update from a touch in the SV (triangle) region.</summary>
    public HslaColor UpdateFromSv(UnitPoint svUnit)
    {
        LocationSv = Triangle.FitToActiveArea(svUnit, Color);

        // Only decode SV; H is left untouched so the SV drag never
        // re-quantizes hue through the encode/decode roundtrip.
        var inColor = new HslaColor(LastHue, Color.S, Color.L, Color.A);
        var newColor = Triangle.UpdateColor(LocationSv, inColor);

        WriteColor(newColor);
        return Color;
    }

    /// <summary>Update from a touch in the H (hue ring) region.</summary>
    public HslaColor UpdateFromH(UnitPoint hUnit)
    {
        LocationH = _hueRing.FitToActiveArea(hUnit, Color);

        // Only decode H; SV is left untouched so the H drag never
        // re-quantizes S/L through the encode/decode roundtrip. This
        // is the fix from PR #36, expressed at the controller level.
        var inColor = new HslaColor(LastHue, Color.S, Color.L, Color.A);
        var newColor = _hueRing.UpdateColor(LocationH, inColor);

        WriteColor(newColor);

        // Re-encode SV from the new hue so the rotating-triangle indicator
        // tracks the (S, L) point under the rotated geometry.
        LocationSv = Triangle.ColorToPoint(Color);
        return Color;
    }

    public bool IsInSv(UnitPoint svUnit) => Triangle.IsInActiveArea(svUnit, Color);

    public bool IsInH(UnitPoint hUnit, float tolerance)
        => new HueRing(tolerance).IsInActiveArea(hUnit, Color);

    public UnitPoint FitToSv(UnitPoint svUnit) => Triangle.FitToActiveArea(svUnit, Color);

    void WriteColor(HslaColor candidate)
    {
        // Grayscale hue-memory: if we were grayscale and the new color picked
        // up some saturation, snap hue back to LastHue.
        if (ZeroSL && candidate.S > 0)
        {
            candidate = candidate.WithH(LastHue);
        }
        LastHue = candidate.H;
        Color = candidate;
    }

    void ReencodeLocations()
    {
        // Use LastHue (not Color.H) so the SV indicator stays put on
        // grayscale — mirrors MAUI's UpdateLocations.
        var probe = new HslaColor(LastHue, Color.S, Color.L, Color.A);
        LocationSv = Triangle.ColorToPoint(probe);
        LocationH  = _hueRing.ColorToPoint(probe);
    }
}
