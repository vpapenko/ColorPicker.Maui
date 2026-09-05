using ColorPicker.Rendering;

namespace ColorPicker.Controls;

/// <summary>A stack of Hue, Saturation and Lightness sliders, with an optional alpha slider.</summary>
public class HslSlider : SliderStackWithAlpha
{
    protected override IEnumerable<SliderBase> GetSliders()
    {
        var result = new List<DelegateSlider>()
            {
                new DelegateSlider(HslSliderFactory.NewValueH,
                            HslSliderFactory.GetNewColorH,
                            HslSliderFactory.GetGradientH,
                            SliderChannel.Hue),

                new DelegateSlider(HslSliderFactory.NewValueS,
                            HslSliderFactory.GetNewColorS,
                            HslSliderFactory.GetGradientS,
                            SliderChannel.Saturation),

                new DelegateSlider(HslSliderFactory.NewValueL,
                            HslSliderFactory.GetNewColorL,
                            HslSliderFactory.GetGradientL,
                            SliderChannel.Luminosity)
            };

        if (ShowAlphaSlider)
        {
            var slider = new DelegateSlider(AlphaSliderFactory.NewValueAlpha,
                                     AlphaSliderFactory.GetNewColorAlpha,
                                     AlphaSliderFactory.GetGradientAlpha,
                                     SliderChannel.Alpha)
            {
                PaintChessPattern = true
            };
            result.Add(slider);
        }

        return result;
    }
}
