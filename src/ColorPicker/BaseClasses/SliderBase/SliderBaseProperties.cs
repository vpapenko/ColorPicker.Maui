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
                                                    typeof(SliderBase),
                                                    Orientation.Horizontal,
                                                    propertyChanged: OnOrientationPropertyChanged );

    protected virtual void OnOrientationChanged( Orientation newOrientation ) { }

    static void OnOrientationPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue is not null && bindable is SliderBase sliderBase )
            sliderBase.OnOrientationChanged( (Orientation)newValue );
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue( OrientationProperty );
        set => SetValue( OrientationProperty, value );
    }
    #endregion
}
