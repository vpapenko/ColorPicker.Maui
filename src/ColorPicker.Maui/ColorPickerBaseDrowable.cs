namespace ColorPicker.Maui
{
    public class ColorPickerBaseDrowable : IDrawable
    {
        readonly Action<ICanvas, RectF> _drawBackground;
        readonly Action<ICanvas, RectF> _drawPicker;

        public ColorPickerBaseDrowable(Action<ICanvas, RectF> drawBackground, Action<ICanvas, RectF> drawPicker)
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
}
