namespace ColorPicker;

public partial class ColorCircle : PickerBase
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