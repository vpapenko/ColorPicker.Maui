using ColorPicker.Rendering;

namespace ColorPicker.Controls;

/// <summary>A stack of Red, Green and Blue sliders, with an optional alpha slider.</summary>
public class RgbSlider : SliderStackWithAlpha
{
    protected override IEnumerable<SliderBase> GetSliders()
    {
        var result = new List<DelegateSlider>()
            {
                new DelegateSlider(RgbSliderFactory.NewValueR,
                            RgbSliderFactory.GetNewColorR,
                            RgbSliderFactory.GetGradientR,
                            SliderChannel.Red),

                new DelegateSlider(RgbSliderFactory.NewValueG,
                            RgbSliderFactory.GetNewColorG,
                            RgbSliderFactory.GetGradientG,
                            SliderChannel.Green),

                new DelegateSlider(RgbSliderFactory.NewValueB,
                            RgbSliderFactory.GetNewColorB,
                            RgbSliderFactory.GetGradientB,
                            SliderChannel.Blue)
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
