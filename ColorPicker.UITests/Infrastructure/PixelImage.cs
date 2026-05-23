using SkiaSharp;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// A single RGBA pixel, deliberately matching ImageSharp's <c>Rgba32</c>
/// field names (<c>R/G/B/A</c>) so existing test code reads identically
/// after the migration off ImageSharp.
/// </summary>
public readonly record struct Pixel(byte R, byte G, byte B, byte A)
{
    public static implicit operator Pixel(SKColor c) => new(c.Red, c.Green, c.Blue, c.Alpha);
}

/// <summary>
/// Thin, disposable wrapper around <see cref="SKBitmap"/> that exposes
/// just the imaging surface the UI tests need:
///   - <c>Width/Height</c>
///   - <c>this[x, y]</c> pixel read
///   - <c>Load(bytes|path)</c>, <c>Save(path)</c>
///   - <c>Crop(x, y, w, h)</c>
///
/// We use SkiaSharp because the picker library already depends on it, so
/// no new transitive dep is introduced — and ImageSharp 4.0 went
/// commercial-license-only, which we're avoiding.
/// </summary>
public sealed class PixelImage : IDisposable
{
    private readonly SKBitmap _bitmap;

    private PixelImage(SKBitmap bitmap) => _bitmap = bitmap;

    public int Width  => _bitmap.Width;
    public int Height => _bitmap.Height;

    /// <summary>RGBA read of the pixel at (x, y). No bounds checking —
    /// callers (test code) are expected to clip first.</summary>
    public Pixel this[int x, int y] => _bitmap.GetPixel(x, y);

    public static PixelImage Load(byte[] bytes)
    {
        var bmp = SKBitmap.Decode(bytes)
            ?? throw new InvalidOperationException("SKBitmap.Decode returned null (corrupt or unsupported PNG bytes)");
        return new PixelImage(bmp);
    }

    public static PixelImage Load(string path)
    {
        var bmp = SKBitmap.Decode(path)
            ?? throw new InvalidOperationException($"SKBitmap.Decode returned null for path: {path}");
        return new PixelImage(bmp);
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var data = _bitmap.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("SKBitmap.Encode returned null");
        using var fs = File.Create(path);
        data.SaveTo(fs);
    }

    /// <summary>Return a new <see cref="PixelImage"/> covering the rectangle
    /// (x, y, w, h). Coordinates are clipped to the source bounds.</summary>
    public PixelImage Crop(int x, int y, int w, int h)
    {
        x = Math.Max(0, x);
        y = Math.Max(0, y);
        w = Math.Min(w, _bitmap.Width  - x);
        h = Math.Min(h, _bitmap.Height - y);

        var dst = new SKBitmap(w, h, _bitmap.ColorType, _bitmap.AlphaType);
        if (!_bitmap.ExtractSubset(dst, new SKRectI(x, y, x + w, y + h)))
        {
            // ExtractSubset can fail when row strides don't match; fall back
            // to a manual blit via SKCanvas.
            dst.Dispose();
            dst = new SKBitmap(w, h, _bitmap.ColorType, _bitmap.AlphaType);
            using var canvas = new SKCanvas(dst);
            canvas.DrawBitmap(_bitmap, new SKRect(x, y, x + w, y + h), new SKRect(0, 0, w, h));
        }
        return new PixelImage(dst);
    }

    public void Dispose() => _bitmap.Dispose();
}
