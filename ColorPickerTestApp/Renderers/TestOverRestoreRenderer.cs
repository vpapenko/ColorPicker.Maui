using ColorPicker.Rendering;

namespace ColorPickerTestApp.Renderers;

/// <summary>
/// UI-test renderer that deliberately over-restores the canvas before changing
/// its transform. Later elements must remain unaffected by this callback.
/// </summary>
public sealed class TestOverRestoreRenderer : ClassicColorPickerRenderer
{
    protected override void DrawCanvas(SKCanvas canvas, CanvasDrawingContext context)
    {
        canvas.Restore();
        canvas.Translate(50, 0);
        base.DrawCanvas(canvas, context);
    }
}
