namespace ColorPicker.Controls;

public class AlphaSlider : SliderPicker
{
    protected override IEnumerable<SliderBase> GetSliders() 
        => new SliderBase[]
        {
            new Slider( SliderFunctionsAlpha.NewValueAlpha,
                        SliderFunctionsAlpha.GetNewColorAlpha,
                        SliderFunctionsAlpha.GetPaintAlpha )
            {
                PaintChessPattern = true
            }
        };
}
