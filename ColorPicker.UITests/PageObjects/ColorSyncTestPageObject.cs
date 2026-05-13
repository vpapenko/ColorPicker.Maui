using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace ColorPicker.UITests.PageObjects;

/// <summary>Page object for ColorSyncTestPage. Drives a 5x4 grid of
/// picker variants all bound to one MasterWheel.</summary>
public sealed class ColorSyncTestPageObject
{
    private readonly WindowsDriver _driver;
    public ColorSyncTestPageObject(WindowsDriver driver) => _driver = driver;

    public AppiumElement InputHex     => Find("InputHex");
    public AppiumElement InputApply   => Find("InputApply");
    public AppiumElement InputPreset  => Find("InputPreset");
    public AppiumElement InputSwatch  => Find("InputSwatch");
    public AppiumElement OutputSwatch => Find("OutputSwatch");
    public AppiumElement OutputHex    => Find("OutputHex");
    public AppiumElement OutputRgba   => Find("OutputRgba");

    public string OutputHexText  => SafeText(OutputHex);
    public string OutputRgbaText => SafeText(OutputRgba);

    public AppiumElement Find(string automationId) =>
        (AppiumElement)_driver.FindElement(MobileBy.AccessibilityId(automationId));

    public void WaitUntilLoaded()
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _ = InputHex; // throws if not present
                // Wait until OnAppearing's SetMaster has populated outputs.
                if (!string.IsNullOrEmpty(OutputHexText) && OutputHexText.StartsWith("#"))
                    return;
            }
            catch (NoSuchElementException) { }
            catch (WebDriverException)     { }
            Thread.Sleep(200);
        }
        throw new TimeoutException("ColorSyncTestPage did not finish loading within 30s.");
    }

    /// <summary>Set the master color via the hex Entry + Apply button.
    /// Format: "#RRGGBBAA". Waits until OutputHex reflects the new value.</summary>
    public void SetHex(string hex)
    {
        SetEntryText(InputHex, hex);
        InputApply.Click();
        WaitForOutputHex(hex);
    }

    public void WaitForOutputHex(string expectedHex, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(5));
        string last = "";
        while (DateTime.UtcNow < deadline)
        {
            last = OutputHexText;
            if (string.Equals(last, expectedHex, StringComparison.OrdinalIgnoreCase))
                return;
            Thread.Sleep(80);
        }
        throw new TimeoutException(
            $"OutputHex did not reach '{expectedHex}' within timeout. Last value: '{last}'.");
    }

    private void SetEntryText(AppiumElement entry, string text)
    {
        // MAUI Entry on Windows Appium: Clear() is unreliable and SendKeys appends.
        // Select-all + Delete via keystrokes is the only robust clear path.
        entry.Click();
        Thread.Sleep(50);
        entry.SendKeys(Keys.Control + "a" + Keys.Control);
        entry.SendKeys(Keys.Delete);
        Thread.Sleep(30);
        entry.SendKeys(text);

        // Verification + one retry.
        try
        {
            var actual = entry.Text ?? "";
            if (actual != text)
            {
                entry.Click();
                Thread.Sleep(50);
                entry.SendKeys(Keys.Control + "a" + Keys.Control);
                entry.SendKeys(Keys.Delete);
                Thread.Sleep(30);
                entry.SendKeys(text);
            }
        }
        catch (WebDriverException) { }
    }

    /// <summary>Capture the full window screenshot as an RGBA image.</summary>
    public Image<Rgba32> CaptureWindow()
    {
        var bytes = _driver.GetScreenshot().AsByteArray;
        return SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
    }

    /// <summary>Bounds (in screen pixels) of an element within the captured window image.
    /// Appium reports element coordinates in screen pixels for Windows desktop apps,
    /// matching what GetScreenshot() returns.</summary>
    public Bounds GetBounds(string automationId)
    {
        var e = Find(automationId);
        var loc = e.Location;
        var size = e.Size;
        return new Bounds(loc.X, loc.Y, size.Width, size.Height);
    }

    /// <summary>
    /// Computes the cell's wheel-area bounds (the square region containing the
    /// SkiaSharp picker, ABOVE the cell caption label). MAUI Border + SkiaSharp
    /// views aren't reliably UIA-discoverable, so we anchor on the cell label
    /// (which IS discoverable) and the input bar to derive the cell rectangle.
    /// </summary>
    public Bounds GetWheelAreaBounds(string cellAutomationId)
    {
        // Bottom anchor: cell caption label at Grid.Row="1" of the inner cell grid.
        var label = GetBounds(cellAutomationId + "_label");
        // Top anchor: bottom of the input bar (above the controls grid).
        var input = GetBounds("InputApply");
        var topGap = 8;          // approximate grid + page padding above row 0
        var bottomGap = 4;       // gap between wheel-area and caption label

        // Find which row this cell is in by guessing based on label.Y vs input.Y:
        // The 4 rows are evenly spaced; total grid height = label.Y(rowN) - input.Bottom.
        // We don't know N here, so instead derive cell-top by walking up by the
        // computed row-height. We approximate row-height = (label.Y - input.Bottom)
        // wouldn't work without knowing the row. Simpler: query OutputHex (always
        // at the bottom) to bound the grid, then compute equally-spaced rows.
        var output = GetBounds("OutputHex");
        var gridTop = input.Y + input.Height + topGap;
        var gridBottom = output.Y - topGap;
        // Determine row index by which quartile of [gridTop, gridBottom] the label is in.
        var labelMidY = label.Y + label.Height / 2;
        var rowH = (gridBottom - gridTop) / 4.0;
        int rowIdx = Math.Clamp((int)Math.Floor((labelMidY - gridTop) / rowH), 0, 3);
        var cellTop = (int)(gridTop + rowIdx * rowH);
        var wheelBottom = label.Y - bottomGap;

        // Width: cell spans the column. Use label X as cell center (label has
        // HorizontalOptions=Center, but in practice spans most of the cell width).
        // We compute column width from the grid's full width / 5 columns.
        var cellWidth = (output.Width > 0) ? (label.Width + 16) : label.Width;
        // Better: derive column from the page's full width by querying OutputRgba
        // (full-bottom-bar). We use the label's horizontal extent expanded by a
        // small border padding as the cell width approximation.
        var cellLeft = label.X - 2;
        return new Bounds(cellLeft, cellTop, label.Width + 4, wheelBottom - cellTop);
    }

    private static string SafeText(AppiumElement e) { try { return e.Text ?? ""; } catch { return ""; } }

    /// <summary>Tap at absolute screen coordinates (uses a touch pointer with a
    /// short dwell so SkiaSharp gesture recognizers register the press).</summary>
    public void TapAt(int x, int y)
    {
        var finger = new PointerInputDevice(PointerKind.Touch, "finger");
        var seq = new ActionSequence(finger, 0);
        seq.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
        seq.AddAction(finger.CreatePointerDown(MouseButton.Left));
        seq.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(120)));
        seq.AddAction(finger.CreatePointerUp(MouseButton.Left));
        _driver.PerformActions(new List<ActionSequence> { seq });
    }

    /// <summary>Tap inside a cell at normalized (rx, ry) ∈ [0,1] of its
    /// <see cref="GetWheelAreaBounds"/> rectangle. Pre-computed offsets for
    /// known colors live in <c>ColorSyncExpectedPickerOffsets</c>.</summary>
    public void TapAtRel(string cellAutomationId, double rx, double ry)
    {
        var b = GetWheelAreaBounds(cellAutomationId);
        TapAt(b.X + (int)Math.Round(rx * b.Width),
              b.Y + (int)Math.Round(ry * b.Height));
    }

    /// <summary>Tap inside a cell's wheel-area at a polar position relative to its
    /// centered square. <paramref name="hue"/> is 0..1 (0 = right / 3 o'clock, going
    /// counter-clockwise to match ColorWheel paint), <paramref name="sat"/> is 0..1
    /// (0 = center, 1 = edge of disc). <paramref name="discFraction"/> is the
    /// fraction of the half-square that the disc occupies.</summary>
    public void TapPolar(string cellAutomationId, double hue, double sat, double discFraction = 0.32)
    {
        var b = GetWheelAreaBounds(cellAutomationId);
        var side = Math.Min(b.Width, b.Height);
        var cx = b.X + (b.Width  - side) / 2.0 + side / 2.0;
        var cy = b.Y + (b.Height - side) / 2.0 + side / 2.0;
        var radius = side / 2.0 * discFraction * sat * 2.0;
        // ColorCircle convention: angleHS = (0.5 - hue) * 2π = π - hue*2π
        // (tested against ColorCircle.cs line 167 — hue=0/red is at 9 o'clock).
        var theta = Math.PI - hue * 2 * Math.PI;
        TapAt((int)Math.Round(cx + radius * Math.Cos(theta)),
              (int)Math.Round(cy + radius * Math.Sin(theta)));
    }

    /// <summary>Wait until OutputHex changes to something different from
    /// <paramref name="previousHex"/>. Returns the new value, or throws on timeout.</summary>
    public string WaitForOutputHexChange(string previousHex, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(3));
        string last = previousHex;
        while (DateTime.UtcNow < deadline)
        {
            last = OutputHexText;
            if (!string.Equals(last, previousHex, StringComparison.OrdinalIgnoreCase))
                return last;
            Thread.Sleep(50);
        }
        throw new TimeoutException(
            $"OutputHex never changed from '{previousHex}' (last value: '{last}').");
    }
}
