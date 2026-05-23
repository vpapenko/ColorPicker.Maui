using ColorPicker.UITests.PageObjects;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>Per-test-class fixture that launches the app in
/// SYNC_TEST=1 mode (shows ColorSyncTestPage instead of MainPage).</summary>
public sealed class SyncTestAppFixture : AppFixtureBase
{
    public ColorSyncTestPageObject Page { get; }

    public SyncTestAppFixture()
    {
        Page = new ColorSyncTestPageObject(Driver);
        Page.WaitUntilLoaded();
    }

    protected override void ConfigureEnvironment(System.Collections.Specialized.StringDictionary env)
    {
        // SYNC_TEST + LAYOUT_TEST both pin the window in App.CreateWindow,
        // so the same "skip Maximize" logic in AppFixtureBase applies.
        env["SYNC_TEST"] = "1";
        env["LAYOUT_TEST"] = "1";
    }
}
