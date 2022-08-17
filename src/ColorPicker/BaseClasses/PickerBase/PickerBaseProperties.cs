namespace ColorPicker;

public partial class PickerBase
{
    #region ShowLuminosityWheel implementation
    /// <summary>
    /// ShowLuminosityWheel bindable property
    /// </summary>
    public static readonly BindableProperty ShowLuminosityProperty
            = BindableProperty.Create( nameof(ShowLuminosity),
                                       typeof(bool),
                                       typeof(IColorPicker),
                                       false,
                                propertyChanged: (bindable, oldValue, newValue) =>
                                {
                                    if ( newValue is not null && bindable is PickerBase pickerBase )
                                        pickerBase.Invalidate();
                                } );

    public bool ShowLuminosity
    {
        get => (bool)GetValue( ShowLuminosityProperty );
        set => SetValue( ShowLuminosityProperty, value );
    }
    #endregion


}
