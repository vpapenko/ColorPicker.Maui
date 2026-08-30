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
                            RgbSliderFactory.GetPaintR),

                new DelegateSlider(RgbSliderFactory.NewValueG,
                            RgbSliderFactory.GetNewColorG,
                            RgbSliderFactory.GetPaintG),

                new DelegateSlider(RgbSliderFactory.NewValueB,
                            RgbSliderFactory.GetNewColorB,
                            RgbSliderFactory.GetPaintB)
            };

        if (ShowAlphaSlider)
        {
            var slider = new DelegateSlider(AlphaSliderFactory.NewValueAlpha,
                                     AlphaSliderFactory.GetNewColorAlpha,
                                     AlphaSliderFactory.GetPaintAlpha)
            {
                PaintChessPattern = true
            };
            result.Add(slider);
        }

        return result;
    }
}
