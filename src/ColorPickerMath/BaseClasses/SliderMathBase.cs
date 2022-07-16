namespace ColorPickerMath;

public abstract class SliderMathBase : ColorPickerMathBase
{
    protected virtual Orientation Orientation { get => Orientation.Horizontal; }

    protected abstract float GetSliderValue( Color color );

    public override bool IsInActiveArea( PointF point, Color color )
    {
        return point.X >= 0 && point.X <= 1 && point.Y >= 0 && point.Y <= 1;
    }

    public override PointF ColorToPoint( Color color )
    {
        var sliderValue = GetSliderValue(color);
        if ( Orientation == Orientation.Horizontal )
            return new PointF( sliderValue, 0.5f );
        else
            return new PointF( 0.5f, sliderValue );
    }

    public override PointF FitToActiveArea( PointF point, Color color )
    {
        if ( Orientation == Orientation.Horizontal )
        {
            point.X = LimitToSize( point.X );
            point.Y = 0.5f;
        }
        else
        {
            point.X = 0.5f;
            point.Y = LimitToSize( point.Y );
        }
        return point;
    }

    protected float GetSliderValue( PointF point, Color color )
    {
        point = FitToActiveArea( point, color );
        return Orientation == Orientation.Horizontal ? point.X : point.Y;
    }

    float LimitToSize( float coordinate )
    {
        coordinate = coordinate < 0 ? 0 : coordinate;
        coordinate = coordinate > 1 ? 1 : coordinate;
        return coordinate;
    }
}
