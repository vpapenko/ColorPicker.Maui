namespace ColorPicker;

public class ColorPickerBaseDrawable : IDrawable
{
    public ColorPickerBase Picker { get; }
    public PointF Center { get; set; }

    public ColorPickerBaseDrawable( ColorPickerBase picker )
    {
        Picker = picker;
    }

    public void Draw( ICanvas canvas, RectF dirtyRect )
    {
        DrawBackground( canvas, dirtyRect );
        DrawContent( canvas, dirtyRect );
    }

    //  Must override
    public virtual void DrawBackground( ICanvas canvas, RectF dirtyRect )
            => throw new NotImplementedException();

    //  May override - default draws a reticle
    public virtual void DrawContent( ICanvas canvas, RectF dirtyRect )
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