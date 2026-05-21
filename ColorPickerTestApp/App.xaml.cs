namespace ColorPickerTestApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Allow UI tests to launch alternate harnesses via env vars:
		//   LAYOUT_TEST=1 → LayoutTestPage (parameterized layout/visual probes)
		//   SYNC_TEST=1   → ColorSyncTestPage (all controls bound to one color, sync tests)
		var layoutTest = Environment.GetEnvironmentVariable("LAYOUT_TEST");
		var syncTest   = Environment.GetEnvironmentVariable("SYNC_TEST");
		MainPage = string.Equals(syncTest, "1", StringComparison.Ordinal)
			? new ColorSyncTestPage()
			: string.Equals(layoutTest, "1", StringComparison.Ordinal)
				? new LayoutTestPage()
				: new MainPage();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);
		// Pin window to a fixed logical size when running UI tests so that
		// `fill*` / `auto*` scenarios are deterministic across machines
		// (laptop, CI runner, different monitor resolutions).
		var isTest = string.Equals(Environment.GetEnvironmentVariable("LAYOUT_TEST"), "1", StringComparison.Ordinal)
		          || string.Equals(Environment.GetEnvironmentVariable("SYNC_TEST"),   "1", StringComparison.Ordinal);
		if (isTest)
		{
			window.X = 0;
			window.Y = 0;
			window.Width  = 1280;
			window.Height = 1024;
		}
		return window;
	}
}
