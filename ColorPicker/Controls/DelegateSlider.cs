using ColorPicker.Rendering;

namespace ColorPicker.Controls;

/// <summary>
/// A single slider whose channel behavior — reading the value from a color, writing
/// a new color, and describing the track gradient — is supplied as delegates, so custom
/// single-channel sliders can be built without a dedicated subclass.
/// </summary>
public class DelegateSlider : SliderBase
{
    readonly Func<Color, float>                _newValue;
    readonly Func<float, Color, Color>         _getNewColor;
    readonly Func<Color, ColorGradient>        _getGradient;

    public DelegateSlider(Func<Color, float> newValue,
                   Func<float, Color, Color> getNewColor,
                   Func<Color, ColorGradient> getGradient,
                   SliderChannel channel = SliderChannel.Custom)
        : base(channel)
    {
        _newValue = newValue;
        _getNewColor = getNewColor;
        _getGradient = getGradient;
    }

    public override Color GetNewColor(float newValue, Color oldColor) => _getNewColor(newValue, oldColor);
    public override ColorGradient GetGradient(Color color) => _getGradient(color);
    public override float NewValue(Color color) => _newValue(color);
}
