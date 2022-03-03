namespace ColorPicker.Controls;

public class LuminositySlider : SliderPicker
{
    protected override IEnumerable<SliderBase> GetSliders() 
        =>  new SliderBase[]
            {
                new Slider( SliderFunctionsHSL.NewValueL, 
                            SliderFunctionsHSL.GetNewColorL, 
                            SliderFunctionsHSL.GetPaintL )
            };
}
