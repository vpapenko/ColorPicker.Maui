namespace ColorPicker;

public partial class SliderBase
{
    #region Orientation implementation
    /// <summary>
    /// Orientation bindable property
    /// </summary>
    public static readonly BindableProperty OrientationProperty
            = BindableProperty.Create( nameof(Orientation),
                                       typeof(Orientation),
                                       typeof(PickerBase),
                                       Orientation.Horizontal,
                                propertyChanged: (bindable, oldValue, newValue) =>
                                {
                                    if ( newValue is not null && bindable is SliderBase sliderBase )
                                        sliderBase.OnOrientationChanged( (Orientation)newValue );
                                } );

    protected virtual void OnOrientationChanged( Orientation newOrientation ) { }

    public Orientation Orientation
    {
        get => (Orientation)GetValue( OrientationProperty );
        set => SetValue( OrientationProperty, value );
    }
    #endregion
}
