using ColorPicker.Core;
using ColorPicker.Rendering;

namespace ColorPicker.Controls;

public static class AlphaSliderFactory
{
    static readonly Core.AlphaSlider _alpha = new();

    public static float NewValueAlpha(Color color) => (float)_alpha.Read(color.ToHsla());

    public static Color GetNewColorAlpha(float newValue, Color oldColor)
        => Color.FromRgba(oldColor.Red, oldColor.Green, oldColor.Blue,
                          _alpha.Write(oldColor.ToHsla(), newValue).A);

    public static ColorGradient GetGradientAlpha(Color color)
    {
        var startColor = Color.FromRgba(color.Red, color.Green, color.Blue, 0).ToSKColor();
        var endColor = Color.FromRgba(color.Red, color.Green, color.Blue, 1).ToSKColor();
        return GetGradient(startColor, endColor);
    }

    public static ColorGradient GetGradient(SKColor startColor, SKColor endColor)
        => new(new[] { startColor, endColor }, new[] { 0F, 1F });
}
