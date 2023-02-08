namespace ColorPicker;

public partial class ColorWheel
{
    #region ShowLuminosityWheel implementation
    /// <summary>
    /// ShowLuminosity bindable property
    /// </summary>
    public static readonly BindableProperty ShowLuminosityWheelProperty
                         = BindableProperty.Create( nameof(ShowLuminosityWheel),
                                                    typeof(bool),
                                                    typeof(IColorPicker),
                                                    false,
                                                    propertyChanged: OnShowLuminosityWheelPropertyChanged );

    static void OnShowLuminosityWheelPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue is not null && bindable is ColorPickerBase pickerBase )
            pickerBase.Invalidate();
    }

    public bool ShowLuminosityWheel
    {
        get => (bool)GetValue( ShowLuminosityWheelProperty );
        set => SetValue( ShowLuminosityWheelProperty, value );
    }
    #endregion

    #region WheelBackgroundColor implementation
    /// <summary>
    /// WheelBackgroundColor bindable property
    /// </summary>
    public static readonly BindableProperty WheelBackgroundColorProperty
                         = BindableProperty.Create( nameof(WheelBackgroundColor),
                                                    typeof(Color),
                                                    typeof(IColorPicker),
                                                    Colors.Transparent,
                                                    propertyChanged: OnWheelBackgroundColorPropertyChanged );

    static void OnWheelBackgroundColorPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( oldValue != newValue && bindable is ColorWheel wheel )
            wheel.Invalidate();
    }

    public Color WheelBackgroundColor
    {
        get => (Color)GetValue( WheelBackgroundColorProperty );
        set => SetValue( WheelBackgroundColorProperty, value );
    }
    #endregion
}
