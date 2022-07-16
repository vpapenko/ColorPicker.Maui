namespace ColorPickerMath;

public class LuminosityWheelMath : ColorPickerMathBase
{
    float _internalRadius = 0.25F;

    public LuminosityWheelMath()
    {
    }

    public LuminosityWheelMath( float internalRadius )
    {
        InternalRadius = internalRadius;
    }

    float MiddleRadius => ( 0.5F - _internalRadius ) / 2 + _internalRadius;
    public float Rotation { get; set; }
    public bool IsLeftSide { get; protected set; } = true;

    public float InternalRadius
    {
        get
        {
            return _internalRadius;
        }
        set
        {
            value = value < 0 ? 0 : value;
            value = value > 0.5F ? 0.5F : value;
            _internalRadius = value;
        }
    }

    public override PointF ColorToPoint( Color color )
    {
        var side    = IsLeftSide ? 1 : -1;
        var r       = MiddleRadius;
        var a       = color.GetLuminosity() * (float)Math.PI + side * (float)Math.PI / 2 - Rotation;

        var polar = new PolarPoint(r, a);
        var point = polar.ToPointF();

        point.Y = -1 * side * point.Y;
        return point.ShiftFromCenter();
    }

    public override PointF FitToActiveArea( PointF point, Color color )
    {
        var polar = ToPolarPoint(point);
        point = polar.ToPointF();
        return point.ShiftFromCenter();
    }

    public override bool IsInActiveArea( PointF point, Color color )
    {
        var polar = ToPolarPoint( point.ShiftToCenter() );
        return polar.Radius <= 0.5 && polar.Radius >= _internalRadius;
    }

    public override Color UpdateColor( PointF point, Color color )
    {
        UpdateSide( point );

        var polar = ToPolarPoint( point );
        polar.Angle += Rotation + (float)Math.PI / 2F;
        polar = polar.ToPointF().ToPolarPoint();

        var l = Math.Abs(polar.Angle) / Math.PI;
        return Color.FromHsla( color.GetHue(), color.GetSaturation(), l, color.Alpha );
    }

    void UpdateSide( PointF point )
    {
        var centeredPoint = point.ShiftToCenter();
        IsLeftSide = centeredPoint.X < 0;
    }

    PolarPoint ToPolarPoint( PointF point )
    {
        var centeredPoint = point.ShiftToCenter();
        return new PolarPoint( centeredPoint )
        {
            Radius = MiddleRadius
        };
    }
}
