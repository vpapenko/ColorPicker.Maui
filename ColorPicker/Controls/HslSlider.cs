namespace ColorPicker.Controls;

public class HslSlider : SliderStackWithAlpha
{
    protected override IEnumerable<SliderBase> GetSliders()
    {
        var result = new List<DelegateSlider>()
            {
                new DelegateSlider( HslSliderFactory.NewValueH,
                            HslSliderFactory.GetNewColorH,
                            HslSliderFactory.GetPaintH ),

                new DelegateSlider( HslSliderFactory.NewValueS,
                            HslSliderFactory.GetNewColorS,
                            HslSliderFactory.GetPaintS),

                new DelegateSlider( HslSliderFactory.NewValueL,
                            HslSliderFactory.GetNewColorL,
                            HslSliderFactory.GetPaintL )
            };

        if ( ShowAlphaSlider )
        {
            var slider = new DelegateSlider( AlphaSliderFactory.NewValueAlpha,
                                     AlphaSliderFactory.GetNewColorAlpha,
                                     AlphaSliderFactory.GetPaintAlpha )
            {
                PaintChessPattern = true
            };
            result.Add( slider );
        }

        return result;
    }
}
