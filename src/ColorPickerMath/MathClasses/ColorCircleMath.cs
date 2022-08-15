namespace ColorPickerMath;

public class ColorCircleMath : IMathAbstractions
{
    public float Rotation { get; set; }

    public PointF ColorToPoint( Color color )
    {
        var r = color.GetSaturation() / 2f;
        var a = color.GetHue() * 2f * (float)Math.PI - Rotation;

        var point = new PolarPoint( r, a ).ToPointF();
        point.X = -point.X;

        return point.ShiftFromCenter();
    }

    public PointF FitToActiveArea( PointF point, Color color )
    {
        var polar = new PolarPoint( point.ShiftToCenter() );

        if ( polar.Radius > 0.5f )
            polar.Radius = 0.5f;

        return polar.ToPointF().ShiftFromCenter();
    }

    public bool IsInActiveArea( PointF point, Color color )
            => new PolarPoint( point.ShiftToCenter() ).Radius <= 1f;

    public Color UpdateColor( PointF point, Color color )
    {
        var centeredPoint   = FitToActiveArea( point, color ).ShiftToCenter();
        centeredPoint.Y = -centeredPoint.Y;

        var rotatedPolar    = centeredPoint.ToPolarPoint().AddAngle( Rotation );

        var h   = (rotatedPolar.Angle + Math.PI) / (Math.PI * 2);
        var s   = rotatedPolar.Radius * 2;
        return Color.FromHsla( h, s, color.GetLuminosity(), color.Alpha );
    }
}