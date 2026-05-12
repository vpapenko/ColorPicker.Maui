using Microsoft.Maui.Controls.Internals;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace ColorPickerTestApp;

/// <summary>
/// Walks a MAUI visual subtree, captures every <see cref="SKCanvasView"/>'s
/// rendered surface, and composites them onto a single <see cref="SKBitmap"/>
/// at their on-screen positions. The result is the raw pixel output produced
/// by SkiaSharp before Windows compositor / DPI bitmap-stretching, so it is
/// fully deterministic across runs at the same DPI.
/// </summary>
internal static class CanvasCapture
{
    public sealed record Result(int PixelWidth, int PixelHeight, int CanvasCount);

    public static async Task<Result> CaptureAsync(
        View root,
        string outPath,
        int timeoutMs = 2000,
        SKColor? backgroundColor = null,
        double sceneWidth = 0,
        double sceneHeight = 0,
        IEnumerable<(double X, double Y, double W, double H, SKColor? Fill, SKColor? Stroke)>? overlays = null)
    {
        var canvases = new List<(SKCanvasView view, double logicalX, double logicalY)>();
        Collect(root, 0, 0, canvases);

        if (canvases.Count == 0 && (sceneWidth <= 0 || sceneHeight <= 0))
        {
            using var empty = new SKBitmap(1, 1);
            await SaveAsync(empty, outPath);
            return new Result(1, 1, 0);
        }

        // Hook each canvas to capture its next paint, then force a redraw.
        var snapshots = new Dictionary<SKCanvasView, SKImage>();
        var tcs = new TaskCompletionSource<bool>();
        int remaining = canvases.Count;
        var handlers = new Dictionary<SKCanvasView, EventHandler<SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs>>();

        if (canvases.Count == 0) tcs.TrySetResult(true);

        foreach (var (cv, _, _) in canvases)
        {
            EventHandler<SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs> h = null!;
            h = (s, e) =>
            {
                lock (snapshots)
                {
                    if (snapshots.ContainsKey(cv)) return;
                    snapshots[cv] = e.Surface.Snapshot();
                }
                cv.PaintSurface -= h;
                if (Interlocked.Decrement(ref remaining) == 0)
                    tcs.TrySetResult(true);
            };
            handlers[cv] = h;
            cv.PaintSurface += h;
            cv.InvalidateSurface();
        }

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
        if (completed != tcs.Task)
        {
            foreach (var (cv, h) in handlers)
                cv.PaintSurface -= h;
        }

        // Determine composite bounds. If the caller specified a scene size,
        // use that (scaled to physical pixels via the first canvas's DPI scale,
        // falling back to root.Width/Height ratio or 1:1). Otherwise compute
        // bounds from canvas placements only (legacy "tight" mode).
        double dpiScale = 1;
        foreach (var (cv, _, _) in canvases)
        {
            if (cv.Width > 0)
            {
                dpiScale = cv.CanvasSize.Width / cv.Width;
                if (dpiScale > 0) break;
            }
        }
        if (dpiScale <= 0) dpiScale = 1;

        int totalW = 0;
        int totalH = 0;
        var placed = new List<(SKImage img, int x, int y)>();
        foreach (var (cv, lx, ly) in canvases)
        {
            if (!snapshots.TryGetValue(cv, out var snap)) continue;
            var size = cv.CanvasSize;
            if (size.Width <= 0 || size.Height <= 0) continue;
            int x = (int)Math.Round(lx * dpiScale);
            int y = (int)Math.Round(ly * dpiScale);
            int right = x + (int)size.Width;
            int bottom = y + (int)size.Height;
            if (right > totalW) totalW = right;
            if (bottom > totalH) totalH = bottom;
            placed.Add((snap, x, y));
        }

        if (sceneWidth > 0 && sceneHeight > 0)
        {
            totalW = (int)Math.Round(sceneWidth * dpiScale);
            totalH = (int)Math.Round(sceneHeight * dpiScale);
        }

        if (totalW <= 0 || totalH <= 0)
        {
            using var empty = new SKBitmap(1, 1);
            await SaveAsync(empty, outPath);
            return new Result(1, 1, snapshots.Count);
        }

        using var composite = new SKBitmap(totalW, totalH);
        using (var canvas = new SKCanvas(composite))
        {
            canvas.Clear(backgroundColor ?? SKColors.Transparent);
            if (overlays != null)
            {
                foreach (var (ox, oy, ow, oh, fill, stroke) in overlays)
                {
                    var rect = new SKRect(
                        (float)(ox * dpiScale), (float)(oy * dpiScale),
                        (float)((ox + ow) * dpiScale), (float)((oy + oh) * dpiScale));
                    if (fill is { } f)
                        using (var p = new SKPaint { Color = f, Style = SKPaintStyle.Fill })
                            canvas.DrawRect(rect, p);
                    if (stroke is { } s)
                        using (var p = new SKPaint { Color = s, Style = SKPaintStyle.Stroke, StrokeWidth = (float)(2 * dpiScale) })
                            canvas.DrawRect(rect, p);
                }
            }
            foreach (var (img, x, y) in placed)
                canvas.DrawImage(img, x, y);
            if (overlays != null)
            {
                foreach (var (ox, oy, ow, oh, _, stroke) in overlays)
                {
                    if (stroke is { } s)
                    {
                        var rect = new SKRect(
                            (float)(ox * dpiScale), (float)(oy * dpiScale),
                            (float)((ox + ow) * dpiScale), (float)((oy + oh) * dpiScale));
                        using var p = new SKPaint { Color = s, Style = SKPaintStyle.Stroke, StrokeWidth = (float)(2 * dpiScale) };
                        canvas.DrawRect(rect, p);
                    }
                }
            }
        }

        try
        {
            foreach (var snap in snapshots.Values) snap.Dispose();
        }
        catch { }

        await SaveAsync(composite, outPath);
        return new Result(totalW, totalH, placed.Count);
    }

    static async Task SaveAsync(SKBitmap bmp, string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        await using var fs = File.Create(path);
        data.SaveTo(fs);
    }

    static void Collect(IView view, double x, double y, List<(SKCanvasView, double, double)> sink)
    {
        if (view is null) return;
        if (view is SKCanvasView cv) sink.Add((cv, x, y));
        if (view is Microsoft.Maui.ILayout layout)
        {
            foreach (var child in layout)
            {
                if (child is IView cv2)
                {
                    var f = cv2.Frame;
                    Collect(cv2, x + f.X, y + f.Y, sink);
                }
            }
        }
        else if (view is IContentView cont && cont.PresentedContent is IView inner)
        {
            var f = inner.Frame;
            Collect(inner, x + f.X, y + f.Y, sink);
        }
    }
}
