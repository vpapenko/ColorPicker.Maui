using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class ColorWheelInteractionTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public ColorWheelInteractionTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void DraggingToHueRing_ProducesSaturatedColor()
    {
        var before = _fx.Page.SelectedColorHex;

        // Drag from wheel center outward toward the rim — lands on the hue ring
        // and selects a fully-saturated color (some channel near 0 or 255).
        _fx.Page.DragInsideSquare(_fx.Page.ColorWheel, 0.50, 0.50, 0.90, 0.50);
        Thread.Sleep(300);

        var after = _fx.Page.SelectedColorHex;
        Assert.NotEqual(before, after);
        Assert.StartsWith("#", after);

        var r = Convert.ToInt32(after.Substring(1, 2), 16);
        var g = Convert.ToInt32(after.Substring(3, 2), 16);
        var b = Convert.ToInt32(after.Substring(5, 2), 16);

        // Saturated colors have at least one channel near 255 and another near 0.
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        Assert.True(max >= 200 && min <= 50,
            $"Expected a saturated color after dragging to wheel rim; got {after} (r={r},g={g},b={b}).");
    }

    [Fact]
    public void DraggingAcrossWheel_ChangesSelectedColor()
    {
        var before = _fx.Page.SelectedColorHex;
        _fx.Page.DragInsideSquare(_fx.Page.ColorWheel, 0.50, 0.50, 0.10, 0.50);
        Thread.Sleep(300);

        Assert.NotEqual(before, _fx.Page.SelectedColorHex);
    }
}
