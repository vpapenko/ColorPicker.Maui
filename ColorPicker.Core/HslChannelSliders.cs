namespace ColorPicker.Core;

/// <summary>
/// Base class for HSL channel sliders (Hue / Saturation / Luminosity /
/// Alpha). Each subclass plugs in a read/write pair for its specific
/// channel; the geometry comes from <see cref="LinearTrack"/>.
///
/// All channel sliders treat the entire unit square as their active area —
/// any tap is mapped to the track via <see cref="FitToActiveArea"/>. (Pixel
/// thickness / hit tolerance lives in the render layer, not here.)
/// </summary>
public abstract class HslChannelSlider : IColorPickerArea
{
    public LinearTrack Track { get; }

    protected HslChannelSlider(LinearTrack track) { Track = track; }

    public bool IsInActiveArea(UnitPoint point, HslaColor color) => true;

    public UnitPoint FitToActiveArea(UnitPoint point, HslaColor color)
        => Track.PointFor(Track.ValueAt(point));

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
        => Write(color, Track.ValueAt(point));

    public UnitPoint ColorToPoint(HslaColor color) => Track.PointFor(Read(color));

    protected abstract double Read(HslaColor color);
    protected abstract HslaColor Write(HslaColor color, double value);
}

public sealed class HueSlider : HslChannelSlider
{
    public HueSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(HslaColor c) => c.H;
    protected override HslaColor Write(HslaColor c, double v) => c.WithH(v);
}

public sealed class SaturationSlider : HslChannelSlider
{
    public SaturationSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(HslaColor c) => c.S;
    protected override HslaColor Write(HslaColor c, double v) => c.WithS(v);
}

public sealed class LuminositySlider : HslChannelSlider
{
    public LuminositySlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(HslaColor c) => c.L;
    protected override HslaColor Write(HslaColor c, double v) => c.WithL(v);
}

public sealed class AlphaSlider : HslChannelSlider
{
    public AlphaSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(HslaColor c) => c.A;
    protected override HslaColor Write(HslaColor c, double v) => c.WithA(v);
}
