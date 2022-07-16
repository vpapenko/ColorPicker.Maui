global using Microsoft.Maui.Graphics;

namespace ColorPickerMath;

//  TODO: Implement default behavior
//
public abstract class ColorPickerMathBase
{
    public abstract bool    IsInActiveArea( PointF point, Color color );
    public abstract PointF  FitToActiveArea( PointF point, Color color );
    public abstract Color   UpdateColor( PointF point, Color color );
    public abstract PointF  ColorToPoint( Color color );
}
