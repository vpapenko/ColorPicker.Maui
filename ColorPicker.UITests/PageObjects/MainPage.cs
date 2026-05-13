using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace ColorPicker.UITests.PageObjects;

/// <summary>
/// Page object for ColorPickerTestApp's MainPage. All lookups are by
/// AutomationId so the same page object can later drive Android / iOS once
/// those drivers are added.
/// </summary>
public sealed class MainPage
{
    private readonly WindowsDriver _driver;
    public MainPage(WindowsDriver driver) => _driver = driver;

    // Switches
    public AppiumElement ShowTriangleSwitch         => Find("ShowTriangleSwitch");
    public AppiumElement ShowAlphaSwitch            => Find("ShowAlphaSwitch");
    public AppiumElement ShowLuminositySliderSwitch => Find("ShowLuminositySlider");
    public AppiumElement ShowLuminosityWheelSwitch  => Find("ShowLuminosityRing");
    public AppiumElement ShowVerticalSliderSwitch   => Find("ShowVerticalSlider");
    public AppiumElement RotateTriangleByHueSwitch  => Find("RotateTriangleByHue");

    // Color readouts
    public string SelectedColorHex  => Find("SelectedColorHex").Text;
    public string SelectedColorRgba => Find("SelectedColorRGBA").Text;
    public string SelectedColorHsla => Find("SelectedColorHSLA").Text;

    // Custom controls — UIA-opaque (SkiaSharp surface). We expose them via an
    // outer Border host (AutomationId="*Host"). Tests use coordinate-based
    // gestures within the host's bounds.
    public AppiumElement ColorWheel    => Find("ColorWheel1Host");
    public AppiumElement ColorTriangle => Find("ColorTriangle1Host");
    public AppiumElement HSLSliders    => Find("HSLSliders1Host");
    public AppiumElement RGBSliders    => Find("RGBSliders1Host");

    public void WaitUntilLoaded()
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _ = ColorWheel; // throws if not present
                return;
            }
            catch (NoSuchElementException) { Thread.Sleep(250); }
            catch (WebDriverException)     { Thread.Sleep(250); }
        }
        throw new TimeoutException("Sample app did not display ColorWheel within 30s.");
    }

    /// <summary>Toggle a Switch. Tapping via PointerActions is more reliable than
    /// driver Click() on a WinUI ToggleSwitch (Click can be a no-op when it lands on
    /// the label region instead of the thumb).</summary>
    public void Toggle(AppiumElement toggle)
    {
        TapInside(toggle, 0.85, 0.50);
        // Allow UIA to settle so a follow-up ToggleState read reflects the new value.
        Thread.Sleep(150);
    }

    /// <summary>Read whether a Switch is on. UIA exposes ToggleState as "On"/"Off" in
    /// the page source; WAD's <c>GetAttribute("ToggleState")</c> doesn't expose it
    /// directly, so we query the page source by AutomationId.</summary>
    public bool IsToggleOn(string automationId)
    {
        var src = _driver.PageSource;
        // Find the element fragment that has this AutomationId, then look for ToggleState within it.
        var key = $"AutomationId=\"{automationId}\"";
        var idx = src.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return false;
        // Look forward to next "/>" — the element's attributes end there.
        var end = src.IndexOf("/>", idx, StringComparison.Ordinal);
        if (end < 0) end = Math.Min(src.Length, idx + 2000);
        var fragment = src.Substring(idx, end - idx);
        var m = System.Text.RegularExpressions.Regex.Match(fragment, @"ToggleState=""(On|Off)""");
        return m.Success && m.Groups[1].Value == "On";
    }

    /// <summary>Convenience overload that reads ToggleState for a known AppiumElement
    /// by re-querying the page source via the element's AutomationId attribute.</summary>
    public bool IsToggleOn(AppiumElement toggle)
    {
        var id = toggle.GetAttribute("AutomationId") ?? string.Empty;
        return IsToggleOn(id);
    }

    /// <summary>Tap at a normalized point (0..1) inside the given control's bounds.</summary>
    public void TapInside(AppiumElement element, double normalizedX, double normalizedY)
    {
        var loc  = element.Location;
        var size = element.Size;
        int x = loc.X + (int)(size.Width  * normalizedX);
        int y = loc.Y + (int)(size.Height * normalizedY);

        Tap(x, y);
    }

    /// <summary>Tap inside the centered square of a control (useful for circular controls
    /// like ColorWheel where the host Border is wider than the visible disc).</summary>
    public void TapInsideSquare(AppiumElement element, double normalizedX, double normalizedY)
    {
        var (x, y) = SquareCenteredPoint(element, normalizedX, normalizedY);
        Tap(x, y);
    }

    private void Tap(int x, int y)
    {
        // SkiaSharp gesture recognizers on MAUI Windows require a small dwell
        // between down and up; an instantaneous click is dropped.
        var finger = new PointerInputDevice(PointerKind.Touch, "finger");
        var seq = new ActionSequence(finger, 0);
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
        seq.AddAction(finger.CreatePointerDown(MouseButton.Left));
        seq.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(120)));
        seq.AddAction(finger.CreatePointerUp(MouseButton.Left));
        _driver.PerformActions(new List<ActionSequence> { seq });
    }

    /// <summary>Drag inside the centered square of a control.</summary>
    public void DragInsideSquare(AppiumElement element,
        double fromNormX, double fromNormY,
        double toNormX,   double toNormY)
    {
        var (x1, y1) = SquareCenteredPoint(element, fromNormX, fromNormY);
        var (x2, y2) = SquareCenteredPoint(element, toNormX,   toNormY);
        var finger = new PointerInputDevice(PointerKind.Touch, "finger");
        var seq = new ActionSequence(finger, 0);
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x1, y1, TimeSpan.Zero));
        seq.AddAction(finger.CreatePointerDown(MouseButton.Left));
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x2, y2, TimeSpan.FromMilliseconds(300)));
        seq.AddAction(finger.CreatePointerUp(MouseButton.Left));
        _driver.PerformActions(new List<ActionSequence> { seq });
    }

    private static (int x, int y) SquareCenteredPoint(AppiumElement element, double nx, double ny)
    {
        var loc  = element.Location;
        var size = element.Size;
        var side = Math.Min(size.Width, size.Height);
        var offX = loc.X + (size.Width  - side) / 2;
        var offY = loc.Y + (size.Height - side) / 2;
        return (offX + (int)(side * nx), offY + (int)(side * ny));
    }

    /// <summary>Drag from one normalized point to another inside the control.</summary>
    public void DragInside(AppiumElement element,
        double fromNormX, double fromNormY,
        double toNormX,   double toNormY)
    {
        var loc  = element.Location;
        var size = element.Size;
        int x1 = loc.X + (int)(size.Width  * fromNormX);
        int y1 = loc.Y + (int)(size.Height * fromNormY);
        int x2 = loc.X + (int)(size.Width  * toNormX);
        int y2 = loc.Y + (int)(size.Height * toNormY);

        var finger = new PointerInputDevice(PointerKind.Touch, "finger");
        var seq = new ActionSequence(finger, 0);
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x1, y1, TimeSpan.Zero));
        seq.AddAction(finger.CreatePointerDown(MouseButton.Left));
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x2, y2, TimeSpan.FromMilliseconds(300)));
        seq.AddAction(finger.CreatePointerUp(MouseButton.Left));
        _driver.PerformActions(new List<ActionSequence> { seq });
    }

    private AppiumElement Find(string automationId) =>
        (AppiumElement)_driver.FindElement(MobileBy.AccessibilityId(automationId));
}
