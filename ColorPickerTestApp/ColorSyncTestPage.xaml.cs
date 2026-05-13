using System.Globalization;
using ColorPicker.BaseClasses;

namespace ColorPickerTestApp;

public partial class ColorSyncTestPage : ContentPage
{
    public ColorSyncTestPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Pick a color with non-zero hue so all controls show distinct picker positions
        // (red=hue 0 makes RotateTriangleByHue look identical to default triangle).
        Dispatcher.Dispatch(() => SetMaster(Colors.Orange));
    }

    void OnInputApplyClicked(object? sender, EventArgs e) => ApplyHex(InputHexEntry.Text);
    void OnInputHexCompleted(object? sender, EventArgs e) => ApplyHex(InputHexEntry.Text);

    void OnInputPresetChanged(object? sender, EventArgs e)
    {
        var name = InputPresetPicker.SelectedItem as string;
        if (string.IsNullOrEmpty(name)) return;
        Color? c = name switch
        {
            "Red"     => Colors.Red,
            "Green"   => Colors.Green,
            "Blue"    => Colors.Blue,
            "Yellow"  => Colors.Yellow,
            "Cyan"    => Colors.Cyan,
            "Magenta" => Colors.Magenta,
            "White"   => Colors.White,
            "Black"   => Colors.Black,
            "Gray50"  => Color.FromRgba(128, 128, 128, 255),
            "Orange"  => Colors.Orange,
            "Purple"  => Colors.Purple,
            "Teal"    => Colors.Teal,
            _         => null,
        };
        if (c is not null) SetMaster(c);
    }

    void ApplyHex(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var c = ParseHex(text.Trim());
        if (c is not null) SetMaster(c);
    }

    void SetMaster(Color c)
    {
        InputSwatch.Color = c;
        foreach (var picker in EnumerateColorPickers(ControlsGrid))
            picker.SelectedColor = c;
        MasterWheel.SelectedColor = c;
    }

    static IEnumerable<ColorPickerViewBase> EnumerateColorPickers(Element root)
    {
        if (root is ColorPickerViewBase cp) yield return cp;
        if (root is IVisualTreeElement vte)
            foreach (var child in vte.GetVisualChildren())
                if (child is Element e)
                    foreach (var nested in EnumerateColorPickers(e))
                        yield return nested;
    }

    static Color? ParseHex(string s)
    {
        if (s.StartsWith("#")) s = s[1..];
        if (s.Length is not (6 or 8)) return null;
        if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v)) return null;
        byte r, g, b, a = 255;
        if (s.Length == 8)
        {
            // Convention: #RRGGBBAA
            r = (byte)((v >> 24) & 0xFF);
            g = (byte)((v >> 16) & 0xFF);
            b = (byte)((v >>  8) & 0xFF);
            a = (byte)(v         & 0xFF);
        }
        else
        {
            r = (byte)((v >> 16) & 0xFF);
            g = (byte)((v >>  8) & 0xFF);
            b = (byte)(v         & 0xFF);
        }
        return Color.FromRgba(r, g, b, a);
    }
}
