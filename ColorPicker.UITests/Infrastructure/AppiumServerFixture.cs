using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;

namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// xUnit collection fixture that ensures an Appium 2 server is running on
/// http://127.0.0.1:4723 for the duration of the test run.
///
/// If a server is already listening (e.g. started manually or by CI), it is
/// reused. Otherwise a new server is started by invoking 'appium' on PATH.
/// </summary>
public sealed class AppiumServerFixture : IDisposable
{
    public const string ServerUrl = "http://127.0.0.1:4723";
    private const int Port = 4723;

    private Process? _process;
    private readonly bool _ownsProcess;

    public AppiumServerFixture()
    {
        if (IsListening(Port))
        {
            _ownsProcess = false;
            WaitUntilReady(TimeSpan.FromSeconds(5));
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "appium.cmd" : "appium",
            Arguments = $"--port {Port} --log-level error",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        _process = Process.Start(psi)
            ?? throw new InvalidOperationException(
                "Could not start 'appium'. Install with: npm install -g appium@2 " +
                "and then: appium driver install --source=npm appium-windows-driver");
        _ownsProcess = true;

        WaitUntilReady(TimeSpan.FromSeconds(60));
    }

    private static bool IsListening(int port)
    {
        try
        {
            using var c = new TcpClient();
            var ar = c.BeginConnect("127.0.0.1", port, null, null);
            var ok = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
            if (!ok) return false;
            c.EndConnect(ar);
            return true;
        }
        catch { return false; }
    }

    private static void WaitUntilReady(TimeSpan timeout)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow + timeout;
        Exception? last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var r = http.GetAsync(ServerUrl + "/status").Result;
                if (r.IsSuccessStatusCode) return;
            }
            catch (Exception ex) { last = ex; }
            Thread.Sleep(500);
        }
        throw new TimeoutException("Appium server not ready at " + ServerUrl, last);
    }

    public void Dispose()
    {
        if (!_ownsProcess || _process is null) return;
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(5000);
            }
        }
        catch { /* best effort */ }
        _process.Dispose();
    }
}

[CollectionDefinition(Name)]
public sealed class AppiumServerCollection : ICollectionFixture<AppiumServerFixture>
{
    public const string Name = "Appium server";
}
