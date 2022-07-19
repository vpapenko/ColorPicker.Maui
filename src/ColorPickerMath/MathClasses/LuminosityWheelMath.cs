namespace ColorPickerMath;

public class LuminosityWheelMath : ColorPickerMathBase
{
    float _internalRadius = 0.25F;

    public LuminosityWheelMath()    { }

    public LuminosityWheelMath( float internalRadius )
        => InternalRadius = internalRadius;


    public float    Rotation    { get; set; }
    public bool     IsLeftSide  { get; protected set; } = true;

    float           MiddleRadius => ( 0.5F - _internalRadius ) / 2 + _internalRadius;

    public float    InternalRadius
    {
        get => _internalRadius;
        set => _internalRadius = value < 0 ? 0 : value > 0.5f ? 0.5f : value;
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
        var polar = ToPolarPoint( point );
        return polar.ToPointF().ShiftFromCenter();
    }

    public override bool IsInActiveArea( PointF point, Color color )
    {
        var polar = ToPolarPoint( point.ShiftToCenter() );
        return polar.Radius <= 0.5 && polar.Radius >= _internalRadius;
    }

    public override Color UpdateColor( PointF point, Color color )
    {
        UpdateSide( point );

        var polar       = ToPolarPoint( point ).AddAngle( Rotation + (float)Math.PI / 2F );
        var luminosity  = Math.Abs(polar.Angle) / (float)Math.PI;

        return Color.FromHsla( color.GetHue(), color.GetSaturation(), luminosity, color.Alpha );
    }

    void UpdateSide( PointF point )
    {
        var centeredPoint   = point.ShiftToCenter();
        IsLeftSide          = centeredPoint.X < 0;
    }

    PolarPoint ToPolarPoint( PointF point )
    {
        var centeredPoint   = point.ShiftToCenter();

        return new PolarPoint( centeredPoint )
        {
            Radius = MiddleRadius
        };
    }
}
