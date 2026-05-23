namespace ColorPicker.Controls;

public class AlphaSlider : SliderStack
{
    protected override IEnumerable<SliderBase> GetSliders() 
        => new SliderBase[]
        {
            new DelegateSlider( AlphaSliderFactory.NewValueAlpha,
                        AlphaSliderFactory.GetNewColorAlpha,
                        AlphaSliderFactory.GetPaintAlpha )
            {
                PaintChessPattern = true
            }
        };
}
