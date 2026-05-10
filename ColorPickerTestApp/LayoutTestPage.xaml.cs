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

    void UpdateAppliedMarker()
    {
        // Format: "spec|hostX,Y,WxH|ctrlX,Y,WxH"
        // Coordinates are MAUI logical units (the test fixture knows DPI scale).
        var hX = HostBorder.X;       var hY = HostBorder.Y;
        var hW = HostBorder.Width;   var hH = HostBorder.Height;
        var cX = ScenarioContent.X;  var cY = ScenarioContent.Y;
        var cW = ScenarioContent.Width; var cH = ScenarioContent.Height;
        AppliedLabel.Text =
            $"{_lastSpec}|{hX:0.##},{hY:0.##},{hW:0.##}x{hH:0.##}|{cX:0.##},{cY:0.##},{cW:0.##}x{cH:0.##}";
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
    void ApplyScenario(string spec)
    {
        try
        {
            var (control, w, h, opts) = Parse(spec);

            // Update _lastSpec FIRST so SizeChanged events triggered by the
            // WidthRequest/HeightRequest assignments below report the new spec.
            _lastSpec = spec;

            HostBorder.WidthRequest  = w;
            HostBorder.HeightRequest = h;

            View child = control switch
            {
                "wheel"    => MakeWheel(opts),
                "triangle" => new ColorTriangle    { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                "hsl"      => new HSLSliders       { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                "rgb"      => new RGBSliders       { AutomationId = "ScenarioControl", HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill },
                _          => throw new ArgumentException($"Unknown control '{control}'"),
            };
            ScenarioContent.Content = child;

            StatusLabel.Text  = $"applied: {spec}";
            UpdateAppliedMarker();
        }
        catch (Exception ex)
        {
            StatusLabel.Text  = "error: " + ex.Message;
            AppliedLabel.Text = "ERROR:" + ex.Message;
        }
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

    static (string control, double width, double height, string[] opts) Parse(string spec)
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

        var w = double.Parse(sizeParts[0], System.Globalization.CultureInfo.InvariantCulture);
        var h = double.Parse(sizeParts[1], System.Globalization.CultureInfo.InvariantCulture);

        var opts = parts.Length >= 3
            ? parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        return (control, w, h, opts);
    }
}
