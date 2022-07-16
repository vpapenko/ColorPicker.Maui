namespace ColorPickerMath;

public class RotatingColorTriangleMath : ColorPickerMathBase
{
    readonly ColorTriangleMath _colorTriangle = new ColorTriangleMath();
    float lastHue = 0;

    public override PointF ColorToPoint( Color color )
    {
        SetAngle( color );
        return _colorTriangle.ColorToPoint( color );
    }

    public override PointF FitToActiveArea( PointF point, Color color )
    {
        SetAngle( color );
        return _colorTriangle.FitToActiveArea( point, color );
    }

    public override bool IsInActiveArea( PointF point, Color color )
    {
        SetAngle( color );
        return _colorTriangle.IsInActiveArea( point, color );
    }

    public override Color UpdateColor( PointF point, Color color )
    {
        SetAngle( color );
        return _colorTriangle.UpdateColor( point, color );
    }

    void SetAngle( Color color )
    {
        ColorTriangleMath.HSLToHSV( color, out var _, out var saturation, out var _ );
        var hue = saturation > 0 ? color.GetHue() : lastHue;
        lastHue = saturation <= 0 ? lastHue : color.GetHue();
        _colorTriangle.Rotation = -2.094395f - hue * 2f * (float)Math.PI;
    }
}
