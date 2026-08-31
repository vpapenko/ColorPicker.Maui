namespace ColorPicker.Controls;

/// <summary>A single alpha (opacity) slider.</summary>
public class AlphaSlider : SliderStack
{
    protected override IEnumerable<SliderBase> GetSliders()
        => new SliderBase[]
        {
            new DelegateSlider(AlphaSliderFactory.NewValueAlpha,
                        AlphaSliderFactory.GetNewColorAlpha,
                        AlphaSliderFactory.GetPaintAlpha)
            {
                PaintChessPattern = true
            }
        };
}
