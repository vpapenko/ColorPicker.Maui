using ColorPicker.Rendering;

namespace ColorPicker.BaseClasses;

public abstract class SliderBase
{
    protected SliderBase(SliderChannel channel)
    {
        Channel = channel;
    }

    public SliderChannel Channel { get; }
    public bool PaintChessPattern { get; set; }

    public abstract float NewValue(Color color);
    public abstract Color GetNewColor(float newValue, Color oldColor);
    public abstract ColorGradient GetGradient(Color color);
}
