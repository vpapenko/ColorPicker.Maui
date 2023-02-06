namespace ColorPickerMath;

public interface IMathAbstractions
{
    bool    IsInActiveArea( PointF point );
    PointF  FitToActiveArea( PointF point );
    PointF  ColorToPoint( Color color );
    Color   UpdateColor( PointF point, Color color );
}
