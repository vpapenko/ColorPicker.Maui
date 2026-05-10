namespace ColorPickerTestApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Allow UI tests to launch a parameterized layout-test harness instead
		// of MainPage by setting LAYOUT_TEST=1 in the process environment.
		var layoutTest = Environment.GetEnvironmentVariable("LAYOUT_TEST");
		MainPage = string.Equals(layoutTest, "1", StringComparison.Ordinal)
			? new LayoutTestPage()
			: new MainPage();
	}
}
