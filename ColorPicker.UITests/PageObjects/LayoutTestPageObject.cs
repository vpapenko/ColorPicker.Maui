using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace ColorPicker.UITests.PageObjects;

/// <summary>Page object for LayoutTestPage — drives the parameterized harness.</summary>
public sealed class LayoutTestPageObject
{
    private readonly WindowsDriver _driver;

    public LayoutTestPageObject(WindowsDriver driver) => _driver = driver;

    public AppiumElement ScenarioEntry  => Find("ScenarioEntry");
    public AppiumElement ApplyButton    => Find("ApplyScenario");
    public AppiumElement Host           => Find("ScenarioHost");
    public AppiumElement Control        => Find("ScenarioControl");
    public AppiumElement AppliedMarker  => Find("ScenarioApplied");

    public void WaitUntilLoaded()
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _ = ScenarioEntry; // throws if not present
                return;
            }
            catch (NoSuchElementException) { Thread.Sleep(250); }
            catch (WebDriverException)     { Thread.Sleep(250); }
        }
        throw new TimeoutException("LayoutTestPage did not display ScenarioEntry within 30s.");
    }

    /// <summary>
    /// Apply a scenario (e.g. "wheel:300x300" or "wheel:400x400:alpha,vertical")
    /// and wait until the page reports it as applied. Returns the parsed
    /// ScenarioState (host & control bounds in MAUI logical units).
    /// </summary>
    public ScenarioState Apply(string scenario)
    {
        SetEntryText(ScenarioEntry, scenario);
        ApplyButton.Click();

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var text = AppliedMarker.Text ?? "";
                if (text.StartsWith("ERROR:", StringComparison.Ordinal))
                    throw new InvalidOperationException("Page reported: " + text);

                if (ScenarioState.TryParse(text, out var state) &&
                    state.Spec == scenario &&
                    state.HostBounds.W > 0 && state.HostBounds.H > 0)
                    return state;
            }
            catch (WebDriverException) { /* element may briefly disappear */ }
            Thread.Sleep(150);
        }
        var last = AppliedMarker.Text ?? "<none>";
        throw new TimeoutException($"Scenario '{scenario}' did not finish applying within 10s. Last marker: {last}");
    }

    public Bounds GetBounds(AppiumElement element)
    {
        var loc  = element.Location;
        var size = element.Size;
        return new Bounds(loc.X, loc.Y, size.Width, size.Height);
    }

    private void SetEntryText(AppiumElement entry, string text)
    {
        entry.Click();
        try { entry.Clear(); } catch { /* fallback below */ }
        entry.SendKeys(text);
    }

    /// <summary>Read the current applied state from the marker label without
    /// re-applying. Useful after window-event triggered relayouts.</summary>
    public ScenarioState GetCurrentState()
    {
        if (ScenarioState.TryParse(AppliedMarker.Text ?? "", out var s))
            return s;
        throw new InvalidOperationException("Marker label has no valid state: " + (AppliedMarker.Text ?? "<null>"));
    }

    /// <summary>Wait until the marker reports the given predicate. Useful after
    /// async layout invalidations (e.g. window resize).</summary>
    public ScenarioState WaitForState(Func<ScenarioState, bool> predicate, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(5));
        ScenarioState last = default;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (ScenarioState.TryParse(AppliedMarker.Text ?? "", out last) && predicate(last))
                    return last;
            }
            catch (WebDriverException) { /* transient */ }
            Thread.Sleep(100);
        }
        throw new TimeoutException("Predicate not satisfied. Last state: " + last);
    }

    public AppiumElement Find(string automationId) =>
        (AppiumElement)_driver.FindElement(MobileBy.AccessibilityId(automationId));

    private AppiumElement FindInternal(string automationId) => Find(automationId);
}

/// <summary>Floating-point bounds in MAUI logical units (DPI-independent).</summary>
public readonly record struct LogicalBounds(double X, double Y, double W, double H)
{
    public override string ToString() => $"[{X:0.#},{Y:0.#} {W:0.#}x{H:0.#}]";
}

/// <summary>Snapshot of a scenario's resolved layout, parsed from the page's
/// AppliedLabel marker. Bounds are in MAUI logical units.</summary>
public readonly record struct ScenarioState(
    string Spec,
    LogicalBounds HostBounds,
    LogicalBounds ControlBounds,
    LogicalBounds ViewportBounds = default)
{
    public static bool TryParse(string text, out ScenarioState state)
    {
        state = default;
        if (string.IsNullOrEmpty(text)) return false;

        var parts = text.Split('|');
        // Accept legacy 3-segment markers (Tier 1/2 pages) plus the new
        // 4-segment marker that includes viewport bounds.
        if (parts.Length != 3 && parts.Length != 4) return false;

        if (!TryParseBounds(parts[1], out var host)) return false;
        if (!TryParseBounds(parts[2], out var ctrl)) return false;
        var viewport = default(LogicalBounds);
        if (parts.Length == 4 && !TryParseBounds(parts[3], out viewport))
            return false;

        state = new ScenarioState(parts[0], host, ctrl, viewport);
        return true;
    }

    static bool TryParseBounds(string s, out LogicalBounds b)
    {
        b = default;
        // "X,Y,WxH"
        var comma = s.Split(',');
        if (comma.Length != 3) return false;
        var sz = comma[2].Split('x');
        if (sz.Length != 2) return false;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        if (!double.TryParse(comma[0], System.Globalization.NumberStyles.Float, ci, out var x)) return false;
        if (!double.TryParse(comma[1], System.Globalization.NumberStyles.Float, ci, out var y)) return false;
        if (!double.TryParse(sz[0],   System.Globalization.NumberStyles.Float, ci, out var w)) return false;
        if (!double.TryParse(sz[1],   System.Globalization.NumberStyles.Float, ci, out var h)) return false;
        b = new LogicalBounds(x, y, w, h);
        return true;
    }
}

public readonly record struct Bounds(int X, int Y, int Width, int Height)
{
    public override string ToString() => $"[{X},{Y} {Width}x{Height}]";
}
