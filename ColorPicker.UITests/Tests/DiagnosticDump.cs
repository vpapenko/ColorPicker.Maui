using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Standalone diagnostic that launches the app, dumps the UIA tree as XML, and
/// quits. Not a [Fact] by default — enable via env var DIAG=1 to run manually.
/// </summary>
[Collection(Infrastructure.AppiumServerCollection.Name)]
public sealed class DiagnosticDump
{
    [Fact]
    public void Dump_PageSource()
    {
        if (Environment.GetEnvironmentVariable("DIAG") != "1") return; // skip by default

        var appPath = ResolveAppPath();
        var p = Process.Start(new ProcessStartInfo(appPath) { UseShellExecute = false })!;
        try
        {
            // wait for window
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            while (DateTime.UtcNow < deadline && (p.MainWindowHandle == IntPtr.Zero || !IsWindowVisible(p.MainWindowHandle)))
            {
                Thread.Sleep(250);
                p.Refresh();
            }
            var hwnd = p.MainWindowHandle;

            var options = new AppiumOptions { PlatformName = "Windows", AutomationName = "Windows" };
            options.AddAdditionalAppiumOption("appTopLevelWindow", hwnd.ToInt64().ToString("x"));
            using var driver = new WindowsDriver(new Uri(Infrastructure.AppiumServerFixture.ServerUrl), options, TimeSpan.FromSeconds(60));

            // Give MAUI time to render
            Thread.Sleep(3000);

            var src = driver.PageSource;
            var outPath = Path.Combine(Path.GetTempPath(), "uia-tree.xml");
            File.WriteAllText(outPath, src);
            Console.WriteLine("UIA tree dumped to: " + outPath + " (" + src.Length + " bytes)");

            driver.Quit();
        }
        finally
        {
            try { if (!p.HasExited) p.Kill(true); } catch { }
        }
    }

    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);

    private static string ResolveAppPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ColorPicker.Maui.sln")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("repo root not found");
        foreach (var cfg in new[] { "Release", "Debug" })
        {
            var c = Path.Combine(dir.FullName, "ColorPickerTestApp", "bin", cfg,
                "net10.0-windows10.0.19041.0", "win-x64", "ColorPickerTestApp.exe");
            if (File.Exists(c)) return c;
        }
        throw new FileNotFoundException("test app not built");
    }
}
