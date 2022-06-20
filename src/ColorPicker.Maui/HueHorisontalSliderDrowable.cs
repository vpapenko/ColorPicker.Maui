namespace ColorPicker.Maui
{
    public class HueHorisontalSliderDrowable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeSize = 6;
            canvas.StrokeColor = Colors.Red;
            canvas.DrawRectangle(dirtyRect); ;
            canvas.StrokeColor = Colors.Green;
            canvas.DrawRectangle(dirtyRect.Size.Width / 4, dirtyRect.Height / 4, dirtyRect.Size.Width / 2, dirtyRect.Height / 2);
        }
    }
}
