using ColorPicker.UITests.PageObjects;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// Per-test-class fixture: launches the sample app on MainPage and attaches a
/// WindowsDriver session by top-level window handle.
/// </summary>
public sealed class AppFixture : AppFixtureBase
{
    public MainPage Page { get; }

    public AppFixture()
    {
        Page = new MainPage(Driver);
        Page.WaitUntilLoaded();
    }
}
