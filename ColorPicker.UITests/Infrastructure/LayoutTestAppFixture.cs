using ColorPicker.UITests.PageObjects;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>Per-test-class fixture that launches the app in
/// LAYOUT_TEST=1 mode (shows LayoutTestPage instead of MainPage).</summary>
public sealed class LayoutTestAppFixture : AppFixtureBase
{
    public LayoutTestPageObject Page { get; }

    public LayoutTestAppFixture()
    {
        Page = new LayoutTestPageObject(Driver);
        Page.WaitUntilLoaded();
    }

    protected override void ConfigureEnvironment(System.Collections.Specialized.StringDictionary env)
    {
        env["LAYOUT_TEST"] = "1";
    }
}
