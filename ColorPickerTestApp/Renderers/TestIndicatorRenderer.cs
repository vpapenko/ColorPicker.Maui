using ColorPicker.Rendering;

namespace ColorPickerTestApp.Renderers;

/// <summary>
/// UI-test renderer proving that one visual element can be replaced while every
/// other element continues to use the classic renderer.
/// </summary>
public sealed class TestIndicatorRenderer : ClassicColorPickerRenderer
{
    public static readonly SKColor TestColor = new(1, 2, 3);

    protected override void DrawIndicator(SKCanvas canvas, IndicatorDrawingContext context)
    {
        using var paint = new SKPaint
        {
            IsAntialias = false,
            Color = TestColor,
            Style = SKPaintStyle.Fill
        };

        var halfSize = context.Radius * 0.75F;
        canvas.DrawRect(
            context.Center.X - halfSize,
            context.Center.Y - halfSize,
            halfSize * 2,
            halfSize * 2,
            paint);
    }
}
