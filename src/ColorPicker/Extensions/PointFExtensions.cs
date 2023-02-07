using System.Runtime.CompilerServices;

namespace ColorPicker;

public static class PointFExtensions
{
    /// <summary>
    /// Adjust the X value of a PointF
    /// </summary>
    /// <param name="point">self</param>
    /// <param name="x">amount to add to the X value</param>
    /// <returns>self</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF AddX( this PointF point, float x )
    {
        point.X += x;
        return point;
    }

    /// <summary>
    /// Adjust the Y value of a PointF
    /// </summary>
    /// <param name="point">self</param>
    /// <param name="y">amount to add to the Y value</param>
    /// <returns>self</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF AddY( this PointF point, float y )
    {
        point.Y += y;
        return point;
    }

    /// <summary>
    /// Clone a PointF
    /// </summary>
    /// <param name="point">self</param>
    /// <returns>cloned value</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF Clone( this PointF point )
            => new PointF( point.X, point.Y );

    /// <summary>
    /// Adjust the point to the center
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF ShiftToCenter( this PointF point )
    {
        point.X -= 0.5F;
        point.Y -= 0.5F;
        return point;
    }

    /// <summary>
    /// Return center-adjusted point to previous context
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF ShiftFromCenter( this PointF point )
    {
        point.X += 0.5F;
        point.Y += 0.5F;
        return point;
    }

    /// <summary>
    /// Convert PolarPoint to PointF
    /// </summary>
    /// <param name="pp">self</param>
    /// <returns>converted value</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PointF ToPointF( this PolarPoint pp )
    {
        float x = pp.Radius * (float)Math.Cos(pp.Angle);
        float y = pp.Radius * (float)Math.Sin(pp.Angle);

        return new PointF( x, y );
    }
}
