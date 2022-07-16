namespace ColorPickerMath;

public class BlueHorizontalSliderMath : SliderMathBase
{
    public override Color UpdateColor( PointF point, Color color )
    {
        var newValue = GetSliderValue(point, color);
        return Color.FromRgba( color.Red, color.Green, newValue, color.Alpha );
    }

    protected override float GetSliderValue( Color color )
    {
        return color.Blue;
    }
}
