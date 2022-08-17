namespace ColorPicker;

public class PickerBaseDrawable : ColorPickerBaseDrawable
{
    public PickerBaseDrawable( PickerBase picker ) : base( picker )
    {
    }

    //  Defers to subclass
    public override void DrawBackground( ICanvas canvas, RectF dirtyRect ) { }

    //  Draws a reticle ( subclass can override)
    public override void DrawContent( ICanvas canvas, RectF dirtyRect )
    {
        canvas.StrokeSize = 2;

        canvas.StrokeColor = Colors.White;
        canvas.DrawCircle( Center, Picker.ReticleRadius );

        canvas.StrokeColor = Colors.Black;
        canvas.DrawCircle( Center, Picker.ReticleRadius - 2 );

        canvas.StrokeColor = Colors.White;
        canvas.DrawCircle( Center, Picker.ReticleRadius - 4 );

        if ( Picker.ShowReticleCrossHairs )
        {
            var radius  =   (float)(Picker.ReticleRadius - 4);

            canvas.StrokeColor = Colors.Black;
            DrawCrossHairsHorizontal( canvas, Center, radius );
            DrawCrossHairsVertical( canvas, Center, radius );
        }
    }

    void DrawCrossHairsHorizontal( ICanvas canvas, PointF c, float r )
    {
        canvas.DrawLine( c.X - r + 1, c.Y, c.X - 4, c.Y );
        canvas.DrawLine( c.X + 4, c.Y, c.X + r - 1, c.Y );
    }

    void DrawCrossHairsVertical( ICanvas canvas, PointF c, float r )
    {
        canvas.DrawLine( c.X, c.Y - r + 1, c.X, c.Y - 4 );
        canvas.DrawLine( c.X, c.Y + 4, c.X, c.Y + r - 1 );
    }
}
