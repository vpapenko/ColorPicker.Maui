namespace ColorPickerMath;

public class RedHorizontalSliderMath : SliderMathBase
{
    public override Color UpdateColor( PointF point, Color color )
    {
        var newValue = GetSliderValue(point, color);
        return Color.FromRgba( newValue, color.Green, color.Blue, color.Alpha );
    }

    protected override float GetSliderValue( Color color )
    {
        return color.Red;
    }
}
