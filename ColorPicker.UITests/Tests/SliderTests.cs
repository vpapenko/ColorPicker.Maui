using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class SliderTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public SliderTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void DraggingHSLSliders_ChangesSelectedColor()
    {
        var before = _fx.Page.SelectedColorHex;
        // Drag horizontally near the top of the HSL panel — this hits the H slider.
        _fx.Page.DragInside(_fx.Page.HslSlider, 0.10, 0.20, 0.85, 0.20);
        Thread.Sleep(300);

        Assert.NotEqual(before, _fx.Page.SelectedColorHex);
    }

    [Fact]
    public void DraggingRGBSliders_ChangesSelectedColor()
    {
        var before = _fx.Page.SelectedColorHex;
        _fx.Page.DragInside(_fx.Page.RgbSlider, 0.10, 0.20, 0.85, 0.20);
        Thread.Sleep(300);

        Assert.NotEqual(before, _fx.Page.SelectedColorHex);
    }
}
