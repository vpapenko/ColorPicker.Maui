namespace ColorPicker;

public class ColorWheelDrawable : ColorCircleDrawable
{
    public ColorWheelDrawable( ColorWheel wheel ) : base( wheel ) { }

    /// <summary>
    /// Draws the background using a sweep gradient
    /// </summary>
    public override void DrawBackground( ICanvas canvas, RectF dirtyRect )
    {
    }

    /// <summary>
    /// Draws the reticle
    /// </summary>
    public override void DrawContent( ICanvas canvas, RectF dirtyRect )
    {
    }
}
