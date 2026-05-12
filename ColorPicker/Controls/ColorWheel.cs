namespace ColorPicker.Controls;

public class ColorWheel : ColorPickerViewBase
{
    readonly ColorCircle        _colorCircle        = new();
    readonly AlphaSlider        _alphaSlider        = new();
    readonly LuminositySlider   _luminositySlider   = new();

    protected const double LuminositySliderRowHeight    = 12;
    protected const double AlphaSliderRowHeight         = 12;

    public static readonly BindableProperty ShowLuminosityWheelProperty 
                         = BindableProperty.Create( nameof(ShowLuminosityWheel),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    true,
                                                    propertyChanged: HandleShowLuminosity );

    public static readonly BindableProperty ShowLuminositySliderProperty 
                         = BindableProperty.Create( nameof(ShowLuminositySlider),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleShowLuminositySlider );

    public static readonly BindableProperty ShowAlphaSliderProperty 
                         = BindableProperty.Create( nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleShowAlphaSlider );

    public static readonly BindableProperty WheelBackgroundColorProperty 
                         = BindableProperty.Create( nameof(WheelBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorWheel),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleWheelBackgroundColor );

    public static readonly BindableProperty PickerRadiusScaleProperty 
                         = BindableProperty.Create( nameof(PickerRadiusScale),
                                                    typeof(float),
                                                    typeof(ColorWheel),
                                                    0.05F,
                                                    propertyChanged: HandlePickerRadiusScale );

    public static readonly BindableProperty VerticalProperty 
                         = BindableProperty.Create( nameof(Vertical),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleVertical );

    public bool ShowLuminosityWheel
    {
        get => (bool)GetValue( ShowLuminosityWheelProperty );
        set => SetValue( ShowLuminosityWheelProperty, value );
    }
    static void HandleShowLuminosity( BindableObject bindable, object oldValue, object newValue )
            => ( (ColorWheel)bindable )._colorCircle.ShowLuminosityWheel = (bool)newValue;


    public bool ShowLuminositySlider
    {
        get => (bool)GetValue( ShowLuminositySliderProperty );
        set => SetValue( ShowLuminositySliderProperty, value );
    }
    static void HandleShowLuminositySlider( BindableObject bindable, object oldValue, object newValue )
            => ( (ColorWheel)bindable ).UpdateLuminositySlider( (bool)newValue );


    public bool ShowAlphaSlider
    {
        get => (bool)GetValue( ShowAlphaSliderProperty );
        set => SetValue( ShowAlphaSliderProperty, value );
    }
    static void HandleShowAlphaSlider( BindableObject bindable, object oldValue, object newValue )
            => ( (ColorWheel)bindable ).UpdateAlphaSlider( (bool)newValue );


    public Color WheelBackgroundColor
    {
        get => (Color)GetValue( WheelBackgroundColorProperty );
        set => SetValue( WheelBackgroundColorProperty, value );
    }
    static void HandleWheelBackgroundColor( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
            ( (ColorWheel)bindable )._colorCircle.WheelBackgroundColor = (Color)newValue;
    }

    public float PickerRadiusScale
    {
        get => (float)GetValue( PickerRadiusScaleProperty );
        set => SetValue( PickerRadiusScaleProperty, value );
    }
    static void HandlePickerRadiusScale( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            ( (ColorWheel)bindable )._colorCircle.PickerRadiusScale = (float)newValue;
            ( (ColorWheel)bindable )._alphaSlider.PickerRadiusScale = (float)newValue;
            ( (ColorWheel)bindable )._luminositySlider.PickerRadiusScale = (float)newValue;
        }
    }

    public bool Vertical
    {
        get => (bool)GetValue( VerticalProperty );
        set => SetValue( VerticalProperty, value );
    }
    static void HandleVertical( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            var wheel = (ColorWheel)bindable;
            wheel._alphaSlider.Vertical = (bool)newValue;
            wheel._luminositySlider.Vertical = (bool)newValue;
            wheel.InvalidateMeasure();
        }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    public ColorWheel()
    {
        _colorCircle.AttachedColorPicker    = this;

        HorizontalOptions                   = LayoutOptions.Center;
        VerticalOptions                     = LayoutOptions.Center;

        Children.Add( _colorCircle );

        _alphaSlider.AttachedColorPicker       = this;
        _luminositySlider.AttachedColorPicker  = this;

        UpdateAlphaSlider( ShowAlphaSlider );
        UpdateLuminositySlider( ShowLuminositySlider );
    }

    protected override void OnSelectedColorChanging( Color color ) { }

    protected override Size MeasureOverride( double widthConstraint, double heightConstraint )
    {
        // Apply WidthRequest/HeightRequest as constraints
        if ( WidthRequest >= 0 )
            widthConstraint = Math.Min( widthConstraint, WidthRequest );
        if ( HeightRequest >= 0 )
            heightConstraint = Math.Min( heightConstraint, HeightRequest );

        if ( double.IsPositiveInfinity( widthConstraint ) &&
             double.IsPositiveInfinity( heightConstraint ) )
        {
            widthConstraint     = 200;
            heightConstraint    = 200;
        }

        var sliderCount = ( ShowAlphaSlider ? 1 : 0 ) + ( ShowLuminositySlider ? 1 : 0 );
        var sliderFraction = 0.1 * sliderCount;

        double circleSize;
        double totalWidth;
        double totalHeight;

        if ( Vertical )
        {
            // Circle fills height, sliders add to the right
            circleSize  = Math.Min( heightConstraint, ( 1.0 - sliderFraction ) * widthConstraint );
            totalHeight = circleSize;
            totalWidth  = circleSize / ( 1.0 - sliderFraction );
        }
        else
        {
            // Circle fills width, sliders add below
            circleSize  = Math.Min( widthConstraint, ( 1.0 - sliderFraction ) * heightConstraint );
            totalWidth  = circleSize;
            totalHeight = circleSize / ( 1.0 - sliderFraction );
        }

        return new Size( totalWidth, totalHeight );
    }

    protected override Size ArrangeLayoutChildren( Rect bounds )
    {
        var width  = bounds.Width;
        var height = bounds.Height;

        var sliderCount    = ( ShowLuminositySlider ? 1 : 0 ) + ( ShowAlphaSlider ? 1 : 0 );
        var sliderFraction = 0.1 * sliderCount;

        // Pick the largest circle that fits both the perpendicular axis and the
        // (1 - sliderFraction) share of the slider axis. Slider thickness then
        // stays at its natural proportion (10% of the circle per slider) rather
        // than stretching to fill leftover host space.
        double circleSize;
        double sliderThickness;

        if ( Vertical )
        {
            circleSize      = Math.Min( height,
                                        sliderCount > 0 ? ( 1.0 - sliderFraction ) * width : width );
            sliderThickness = sliderCount > 0
                            ? ( sliderFraction * circleSize ) / ( ( 1.0 - sliderFraction ) * sliderCount )
                            : 0;
        }
        else
        {
            circleSize      = Math.Min( width,
                                        sliderCount > 0 ? ( 1.0 - sliderFraction ) * height : height );
            sliderThickness = sliderCount > 0
                            ? ( sliderFraction * circleSize ) / ( ( 1.0 - sliderFraction ) * sliderCount )
                            : 0;
        }

        // Bounds == natural size (parent honors our DesiredSize via HO/VO=Center),
        // so children are placed at (0,0) with no centering offset needed.
        ( (IView)_colorCircle ).Measure( circleSize, circleSize );
        ( (IView)_colorCircle ).Arrange( new Rect( 0, 0, circleSize, circleSize ) );

        if ( Vertical )
        {
            var x = circleSize;
            if ( ShowLuminositySlider )
            {
                ( (IView)_luminositySlider ).Measure( sliderThickness, circleSize );
                ( (IView)_luminositySlider ).Arrange( new Rect( x, 0, sliderThickness, circleSize ) );
                x += sliderThickness;
            }
            if ( ShowAlphaSlider )
            {
                ( (IView)_alphaSlider ).Measure( sliderThickness, circleSize );
                ( (IView)_alphaSlider ).Arrange( new Rect( x, 0, sliderThickness, circleSize ) );
            }
        }
        else
        {
            var y = circleSize;
            if ( ShowLuminositySlider )
            {
                ( (IView)_luminositySlider ).Measure( circleSize, sliderThickness );
                ( (IView)_luminositySlider ).Arrange( new Rect( 0, y, circleSize, sliderThickness ) );
                y += sliderThickness;
            }
            if ( ShowAlphaSlider )
            {
                ( (IView)_alphaSlider ).Measure( circleSize, sliderThickness );
                ( (IView)_alphaSlider ).Arrange( new Rect( 0, y, circleSize, sliderThickness ) );
            }
        }

        return bounds.Size;
    }

    void BoundColorPicker_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
        if ( e.PropertyName == nameof( SelectedColor ) )
            SelectedColor = ( (IColorPicker)sender ).SelectedColor;
    }

    void UpdateAlphaSlider( bool show )
    {
        if ( show )
            Children.Add( _alphaSlider );
        else
            Children.Remove( _alphaSlider );
    }

    void UpdateLuminositySlider( bool show )
    {
        if ( show )
            Children.Add( _luminositySlider );
        else
            Children.Remove( _luminositySlider );
    }
}
