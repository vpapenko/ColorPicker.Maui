namespace ColorPicker.BaseClasses;

public abstract class SliderStackWithAlpha : SliderStack
{
    public static readonly BindableProperty ShowAlphaSliderProperty
                         = BindableProperty.Create(nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(SliderStackWithAlpha),
                                                    true,
                                                    propertyChanged: HandleShowLuminositySet);
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
