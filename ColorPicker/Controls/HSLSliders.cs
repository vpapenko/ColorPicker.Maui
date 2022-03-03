namespace ColorPicker.Controls;

public class HSLSliders : SliderPickerWithAlpha
{
    protected override IEnumerable<SliderBase> GetSliders()
    {
        var result = new List<Slider>()
            {
                new Slider( SliderFunctionsHSL.NewValueH,
                            SliderFunctionsHSL.GetNewColorH,
                            SliderFunctionsHSL.GetPaintH ),

                new Slider( SliderFunctionsHSL.NewValueS,
                            SliderFunctionsHSL.GetNewColorS,
                            SliderFunctionsHSL.GetPaintS),

                new Slider( SliderFunctionsHSL.NewValueL,
                            SliderFunctionsHSL.GetNewColorL,
                            SliderFunctionsHSL.GetPaintL )
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
