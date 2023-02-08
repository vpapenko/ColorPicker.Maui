namespace ColorPicker;

public abstract class ColorPickerBaseDrawable : IDrawable
{
    public ColorPickerBase  Picker      { get; }
    public SizeF            CanvasSize  { get; set; }
    public PointF           Center      { get; set; }

    public ColorPickerBaseDrawable( ColorPickerBase picker )
    {
        Picker = picker;
    }

    public void Draw( ICanvas canvas, RectF dirtyRect )
    {
        canvas.Antialias    = true;
        CanvasSize          = new SizeF( dirtyRect.Width, dirtyRect.Height );

        if ( Center.X == -0.5 && Center.Y == -0.5 )
            Center = dirtyRect.Center;

        DrawBackground( canvas, dirtyRect );
        DrawContent( canvas, dirtyRect );
    }

    public virtual void DrawBackground( ICanvas canvas, RectF dirtyRect )   => throw new NotImplementedException();
    public virtual void DrawContent( ICanvas canvas, RectF dirtyRect )      => throw new NotImplementedException();

    /// <summary>
    /// Both an picker and a slider need to be able to draw a reticle
    /// </summary>
    public void DrawReticle( ICanvas canvas, RectF dirtyRect )
    {
        canvas.StrokeSize   = 2;
        canvas.StrokeColor  = Colors.White;
        canvas.DrawCircle( Center, Picker.ReticleRadius );

        canvas.StrokeColor  = Colors.Black;
        canvas.DrawCircle( Center, Picker.ReticleRadius - 2 );

        canvas.StrokeColor  = Colors.White;
        canvas.DrawCircle( Center, Picker.ReticleRadius - 4 );

        if ( Picker.ShowReticleCrossHairs )
        {
            var radius  =   (float)(Picker.ReticleRadius - 4);

            canvas.StrokeColor = Colors.Black;
            DrawHorizontalCrossHair( canvas, Center, radius );
            DrawVerticalCrossHair( canvas, Center, radius );
        }
    }

    void DrawHorizontalCrossHair( ICanvas canvas, PointF center, float radius )
    {
        canvas.DrawLine( center.X - radius + 1, center.Y, center.X - 4, center.Y );
        canvas.DrawLine( center.X + 4, center.Y, center.X + radius - 1, center.Y );
    }

    void DrawVerticalCrossHair( ICanvas canvas, PointF center, float radius )
    {
        canvas.DrawLine( center.X, center.Y - radius + 1, center.X, center.Y - 4 );
        canvas.DrawLine( center.X, center.Y + 4, center.X, center.Y + radius - 1 );
    }
}
