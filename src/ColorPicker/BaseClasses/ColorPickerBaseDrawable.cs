namespace ColorPicker;

public class ColorPickerBaseDrawable : IDrawable
{
    readonly Action<ICanvas, RectF> _drawBackground;
    readonly Action<ICanvas, RectF> _drawPicker;

    public ColorPickerBaseDrawable(Action<ICanvas, RectF> drawBackground, Action<ICanvas, RectF> drawPicker)
    {
        _drawBackground = drawBackground;
        _drawPicker = drawPicker;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _drawBackground(canvas, dirtyRect);
        _drawPicker(canvas, dirtyRect);
    }
}