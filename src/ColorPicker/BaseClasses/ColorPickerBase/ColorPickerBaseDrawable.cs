namespace ColorPicker;

public class ColorPickerBaseDrawable : IDrawable
{
    public ColorPickerBase Picker { get; }
    public PointF Center { get; set; }
    public SizeF CanvasSize { get; set; }

    public ColorPickerBaseDrawable( ColorPickerBase picker )
    {
        Picker = picker;
    }

    public void Draw( ICanvas canvas, RectF dirtyRect )
    {
        CanvasSize = new SizeF()
        {
            Width = dirtyRect.Width,
            Height = dirtyRect.Height
        };

        canvas.Antialias = true;

        DrawBackground( canvas, dirtyRect );
        DrawContent( canvas, dirtyRect );
    }

    public virtual void DrawBackground( ICanvas canvas, RectF dirtyRect ) => throw new NotImplementedException();
    public virtual void DrawContent( ICanvas canvas, RectF dirtyRect ) => throw new NotImplementedException();
}