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

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);
		// Pin window to a fixed logical size when running UI tests so that
		// `fill*` / `auto*` scenarios are deterministic across machines
		// (laptop, CI runner, different monitor resolutions).
		if (string.Equals(Environment.GetEnvironmentVariable("LAYOUT_TEST"), "1", StringComparison.Ordinal))
		{
			window.X = 0;
			window.Y = 0;
			window.Width  = 1280;
			window.Height = 1024;
		}
		return window;
	}
}
