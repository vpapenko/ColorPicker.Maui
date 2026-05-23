namespace ConsumerSmoke;

using ColorPicker.Controls;

public partial class SmokePage : ContentPage
{
    public SmokePage()
    {
        InitializeComponent();

        // Compile-time references to every control type we publish, so the
        // smoke build also fails if a control class disappears from the
        // packed assembly (e.g. accidentally made internal during refactor).
        _ = typeof(ColorWheel);
        _ = typeof(ColorTriangle);
        _ = typeof(HslSlider);
        _ = typeof(RgbSlider);
        _ = typeof(AlphaSlider);
        _ = typeof(LuminositySlider);
        _ = typeof(ColorDisc);
    }
}
