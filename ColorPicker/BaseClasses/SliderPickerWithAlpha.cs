namespace ColorPicker.BaseClasses;

public abstract class SliderPickerWithAlpha : SliderPicker
{
    public static readonly BindableProperty ShowAlphaSliderProperty 
                         = BindableProperty.Create( nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(SliderPickerWithAlpha),
                                                    true,
                                                    propertyChanged: HandleShowLuminositySet );
    public bool ShowAlphaSlider
    {
        get => (bool)GetValue( ShowAlphaSliderProperty );
        set => SetValue( ShowAlphaSliderProperty, value );
    }

    static void HandleShowLuminositySet( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            ( (SliderPickerWithAlpha)bindable ).UpdateSliders();
        }
    }
}
