namespace ColorPicker;

public partial class ColorCircle
{
    public new float Rotation { get; set; }

    public override PointF ColorToPoint( Color color )
    {
        var r = color.GetSaturation() / 2f;
        var a = color.GetHue() * 2f * (float)Math.PI - Rotation;

        var point   = new PolarPoint( r, a ).ToPointF();
        point.X = -point.X;

        return point.ShiftFromCenter();
    }

    public override PointF FitToActiveArea( PointF point )
    {
        var polar = new PolarPoint( point.ShiftToCenter() );

        if ( polar.Radius > 0.5f )
            polar.Radius = 0.5f;

        return polar.ToPointF().ShiftFromCenter();
    }

    public override bool IsInActiveArea( PointF point )
            => new PolarPoint( point.ShiftToCenter() ).Radius <= 1f;

    public override Color UpdateColor( PointF point, Color color )
    {
        var centeredPoint   = FitToActiveArea( point ).ShiftToCenter();
        centeredPoint.Y = -centeredPoint.Y;
        var rotatedPolar    = centeredPoint.ToPolarPoint().AddAngle( Rotation );

        var h   = (rotatedPolar.Angle + Math.PI) / (Math.PI * 2);
        var s   = rotatedPolar.Radius * 2;
        return Color.FromHsla( h, s, color.GetLuminosity(), color.Alpha );
    }
}
