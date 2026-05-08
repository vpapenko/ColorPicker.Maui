using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class SettingsToggleTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fx;
    public SettingsToggleTests(AppFixture fx) => _fx = fx;

    [Fact]
    public void ShowAlphaSwitch_Toggles()
    {
        var sw = _fx.Page.ShowAlphaSwitch;
        var initial = _fx.Page.IsToggleOn("ShowAlphaSwitch");

        _fx.Page.Toggle(sw);
        Assert.NotEqual(initial, _fx.Page.IsToggleOn("ShowAlphaSwitch"));

        _fx.Page.Toggle(sw);
        Assert.Equal(initial, _fx.Page.IsToggleOn("ShowAlphaSwitch"));
    }

    [Fact]
    public void ShowTriangleSwitch_SwapsWheelForTriangle()
    {
        var sw = _fx.Page.ShowTriangleSwitch;
        var wasOn = _fx.Page.IsToggleOn(sw);
        try
        {
            if (!wasOn) _fx.Page.Toggle(sw);

            // Wait for the triangle host to become visible.
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var triangle = _fx.Page.ColorTriangle;
                    Assert.True(triangle.Size.Width > 50, "Triangle should be visible & sized.");
                    return;
                }
                catch { Thread.Sleep(200); }
            }
            Assert.Fail("ColorTriangle host never appeared after toggling ShowTriangleSwitch.");
        }
        finally
        {
            if (!wasOn && _fx.Page.IsToggleOn(sw)) _fx.Page.Toggle(sw);
        }
    }
}
