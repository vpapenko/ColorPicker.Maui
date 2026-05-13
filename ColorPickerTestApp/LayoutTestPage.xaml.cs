using ColorPicker.BaseClasses;
using ColorPicker.Controls;

namespace ColorPickerTestApp;

public partial class LayoutTestPage : ContentPage
{
    public LayoutTestPage()
    {
        InitializeComponent();
        // Apply default scenario on initial render so a test that doesn't
        // explicitly set one still has something to query.
        Loaded += (_, _) => ApplyScenario(ScenarioEntry.Text);
    }

    void OnApplyClicked(object sender, EventArgs e) => ApplyScenario(ScenarioEntry.Text);

    static int _captureSeq;

    async void OnCaptureClicked(object sender, EventArgs e) => await CaptureAsync(null);

    async Task CaptureAsync(string? outPath)
    {
        var seq = Interlocked.Increment(ref _captureSeq);
        try
        {
            outPath ??= Path.Combine(Path.GetTempPath(), "colorpicker-capture.png");
            CapturePathLabel.Text = $"capturing #{seq}";
            await Task.Yield();
            await Task.Delay(50);
            View? root = HostContainer;
            if (root is null)
            {
                CapturePathLabel.Text = $"#{seq} no content";
                return;
            }
            // Scene-composite mode: paint HostContainer's BG (yellow in debug) so
            // we see the whole viewport with all 3 borders (yellow > white > red).
            var bg = HostContainer.BackgroundColor ?? Colors.White;
            var skBg = new SkiaSharp.SKColor(
                (byte)(bg.Red   * 255),
                (byte)(bg.Green * 255),
                (byte)(bg.Blue  * 255),
                (byte)(bg.Alpha * 255));
            // Compute HostBorder + PickerOutline positions in HostContainer coords.
            Point Loc(View v)
            {
                double x = 0, y = 0;
                Element? e = v;
                while (e is View vv && e != (Element)HostContainer)
                {
                    x += vv.X; y += vv.Y;
                    e = vv.Parent as Element;
                }
                return new Point(x, y);
            }
            var hbLoc = Loc(HostBorder);
            var poLoc = Loc(PickerOutline);
            var overlays = new[]
            {
                ((double)hbLoc.X, (double)hbLoc.Y, HostBorder.Width, HostBorder.Height,
                 (SkiaSharp.SKColor?)new SkiaSharp.SKColor(255,255,0), (SkiaSharp.SKColor?)new SkiaSharp.SKColor(0,0,255)),
                ((double)poLoc.X, (double)poLoc.Y, PickerOutline.Width, PickerOutline.Height,
                 (SkiaSharp.SKColor?)null, (SkiaSharp.SKColor?)new SkiaSharp.SKColor(255,0,0)),
            };
            var result = await CanvasCapture.CaptureAsync(
                root, outPath,
                backgroundColor: skBg,
                sceneWidth: HostContainer.Width,
                sceneHeight: HostContainer.Height,
                overlays: overlays);
            CapturePathLabel.Text = $"#{seq} saved {result.PixelWidth}x{result.PixelHeight} ({result.CanvasCount} canvases): {outPath}";
        }
        catch (Exception ex)
        {
            CapturePathLabel.Text = $"#{seq} ERROR: " + ex.Message;
        }
    }

    void OnHostSizeChanged(object sender, EventArgs e) => UpdateAppliedMarker();

    string _lastSpec = "";
    string _lastControl = "";
    string _lastSizeKey = "";
    string _lastFeatureKey = "";

    static string MakeFeatureKey(string control, string[] opts)
    {
        // Only feature flags that affect the picker's children list matter for
        // the rebuild decision. bg/wbg/etc. are safe to mutate at runtime.
        if (control == "wheel")
        {
            var flags = new List<string>();
            foreach (var o in opts)
            {
                var t = o.Trim().ToLowerInvariant();
                if (t is "alpha" or "lumslider" or "nolumwheel" or "vertical")
                    flags.Add(t);
            }
            flags.Sort(StringComparer.Ordinal);
            return string.Join(",", flags);
        }
        if (control == "triangle")
        {
            var flags = new List<string>();
            foreach (var o in opts)
            {
                var t = o.Trim().ToLowerInvariant();
                if (t is "norotate" or "rotate") flags.Add(t);
            }
            flags.Sort(StringComparer.Ordinal);
            return string.Join(",", flags);
        }
        if (control == "hsl" || control == "rgb")
        {
            var flags = new List<string>();
            foreach (var o in opts)
            {
                var t = o.Trim().ToLowerInvariant();
                if (t is "vertical" or "noalpha") flags.Add(t);
            }
            flags.Sort(StringComparer.Ordinal);
            return string.Join(",", flags);
        }
        return "";
    }

    void ApplyHostSizing(SizeMode wMode, double wValue, SizeMode hMode, double hValue)
    {
        // HostBorder = the requested cell. The control sits inside it (via
        // PickerOutline + ScenarioContent) and reports its own DesiredSize;
        // for aspect-locked controls (wheel, triangle) that DesiredSize is
        // the natural square so PickerOutline auto-wraps to just the useful
        // area. For free-aspect controls (sliders) DesiredSize == the cell.
        //
        // Width
        switch (wMode)
        {
            case SizeMode.Fixed:
                HostBorder.WidthRequest      = wValue;
                HostBorder.HorizontalOptions = LayoutOptions.Start;
                break;
            case SizeMode.Auto:
                HostBorder.WidthRequest      = -1;
                HostBorder.HorizontalOptions = LayoutOptions.Start;
                break;
            case SizeMode.Fill:
                HostBorder.WidthRequest      = -1;
                HostBorder.HorizontalOptions = LayoutOptions.Fill;
                break;
        }
        // Height
        switch (hMode)
        {
            case SizeMode.Fixed:
                HostBorder.HeightRequest    = hValue;
                HostBorder.VerticalOptions  = LayoutOptions.Start;
                break;
            case SizeMode.Auto:
                HostBorder.HeightRequest    = -1;
                HostBorder.VerticalOptions  = LayoutOptions.Start;
                break;
            case SizeMode.Fill:
                HostBorder.HeightRequest    = -1;
                HostBorder.VerticalOptions  = LayoutOptions.Fill;
                break;
        }
    }

    void OnPickerOutlineSizeChanged(object sender, EventArgs e) => UpdateAppliedMarker();

    void UpdateAppliedMarker()
    {
        // Format: "spec|hostX,Y,WxH|ctrlX,Y,WxH|viewportX,Y,WxH"
        var hX = HostBorder.X;       var hY = HostBorder.Y;
        var hW = HostBorder.Width;   var hH = HostBorder.Height;
        var cX = ScenarioContent.X;  var cY = ScenarioContent.Y;
        var cW = ScenarioContent.Width; var cH = ScenarioContent.Height;
        var vX = HostContainer.X;    var vY = HostContainer.Y;
        var vW = HostContainer.Width;var vH = HostContainer.Height;
        AppliedLabel.Text =
            $"{_lastSpec}|{hX:0.##},{hY:0.##},{hW:0.##}x{hH:0.##}" +
            $"|{cX:0.##},{cY:0.##},{cW:0.##}x{cH:0.##}" +
            $"|{vX:0.##},{vY:0.##},{vW:0.##}x{vH:0.##}";

        // Deep diagnostics: enumerate every layer's bounds + each SKCanvasView's
        // absolute position. Useful for tracking where natural-size shrinkwrap
        // breaks down (e.g. slider bleeding past parent bounds).
        try
        {
            var sb = new System.Text.StringBuilder();
            void Add(string name, View v) =>
                sb.Append($"{name}=[{v.X:0.#},{v.Y:0.#} {v.Width:0.#}x{v.Height:0.#} ds={v.DesiredSize.Width:0.#}x{v.DesiredSize.Height:0.#}] ");
            Add("HC", HostContainer);
            Add("HB", HostBorder);
            Add("PO", PickerOutline);
            Add("SC", ScenarioContent);
            if (ScenarioContent.Content is View inner)
            {
                Add("CTRL", inner);
                if (inner is Microsoft.Maui.Controls.Layout layout)
                {
                    int i = 0;
                    foreach (var ch in layout.Children)
                        if (ch is View vch) Add($"CH{i++}({vch.GetType().Name})", vch);
                }
            }
            // SKCanvasView absolute positions as the capture computes them
            sb.Append("|SKCV: ");
            void Walk(IView v, double x, double y)
            {
                if (v is SkiaSharp.Views.Maui.Controls.SKCanvasView cv)
                    sb.Append($"{cv.GetType().Name}@({x:0.#},{y:0.#}) cs={cv.CanvasSize.Width:0.#}x{cv.CanvasSize.Height:0.#} fr={cv.Width:0.#}x{cv.Height:0.#} ");
                if (v is Microsoft.Maui.ILayout l)
                    foreach (var c in l) if (c is View vc) Walk(vc, x + vc.X, y + vc.Y);
                if (v is Microsoft.Maui.IContentView cv2 && cv2.PresentedContent is View pv) Walk(pv, x + pv.X, y + pv.Y);
            }
            Walk(HostContainer, 0, 0);
            DebugTraceLabel.Text = sb.ToString();
        }
        catch (Exception ex) { DebugTraceLabel.Text = "diag err: " + ex.Message; }
    }

    /// <summary>
    /// Scenario format: "<control>:<width>x<height>[:opt1,opt2,...]"
    ///   control = wheel | triangle | hsl | rgb
    ///   opts (wheel only): alpha, lumslider, lumwheel, vertical
    /// Examples:
    ///   "wheel:300x300"
    ///   "wheel:400x400:alpha,vertical"
    ///   "triangle:600x300"
    /// </summary>
    /// <summary>How a single dimension is sized.</summary>
    enum SizeMode { Fixed, Auto, Fill }

    void ApplyScenario(string spec)
    {
        var trace = new System.Text.StringBuilder();
        void T(string s) { trace.Append(s); trace.Append(" | "); DebugTraceLabel.Text = trace.ToString(); }
        try
        {
            T($"RECV:{spec}");
            T($"PRE:entry='{ScenarioEntry.Text}' hb.WR={HostBorder.WidthRequest:0} hb.HR={HostBorder.HeightRequest:0} hb.HO={HostBorder.HorizontalOptions.Alignment} hb.VO={HostBorder.VerticalOptions.Alignment} hb.W={HostBorder.Width:0} hb.H={HostBorder.Height:0}");

            var (control, wMode, wValue, hMode, hValue, opts) = Parse(spec);
            T($"PARSED:{control} {wMode}={wValue} x {hMode}={hValue} opts=[{string.Join(",", opts)}]");

            // Update _lastSpec FIRST so SizeChanged events triggered by the
            // WidthRequest/HeightRequest assignments below report the new spec.
            _lastSpec = spec;

            ApplyHostSizing(wMode, wValue, hMode, hValue);
            T($"SIZED:hb.WR={HostBorder.WidthRequest:0} hb.HR={HostBorder.HeightRequest:0} hb.HO={HostBorder.HorizontalOptions.Alignment} hb.VO={HostBorder.VerticalOptions.Alignment} po.HO={PickerOutline.HorizontalOptions.Alignment} po.VO={PickerOutline.VerticalOptions.Alignment}");

            // Per-scenario host bg (the canvas behind the picker). Applied
            // unconditionally so the runtime-toggle path picks up bg= changes.
            HostBorder.BackgroundColor = ParseColorOpt(opts, "bg") ?? Colors.White;

            // Runtime-toggle path: if neither the control type nor the host
            // spec changed, mutate the *existing* instance's feature flags
            // instead of replacing the view. This is what exercises the
            // runtime-invalidation code path (the bug class that broke when
            // ShowAlpha was toggled on a live wheel).
            //
            // Exception: changing slider-visibility flags via runtime mutation
            // hits a MAUI handler-lifecycle race (newly-added slider Children
            // sometimes render at zero size). For wheel scenarios where the
            // alpha/lumslider/vertical/lumwheel flags differ from the last
            // applied set, force a full rebuild. The runtime-toggle path is
            // still exercised by RuntimeInvarianceTests via single-flag toggles
            // and by tests that re-apply the same scenario.
            var sizeKey    = $"{wMode}:{wValue}x{hMode}:{hValue}";
            var featureKey = MakeFeatureKey(control, opts);
            if (control == _lastControl && sizeKey == _lastSizeKey
                && featureKey == _lastFeatureKey
                && ScenarioContent.Content is View existing
                && TryReconfigure(existing, control, opts))
            {
                T($"RECONFIGURED (same control+size+features)");
                StatusLabel.Text = $"toggled: {spec}";
                UpdateAppliedMarker();
                T($"DONE marker={AppliedLabel.Text}");
                return;
            }

            View child = control switch
            {
                "wheel"    => MakeWheel(opts),
                "triangle" => MakeTriangle(opts),
                "hsl"      => MakeSliders<HSLSliders>(opts),
                "rgb"      => MakeSliders<RGBSliders>(opts),
                _          => throw new ArgumentException($"Unknown control '{control}'"),
            };
            T($"BUILT-CHILD type={child.GetType().Name}");
            ScenarioContent.Content = child;
            T($"CONTENT-SET; hb.W={HostBorder.Width:0} hb.H={HostBorder.Height:0} sc.W={ScenarioContent.Width:0} sc.H={ScenarioContent.Height:0}");
            _lastControl = control; _lastSizeKey = sizeKey; _lastFeatureKey = featureKey;

            StatusLabel.Text  = $"applied: {spec}";
            UpdateAppliedMarker();
            T($"DONE marker={AppliedLabel.Text}");
        }
        catch (Exception ex)
        {
            T($"EXCEPTION:{ex.GetType().Name}:{ex.Message}");
            StatusLabel.Text  = "error: " + ex.Message;
            AppliedLabel.Text = "ERROR:" + ex.Message;
        }
    }

    static bool TryReconfigure(View existing, string control, string[] opts)
    {
        if (control == "wheel" && existing is ColorWheel w)
        {
            // Reset to defaults then apply opts.
            w.ShowAlphaSlider     = false;
            w.ShowLuminositySlider= false;
            w.ShowLuminosityWheel = true;
            w.Vertical            = false;
            w.WheelBackgroundColor = Colors.Transparent;
            foreach (var opt in opts)
            {
                if (IsKvOpt(opt, out _, out _)) continue; // handled separately
                switch (opt.Trim().ToLowerInvariant())
                {
                    case "alpha":     w.ShowAlphaSlider     = true;  break;
                    case "lumslider": w.ShowLuminositySlider= true;  break;
                    case "nolumwheel":w.ShowLuminosityWheel = false; break;
                    case "vertical":  w.Vertical            = true;  break;
                    case "":          break;
                    default:          throw new ArgumentException("Unknown option: " + opt);
                }
            }
            var wbg = ParseColorOpt(opts, "wbg");
            if (wbg is not null) w.WheelBackgroundColor = wbg;
            return true;
        }
        if (control == "triangle" && existing is ColorTriangle t)
        {
            t.WheelBackgroundColor = ParseColorOpt(opts, "wbg") ?? Colors.Transparent;
            t.RotateTriangleByHue  = true;
            foreach (var opt in opts)
            {
                if (IsKvOpt(opt, out _, out _)) continue;
                switch (opt.Trim().ToLowerInvariant())
                {
                    case "norotate": t.RotateTriangleByHue = false; break;
                    case "rotate":   t.RotateTriangleByHue = true;  break;
                    case "":         break;
                    default:         throw new ArgumentException("Unknown option: " + opt);
                }
            }
            return true;
        }
        if ((control == "hsl" || control == "rgb") && existing is SliderPickerWithAlpha s)
        {
            s.Vertical        = false;
            s.ShowAlphaSlider = true;
            foreach (var opt in opts)
            {
                if (IsKvOpt(opt, out _, out _)) continue;
                switch (opt.Trim().ToLowerInvariant())
                {
                    case "vertical": s.Vertical        = true;  break;
                    case "noalpha":  s.ShowAlphaSlider = false; break;
                    case "":         break;
                    default:         throw new ArgumentException("Unknown option: " + opt);
                }
            }
            return true;
        }
        // Other control types have no toggleable flags supported yet — fall
        // through and let the caller rebuild when bare.
        return opts.Length == 0;
    }

    static T MakeSliders<T>(string[] opts) where T : SliderPickerWithAlpha, new()
    {
        var s = new T { AutomationId = "ScenarioControl" };
        foreach (var opt in opts)
        {
            if (IsKvOpt(opt, out _, out _)) continue;
            switch (opt.Trim().ToLowerInvariant())
            {
                case "vertical": s.Vertical        = true;  break;
                case "noalpha":  s.ShowAlphaSlider = false; break;
                case "":         break;
                default:         throw new ArgumentException("Unknown option: " + opt);
            }
        }
        return s;
    }

    static ColorWheel MakeWheel(string[] opts)
    {
        var wheel = new ColorWheel { AutomationId = "ScenarioControl" };
        foreach (var opt in opts)
        {
            if (IsKvOpt(opt, out _, out _)) continue;
            switch (opt.Trim().ToLowerInvariant())
            {
                case "alpha":     wheel.ShowAlphaSlider     = true;  break;
                case "lumslider": wheel.ShowLuminositySlider = true; break;
                case "noLumWheel":
                case "nolumwheel":wheel.ShowLuminosityWheel  = false; break;
                case "vertical":  wheel.Vertical            = true;  break;
                case "":          break;
                default:          throw new ArgumentException("Unknown option: " + opt);
            }
        }
        var wbg = ParseColorOpt(opts, "wbg");
        if (wbg is not null) wheel.WheelBackgroundColor = wbg;
        return wheel;
    }
    static ColorTriangle MakeTriangle(string[] opts)
    {
        var t = new ColorTriangle { AutomationId = "ScenarioControl" };
        foreach (var opt in opts)
        {
            if (IsKvOpt(opt, out _, out _)) continue;
            switch (opt.Trim().ToLowerInvariant())
            {
                case "norotate": t.RotateTriangleByHue = false; break;
                case "rotate":   t.RotateTriangleByHue = true;  break;
                case "":         break;
                default:         throw new ArgumentException("Unknown option: " + opt);
            }
        }
        var wbg = ParseColorOpt(opts, "wbg");
        if (wbg is not null) t.WheelBackgroundColor = wbg;
        return t;
    }

    static bool IsKvOpt(string opt, out string key, out string value)
    {
        var idx = opt.IndexOf('=');
        if (idx > 0)
        {
            key   = opt[..idx].Trim().ToLowerInvariant();
            value = opt[(idx + 1)..].Trim();
            return true;
        }
        key = ""; value = "";
        return false;
    }

    static Color? ParseColorOpt(string[] opts, string key)
    {
        foreach (var opt in opts)
        {
            if (IsKvOpt(opt, out var k, out var v) && k == key)
                return ParseColor(v);
        }
        return null;
    }

    static Color ParseColor(string s)
    {
        s = s.Trim();
        // Accept #RRGGBB, #AARRGGBB, or named colors (subset).
        if (s.StartsWith("#")) return Color.FromArgb(s);
        return s.ToLowerInvariant() switch
        {
            "transparent"  => Colors.Transparent,
            "white"        => Colors.White,
            "black"        => Colors.Black,
            "red"          => Colors.Red,
            "green"        => Colors.Green,
            "blue"         => Colors.Blue,
            "yellow"       => Colors.Yellow,
            "magenta"      => Colors.Magenta,
            "cyan"         => Colors.Cyan,
            "gray" or "grey" => Colors.Gray,
            "lightgray" or "lightgrey" => Colors.LightGray,
            _ => throw new ArgumentException("Unknown color: " + s),
        };
    }

    static (string control, SizeMode wMode, double wValue, SizeMode hMode, double hValue, string[] opts) Parse(string spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
            throw new ArgumentException("empty scenario");

        var parts = spec.Split(':');
        if (parts.Length < 2)
            throw new ArgumentException("missing size");

        var control = parts[0].Trim().ToLowerInvariant();
        var sizeParts = parts[1].Split('x');
        if (sizeParts.Length != 2)
            throw new ArgumentException("bad size: " + parts[1]);

        var (wMode, wVal) = ParseSizeToken(sizeParts[0]);
        var (hMode, hVal) = ParseSizeToken(sizeParts[1]);

        var opts = parts.Length >= 3
            ? parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        return (control, wMode, wVal, hMode, hVal, opts);
    }

    static (SizeMode mode, double value) ParseSizeToken(string token)
    {
        var t = token.Trim().ToLowerInvariant();
        if (t == "auto") return (SizeMode.Auto, 0);
        if (t == "fill") return (SizeMode.Fill, 0);
        var v = double.Parse(t, System.Globalization.CultureInfo.InvariantCulture);
        return (SizeMode.Fixed, v);
    }
}
