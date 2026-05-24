namespace ColorPicker.Core;

/// <summary>
/// Base class for RGB channel sliders (Red / Green / Blue). Each subclass
/// plugs in a read/write pair for its specific channel; the geometry comes
/// from <see cref="LinearTrack"/>.
///
/// <para>RGB sliders operate on the RGB projection of the caller's HSL
/// color via <see cref="ColorConversions"/>. To match MAUI behavior, when
/// the resulting RGB triple is grayscale (R=G=B) we keep the caller's
/// original H and S so the picker indicator doesn't snap to hue 0.</para>
/// </summary>
public abstract class RgbChannelSlider : IColorPickerArea
{
    public LinearTrack Track { get; }

    protected RgbChannelSlider(LinearTrack track) { Track = track; }

    public bool IsInActiveArea(UnitPoint point, HslaColor color) => true;

    public UnitPoint FitToActiveArea(UnitPoint point, HslaColor color)
        => Track.PointFor(Track.ValueAt(point));

    public HslaColor UpdateColor(UnitPoint point, HslaColor color)
    {
        var rgb = ColorConversions.HslToRgb(color);
        var newRgb = Write(rgb, Track.ValueAt(point));
        var newHsl = ColorConversions.RgbToHsl(newRgb);
        // Preserve hue/saturation when the new color is grayscale (RgbToHsl
        // returns H=S=0 by convention, but the caller's preferred hue should
        // stick across grayscale transitions).
        if (newHsl.S == 0)
            newHsl = newHsl.WithH(color.H).WithS(color.S == 0 ? 0 : color.S);
        return newHsl;
    }

    public UnitPoint ColorToPoint(HslaColor color)
        => Track.PointFor(Read(ColorConversions.HslToRgb(color)));

    protected abstract double Read(RgbaColor c);
    protected abstract RgbaColor Write(RgbaColor c, double value);
}

public sealed class RedSlider : RgbChannelSlider
{
    public RedSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(RgbaColor c) => c.R;
    protected override RgbaColor Write(RgbaColor c, double v) => c.WithR(v);
}

public sealed class GreenSlider : RgbChannelSlider
{
    public GreenSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(RgbaColor c) => c.G;
    protected override RgbaColor Write(RgbaColor c, double v) => c.WithG(v);
}

public sealed class BlueSlider : RgbChannelSlider
{
    public BlueSlider(bool vertical = false) : base(new LinearTrack(vertical)) { }
    protected override double Read(RgbaColor c) => c.B;
    protected override RgbaColor Write(RgbaColor c, double v) => c.WithB(v);
}
