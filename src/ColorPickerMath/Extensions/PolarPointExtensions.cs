namespace ColorPickerMath;

public static class PolarPointExtensions
{
    /// <summary>
    /// Adjust the angle of a PolalPoint
    /// </summary>
    /// <param name="point">self</param>
    /// <param name="angle">value to add to the angle</param>
    /// <returns>self</returns>
    public static PolarPoint AddAngle( this PolarPoint point, float angle )
    {
        point.Angle += angle;
        return point;
    }
    
    /// <summary>
    /// Adjust the radius of a PolarPoint
    /// </summary>
    /// <param name="point">self</param>
    /// <param name="radius">value to add to the radius</param>
    /// <returns>self</returns>
    public static PolarPoint AddRadius( this PolarPoint point, float radius )
    {
        point.Radius += radius;
        return point;
    }

    /// <summary>
    /// Clone a PolarPoint
    /// </summary>
    /// <param name="point">self</param>
    /// <returns>cloned value</returns>
    public static PolarPoint Clone( this PolarPoint pp )
            => new PolarPoint( pp.Radius, pp.Angle );
}
