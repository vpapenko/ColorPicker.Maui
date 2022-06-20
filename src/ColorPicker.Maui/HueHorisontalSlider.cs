namespace ColorPicker.Maui
{
    public class HueHorisontalSlider : ColorPickerBase<Calculations.Slider.HueHorisontalSlider>
    {
        protected override void DrawBackground(ICanvas canvas, RectF dirtyRect)
        {
            LinearGradientPaint linearGradientPaint = new LinearGradientPaint
            {
                StartColor = Colors.Yellow,
                EndColor = Colors.Green,
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            linearGradientPaint.AddOffset(0.25f, Colors.Red);
            linearGradientPaint.AddOffset(0.75f, Colors.Blue);

            canvas.SetFillPaint(linearGradientPaint, dirtyRect);
            canvas.SetShadow(new SizeF(10, 10), 10, Colors.Grey);
            canvas.FillRoundedRectangle(dirtyRect, 12);
        }
    }
}