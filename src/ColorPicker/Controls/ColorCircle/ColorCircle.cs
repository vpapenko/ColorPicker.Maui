namespace ColorPicker;

public partial class ColorCircle : ColorPickerBase
{
    public ColorCircle()
    {
        var radius      = Math.Min( Math.Max( 500, HeightRequest ), Math.Max( 500, WidthRequest ) );
        HeightRequest   = WidthRequest      = radius;
        Drawable        = PickerDrawable    = new ColorCircleDrawable( this );
    }
}
