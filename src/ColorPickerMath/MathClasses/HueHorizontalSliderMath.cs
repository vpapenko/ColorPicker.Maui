namespace ColorPickerMath;

public class HueHorizontalSliderMath : SliderMathBase
{
    public override Color UpdateColor(PointF point, Color color)
    {
        var newValue = GetSliderValue(point, color);
        return Color.FromHsla(newValue, color.GetSaturation(), color.GetLuminosity(), color.Alpha);
    }

    protected override float GetSliderValue(Color color)
        => color.GetHue();
}
