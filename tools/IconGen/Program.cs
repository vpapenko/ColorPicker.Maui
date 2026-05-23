// One-off generator for the NuGet package icon.
// Run: dotnet run --project tools/IconGen
// Output: ColorPicker/icon.png (128x128).

using SkiaSharp;

const int Size = 128;
const int Margin = 4;
var bitmap = new SKBitmap(Size, Size, SKColorType.Rgba8888, SKAlphaType.Premul);
using (var canvas = new SKCanvas(bitmap))
{
    canvas.Clear(SKColors.Transparent);

    var center = new SKPoint(Size / 2f, Size / 2f);
    var radius = (Size / 2f) - Margin;

    var colors = new SKColor[]
    {
        new(255,   0,   0), new(255, 255,   0),
        new(  0, 255,   0), new(  0, 255, 255),
        new(  0,   0, 255), new(255,   0, 255),
        new(255,   0,   0),
    };
    using var paint = new SKPaint
    {
        IsAntialias = true,
        Shader = SKShader.CreateSweepGradient(center, colors),
    };
    canvas.DrawCircle(center, radius, paint);

    using var fade = new SKPaint
    {
        IsAntialias = true,
        Shader = SKShader.CreateRadialGradient(
            center,
            radius,
            new[] { new SKColor(255, 255, 255, 220), new SKColor(255, 255, 255, 0) },
            new[] { 0f, 0.65f },
            SKShaderTileMode.Clamp),
    };
    canvas.DrawCircle(center, radius, fade);

    using var ring = new SKPaint
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.5f,
        Color = new SKColor(0, 0, 0, 64),
    };
    canvas.DrawCircle(center, radius, ring);
}

var outDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "ColorPicker"));
var outPath = Path.Combine(outDir, "icon.png");
using var img = SKImage.FromBitmap(bitmap);
using var data = img.Encode(SKEncodedImageFormat.Png, 100);
using var fs = File.OpenWrite(outPath);
data.SaveTo(fs);
Console.WriteLine($"Wrote {outPath} ({data.Size} bytes)");
