using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using ColorPicker.UITests.PageObjects;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// Captures the desktop via Appium and provides logical-to-pixel coordinate
/// translation, anchored on the ScenarioApplied label (a UIA-visible element
/// that lives in the same Grid column as HostContainer, so its pixel width
/// gives the DPI scale and its pixel position lets us derive the origin).
/// </summary>
public static class Screenshot
{
    public static Image<Rgba32> Capture(WindowsDriver driver)
    {
        var bytes = driver.GetScreenshot().AsByteArray;
        return SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
    }

    /// <summary>Map a logical-units rectangle (as reported by the marker)
    /// into image-pixel coordinates inside a screenshot of the desktop.
    /// The ScenarioApplied label is used as the anchor.</summary>
    public static PixelRect ToPixels(LogicalBounds logical, ScenarioState state, AppiumElement anchor)
    {
        // dpiScale: pixel width of anchor / its logical width.
        // The AppliedLabel sits in the same Grid (row 1) so its logical width
        // equals viewport.W (HostContainer fills row 2 of the same Grid).
        double scale = anchor.Size.Width / Math.Max(1.0, state.ViewportBounds.W);

        // The label's logical Y = viewport.Y - label.Height_logical.
        double labelLogicalH = anchor.Size.Height / scale;
        double labelLogicalY = state.ViewportBounds.Y - labelLogicalH;

        // Origin (logical 0,0) in screen pixels:
        int originX = anchor.Location.X - (int)Math.Round(state.ViewportBounds.X * scale);
        int originY = anchor.Location.Y - (int)Math.Round(labelLogicalY * scale);

        return new PixelRect(
            X: originX + (int)Math.Round(logical.X * scale),
            Y: originY + (int)Math.Round(logical.Y * scale),
            W: (int)Math.Round(logical.W * scale),
            H: (int)Math.Round(logical.H * scale),
            DpiScale: scale);
    }

    public static Rgba32 SampleBox(Image<Rgba32> img, int cx, int cy, int size = 5)
    {
        int half = size / 2;
        long r = 0, g = 0, b = 0, a = 0, n = 0;
        for (int dy = -half; dy <= half; dy++)
        for (int dx = -half; dx <= half; dx++)
        {
            int x = cx + dx, y = cy + dy;
            if (x < 0 || y < 0 || x >= img.Width || y >= img.Height) continue;
            var p = img[x, y];
            r += p.R; g += p.G; b += p.B; a += p.A; n++;
        }
        if (n == 0) return new Rgba32(0, 0, 0, 0);
        return new Rgba32((byte)(r / n), (byte)(g / n), (byte)(b / n), (byte)(a / n));
    }

    public static int ColorDistance(Rgba32 a, Rgba32 b) =>
        Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
}

public readonly record struct PixelRect(int X, int Y, int W, int H, double DpiScale)
{
    public int Right   => X + W;
    public int Bottom  => Y + H;
    public int CenterX => X + W / 2;
    public int CenterY => Y + H / 2;
}

