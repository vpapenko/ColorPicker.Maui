using System.Diagnostics;
using System.Runtime.InteropServices;
using ColorPicker.UITests.PageObjects;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// Per-test-class fixture: launches the sample app and attaches a
/// WindowsDriver session by top-level window handle. This is more reliable for
/// .NET MAUI Windows apps than letting WAD launch the .exe (MAUI's main window
/// is created asynchronously and may not be visible when WAD checks).
///
/// App location resolution order:
///   1. UITEST_APP_PATH env var (CI uses this)
///   2. ../ColorPickerTestApp/bin/Release/.../ColorPickerTestApp.exe
///   3. ../ColorPickerTestApp/bin/Debug/.../ColorPickerTestApp.exe
/// </summary>
public sealed class AppFixture : IDisposable
{
    public WindowsDriver Driver { get; }
    public MainPage Page { get; }

    private readonly Process _appProcess;

    public AppFixture()
    {
        var appPath = ResolveAppPath();

        _appProcess = Process.Start(new ProcessStartInfo(appPath)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(appPath)!,
        }) ?? throw new InvalidOperationException("Failed to launch " + appPath);

        var hwnd = WaitForTopLevelWindow(_appProcess, TimeSpan.FromSeconds(30));

        var options = new AppiumOptions
        {
            PlatformName = "Windows",
            AutomationName = "Windows",
        };
        options.AddAdditionalAppiumOption("appTopLevelWindow", hwnd.ToInt64().ToString("x"));

        Driver = new WindowsDriver(new Uri(AppiumServerFixture.ServerUrl), options,
            TimeSpan.FromSeconds(120));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        Page = new MainPage(Driver);
        Page.WaitUntilLoaded();
    }

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

            // Also probe sibling processes — some MAUI variants spawn the UI in a child.
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

        // Prefer Release (CI builds Release), fall back to Debug.
        foreach (var cfg in new[] { "Release", "Debug" })
        {
            var candidate = Path.Combine(repoRoot,
                "ColorPickerTestApp", "bin", cfg,
                "net8.0-windows10.0.19041.0", "win-x64", "ColorPickerTestApp.exe");
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException(
            "ColorPickerTestApp.exe not found in Release or Debug output. Build with: " +
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

    public void Dispose()
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

