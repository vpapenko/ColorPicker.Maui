namespace ColorPicker.Classes;

public class SliderBounds
{
    public SliderBounds( SliderBase slider )  => Slider = slider;

    public SliderBase   Slider                      { get; }

    public long?        LocationProgressId          { get; set; }
    public float        OffsetLocationMultiplier    { get; set; }
    public SKPoint      Location                    { get; set; } = new SKPoint();

    public float GetSliderOffset( float PickerRadiusPixels )  => PickerRadiusPixels * OffsetLocationMultiplier;
}
