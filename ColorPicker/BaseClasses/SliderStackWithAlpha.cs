namespace ColorPicker.BaseClasses;

/// <summary>
/// A <see cref="SliderStack"/> that can append an alpha (opacity) slider.
/// </summary>
public abstract class SliderStackWithAlpha : SliderStack
{
    public static readonly BindableProperty ShowAlphaSliderProperty
                         = BindableProperty.Create(nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(SliderStackWithAlpha),
                                                    true,
                                                    propertyChanged: HandleShowLuminositySet);
    /// <summary>Whether to append an alpha (opacity) slider. Default <c>true</c>.</summary>
    public bool ShowAlphaSlider
    {
        get => (bool)GetValue(ShowAlphaSliderProperty);
        set => SetValue(ShowAlphaSliderProperty, value);
    }

    static void HandleShowLuminositySet(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((SliderStackWithAlpha)bindable).UpdateSliders();
        }
    }
}
