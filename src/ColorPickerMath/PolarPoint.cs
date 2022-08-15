namespace ColorPickerMath;

public struct PolarPoint
{
    //  Properties
    //
    public float Radius { get; set; }

    public float Angle
    {
        get => _angle;
        set => _angle = (float)Math.Atan2( Math.Sin( value ), Math.Cos( value ) );
    }
    float _angle;

    /// <summary>
    /// Create a PolarPoint from an angle and radius
    /// </summary>
    public PolarPoint( float radius, float angle )
    {
        Radius = radius;
        Angle = angle;
    }

    /// <summary>
    /// Create a PolarPoint from a PointF
    /// </summary>
    public PolarPoint( PointF point )
    {
        //  c = sqrt( X^2 + Y^2 )
        Radius = (float)Math.Sqrt( ( point.X * point.X ) + ( point.Y * point.Y ) );

        //  a = arctan( Y, X )
        Angle = (float)Math.Atan2( point.Y, point.X );
    }

    public override string ToString()
    {
        return string.Format( "Radius: {0}; Angle: {1}", Radius, Angle );
    }
}