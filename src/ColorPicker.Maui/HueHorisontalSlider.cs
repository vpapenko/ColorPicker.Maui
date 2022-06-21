namespace ColorPicker.Maui
{
    public class HueHorisontalSlider : ColorPickerBase<Calculations.Slider.HueHorisontalSlider>
    {
        protected override void DrawBackground(ICanvas canvas, RectF dirtyRect)
        {
            LinearGradientPaint linearGradientPaint = new LinearGradientPaint()
            {
                StartColor = Colors.Red,
                EndColor = Colors.Red,
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5)
            };

            for (int i = 0; i <= 255; i++)
            {
                linearGradientPaint.AddOffset(i / 255F, Color.FromHsla(i / 255F, 1, 0.5));
            }

            canvas.SetFillPaint(linearGradientPaint, dirtyRect);
            canvas.FillRoundedRectangle(dirtyRect, 12);
        }
    }
}