//namespace ColorPicker;

//public class RedHorizontalSlider : ColorPickerBase<RedHorizontalSliderMath>
//{
//    protected override void DrawBackground( ICanvas canvas, RectF dirtyRect )
//    {
//        var startColor  = Color.FromRgb(0, SelectedColor.Green, SelectedColor.Blue);
//        var endColor    = Color.FromRgb(1, SelectedColor.Green, SelectedColor.Blue);

//        var linearGradientPaint = new LinearGradientPaint()
//        {
//            StartColor  = startColor,
//            EndColor    = endColor,
//            StartPoint  = new Point(0, 0.5),
//            EndPoint    = new Point(1, 0.5)
//        };

//        canvas.SetFillPaint( linearGradientPaint, dirtyRect );
//        canvas.FillRectangle( dirtyRect );
//    }
//}