namespace ColorPicker;

public partial class ColorCircle : ColorPickerBase
{
    public ColorCircle()
    {
        Drawable = PickerDrawable = new ColorCircleDrawable( this );
    }
}
