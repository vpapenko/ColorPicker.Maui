using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class SmokeTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public SmokeTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void App_Launches_And_ShowsColorWheel()
    {
        var size = _fx.Page.ColorWheel.Size;
        Assert.True(size.Width > 50, $"ColorWheel width too small: {size.Width}");
        Assert.True(size.Height > 50, $"ColorWheel height too small: {size.Height}");
    }

    [Fact]
    public void Initial_ColorReadouts_Are_Populated()
    {
        Assert.False(string.IsNullOrWhiteSpace(_fx.Page.SelectedColorHex));
        Assert.StartsWith("#", _fx.Page.SelectedColorHex);
        Assert.Contains("RGBA", _fx.Page.SelectedColorRgba);
        Assert.Contains("HSLA", _fx.Page.SelectedColorHsla);
    }
}
