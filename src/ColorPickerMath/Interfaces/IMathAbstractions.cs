namespace ColorPickerMath;

public interface IMathAbstractions
{
    bool IsInActiveArea( PointF point, Color color );
    PointF FitToActiveArea( PointF point, Color color );
    PointF ColorToPoint( Color color );
    Color UpdateColor( PointF point, Color color );
}