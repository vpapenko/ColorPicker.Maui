using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// Shared logic for launching the test app and attaching a WindowsDriver.
/// Subclasses set environment variables (e.g. LAYOUT_TEST=1) by overriding
/// <see cref="ConfigureEnvironment"/>.
/// </summary>
public abstract class AppFixtureBase : IDisposable
{
    public WindowsDriver Driver { get; }
    private readonly Process _appProcess;
    /// <summary>HWND of the launched app's top-level window.</summary>
    public IntPtr AppHwnd { get; private set; }

    protected AppFixtureBase()
    {
        var appPath = ResolveAppPath();

        var psi = new ProcessStartInfo(appPath)
        {
            UseShellExecute  = false,
            WorkingDirectory = Path.GetDirectoryName(appPath)!,
        };
        ConfigureEnvironment(psi.EnvironmentVariables);

        _appProcess = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to launch " + appPath);

        var hwnd = WaitForTopLevelWindow(_appProcess, TimeSpan.FromSeconds(30));
        AppHwnd = hwnd;

        var options = new AppiumOptions
        {
            PlatformName   = "Windows",
            AutomationName = "Windows",
        };
        options.AddAdditionalAppiumOption("appTopLevelWindow", hwnd.ToInt64().ToString("x"));

        Driver = new WindowsDriver(new Uri(AppiumServerFixture.ServerUrl), options,
            TimeSpan.FromSeconds(120));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        // For the LayoutTest harness we pin the window to a fixed logical
        // size in App.CreateWindow so every machine / CI runner produces
        // identical layouts. Maximizing here would override that pin. We
        // therefore skip Maximize when LAYOUT_TEST=1 is in our env (the
        // env we propagated to the child app).
        bool layoutTest = false;
        var envDict = new System.Collections.Specialized.StringDictionary();
        ConfigureEnvironment(envDict);
        if (envDict.ContainsKey("LAYOUT_TEST") && envDict["LAYOUT_TEST"] == "1")
            layoutTest = true;
        if (!layoutTest)
        {
            try { Driver.Manage().Window.Maximize(); } catch { /* best effort */ }
        }
    }

    /// <summary>Override to add env vars (e.g. LAYOUT_TEST=1).</summary>
    protected virtual void ConfigureEnvironment(System.Collections.Specialized.StringDictionary env) { }

    private static IntPtr WaitForTopLevelWindow(Process p, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (p.HasExited)
                throw new InvalidOperationException(
                    $"App exited unexpectedly with code {p.ExitCode} before showing a window.");

            p.Refresh();
            if (p.MainWindowHandle != IntPtr.Zero && IsWindowVisible(p.MainWindowHandle))
                return p.MainWindowHandle;

            foreach (var sibling in Process.GetProcessesByName(p.ProcessName))
            {
                if (sibling.Id == p.Id) continue;
                if (sibling.MainWindowHandle != IntPtr.Zero && IsWindowVisible(sibling.MainWindowHandle))
                    return sibling.MainWindowHandle;
            }
            Thread.Sleep(250);
        }
        throw new TimeoutException("Timed out waiting for the test app's main window to appear.");
    }

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private static string ResolveAppPath()
    {
        var fromEnv = Environment.GetEnvironmentVariable("UITEST_APP_PATH");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
            return fromEnv;

        var here = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(here)
            ?? throw new InvalidOperationException("Could not locate repo root from " + here);

        foreach (var cfg in new[] { "Release", "Debug" })
        {
            var candidate = Path.Combine(repoRoot,
                "ColorPickerTestApp", "bin", cfg,
                "net8.0-windows10.0.19041.0", "win-x64", "ColorPickerTestApp.exe");
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException(
            "ColorPickerTestApp.exe not found. Build with: " +
            "dotnet build ColorPickerTestApp/ColorPickerTestApp.csproj " +
            "-c Release -f net8.0-windows10.0.19041.0 -r win-x64");
    }

    private static string? FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ColorPicker.Maui.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    public virtual void Dispose()
    {
        try { Driver.Quit(); } catch { /* best effort */ }
        Driver.Dispose();
        try
        {
            if (!_appProcess.HasExited)
            {
                _appProcess.Kill(entireProcessTree: true);
                _appProcess.WaitForExit(5000);
            }
        }
        catch { /* best effort */ }
        _appProcess.Dispose();
    }
}
