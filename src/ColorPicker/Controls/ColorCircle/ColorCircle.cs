namespace ColorPicker;

public class ColorCircle : ColorPickerBase
{
    public ColorCircle()
    {
        var radius  = Math.Min( Math.Max( 500, HeightRequest ), Math.Max( 500, WidthRequest ) );
        HeightRequest = WidthRequest = radius;

        PickerMath = new ColorCircleMath();
        PickerDrawable = new ColorCircleDrawable( this );
        Drawable = PickerDrawable;
    }
}