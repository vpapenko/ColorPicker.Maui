using ColorPicker.UITests.Infrastructure;
using OpenQA.Selenium.Appium;
using Xunit;
using Xunit.Abstractions;

namespace ColorPicker.UITests.Tests;

[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class LayoutTestPageDiag : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fix;
    private readonly ITestOutputHelper _out;
    public LayoutTestPageDiag(LayoutTestAppFixture f, ITestOutputHelper o) { _fix = f; _out = o; }

    [Fact]
    public void Dump_Page_Elements()
    {
        var d = _fix.Driver;
        var src = d.PageSource;
        _out.WriteLine("--- PageSource (first 6000 chars) ---");
        _out.WriteLine(src.Substring(0, Math.Min(6000, src.Length)));

        foreach (var id in new[] { "ScenarioEntry", "ApplyScenario", "ScenarioApplied",
                                   "ScenarioHost", "ScenarioContent", "ScenarioStatus" })
        {
            try
            {
                var e = (AppiumElement)d.FindElement(MobileBy.AccessibilityId(id));
                _out.WriteLine($"FOUND {id}: loc={e.Location} sz={e.Size} text={e.Text}");
            }
            catch (Exception ex)
            {
                _out.WriteLine($"MISSING {id}: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
