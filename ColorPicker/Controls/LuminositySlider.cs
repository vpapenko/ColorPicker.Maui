namespace ColorPicker.Controls;

public class LuminositySlider : SliderStack
{
    protected override IEnumerable<SliderBase> GetSliders()
        => new SliderBase[]
            {
                new DelegateSlider(HslSliderFactory.NewValueL,
                            HslSliderFactory.GetNewColorL,
                            HslSliderFactory.GetPaintL)
            };
}
