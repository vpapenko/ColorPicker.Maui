namespace ColorPicker.Controls;

public class RGBSliders : SliderPickerWithAlpha
{
    protected override IEnumerable<SliderBase> GetSliders()
    {
        var result = new List<Slider>()
            {
                new Slider( SliderFunctionsRGB.NewValueR,
                            SliderFunctionsRGB.GetNewColorR,
                            SliderFunctionsRGB.GetPaintR ),

                new Slider( SliderFunctionsRGB.NewValueG,
                            SliderFunctionsRGB.GetNewColorG,
                            SliderFunctionsRGB.GetPaintG ),

                new Slider( SliderFunctionsRGB.NewValueB,
                            SliderFunctionsRGB.GetNewColorB,
                            SliderFunctionsRGB.GetPaintB )
            };

        if ( ShowAlphaSlider )
        {
            var slider = new Slider( SliderFunctionsAlpha.NewValueAlpha,
                                     SliderFunctionsAlpha.GetNewColorAlpha,
                                     SliderFunctionsAlpha.GetPaintAlpha )
            {
                PaintChessPattern = true
            };
            result.Add( slider );
        }

        return result;
    }
}
