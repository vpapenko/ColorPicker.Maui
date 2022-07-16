namespace ColorPickerMath;

public class AlphaHorizontalSliderMath : SliderMathBase
{
    public override Color UpdateColor( PointF point, Color color )
    {
        var newValue = GetSliderValue(point, color);
        return Color.FromRgba( color.Red, color.Green, color.Blue, newValue );
    }

    protected override float GetSliderValue( Color color )
    {
        return color.Alpha;
    }
}
