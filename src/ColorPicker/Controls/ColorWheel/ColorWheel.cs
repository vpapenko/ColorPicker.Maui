namespace ColorPicker;

public partial class ColorWheel : ColorCircle
{
    public ColorWheel()
    {
        Drawable = PickerDrawable = new ColorWheelDrawable( this );
    }
}
