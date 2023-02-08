namespace ColorPicker;

public partial class ColorCircle
{ 
    #region Radius implementation
    /// <summary>
    /// ReticalRadius bindable property
    /// </summary>
    public static readonly BindableProperty RadiusProperty
                         = BindableProperty.Create( nameof(Radius),
                                                    typeof(double),
                                                    typeof(ColorCircle),
                                                    500.0,
                                                    propertyChanged: OnRadiusPropertyChanged );

    static void OnRadiusPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue is not null && bindable is ColorCircle colorCircle )
        {
            colorCircle.HeightRequest = colorCircle.WidthRequest = (double)newValue * 2.0;
        }
    }

    public double Radius
    {
        get => (double)GetValue( RadiusProperty );
        set => SetValue( RadiusProperty, value );
    }
    #endregion 
}
