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

    void OnHostSizeChanged(object sender, EventArgs e) => UpdateAppliedMarker();

    string _lastSpec = "";
    string _lastControl = "";
    string _lastSizeKey = "";

    void ApplyHostSizing(SizeMode wMode, double wValue, SizeMode hMode, double hValue)
    {
        // Width
        switch (wMode)
        {
            case SizeMode.Fixed:
                HostBorder.WidthRequest    = wValue;
                HostBorder.HorizontalOptions = LayoutOptions.Start;
                break;
            case SizeMode.Auto:
                HostBorder.WidthRequest    = -1;        // -1 == unset
                HostBorder.HorizontalOptions = LayoutOptions.Start;
                break;
            case SizeMode.Fill:
                HostBorder.WidthRequest    = -1;
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

    void UpdateAppliedMarker()
    {
        // Format: "spec|hostX,Y,WxH|ctrlX,Y,WxH|viewportX,Y,WxH"
        // Coordinates are MAUI logical units (the test fixture knows DPI scale).
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
        try
        {
            var (control, wMode, wValue, hMode, hValue, opts) = Parse(spec);

            // Update _lastSpec FIRST so SizeChanged events triggered by the
            // WidthRequest/HeightRequest assignments below report the new spec.
            _lastSpec = spec;

            ApplyHostSizing(wMode, wValue, hMode, hValue);

            // Runtime-toggle path: if neither the control type nor the host
            // spec changed, mutate the *existing* instance's feature flags
            // instead of replacing the view. This is what exercises the
            // runtime-invalidation code path (the bug class that broke when
            // ShowAlpha was toggled on a live wheel).
            var sizeKey = $"{wMode}:{wValue}x{hMode}:{hValue}";
            if (control == _lastControl && sizeKey == _lastSizeKey
                && ScenarioContent.Content is View existing
                && TryReconfigure(existing, control, opts))
            {
                StatusLabel.Text = $"toggled: {spec}";
                UpdateAppliedMarker();
                return;
            }

            View child = control switch
            {
                "wheel"    => MakeWheel(opts),
                "triangle" => new ColorTriangle    { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                "hsl"      => new HSLSliders       { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                "rgb"      => new RGBSliders       { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                _          => throw new ArgumentException($"Unknown control '{control}'"),
            };
            ScenarioContent.Content = child;
            _lastControl = control; _lastSizeKey = sizeKey;

            StatusLabel.Text  = $"applied: {spec}";
            UpdateAppliedMarker();
        }
        catch (Exception ex)
        {
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
            foreach (var opt in opts)
            {
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
            return true;
        }
        // Other control types have no toggleable flags supported yet — fall
        // through and let the caller rebuild.
        return opts.Length == 0 && (control == "triangle" || control == "hsl" || control == "rgb");
    }

    static ColorWheel MakeWheel(string[] opts)
    {
        var wheel = new ColorWheel
        {
            AutomationId      = "ScenarioControl",
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions   = LayoutOptions.Fill,
        };
        foreach (var opt in opts)
        {
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
        return wheel;
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
