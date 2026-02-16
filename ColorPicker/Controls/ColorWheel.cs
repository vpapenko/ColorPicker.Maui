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
            System.Diagnostics.Debug.WriteLine( $"[ColorWheel] HandleVertical old={oldValue} new={newValue} children={wheel.Children.Count}" );
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

        System.Diagnostics.Debug.WriteLine( $"[ColorWheel] MeasureOverride Vertical={Vertical} sliders={sliderCount} w={widthConstraint} h={heightConstraint} circleSize={circleSize} -> totalW={totalWidth} totalH={totalHeight}" );
        return new Size( totalWidth, totalHeight );
    }

    protected override Size ArrangeOverride( Rect bounds )
    {
        return base.ArrangeOverride( bounds );
    }

    protected override Size ArrangeLayoutChildren( Rect bounds )
    {
        System.Diagnostics.Debug.WriteLine( $"[ColorWheel] ArrangeLayoutChildren bounds={bounds}" );

        var width = bounds.Width;
        var height = bounds.Height;

        var sliderCount = ( ShowLuminositySlider ? 1 : 0 ) + ( ShowAlphaSlider ? 1 : 0 );
        var sliderFraction = 0.1 * sliderCount;

        double circleSize;
        double sliderThickness;

        if ( Vertical )
        {
            circleSize      = sliderCount > 0 ? width * ( 1.0 - sliderFraction ) : width;
            circleSize      = Math.Min( circleSize, height );
            sliderThickness = sliderCount > 0 ? ( width - circleSize ) / sliderCount : 0;
        }
        else
        {
            circleSize      = sliderCount > 0 ? height * ( 1.0 - sliderFraction ) : height;
            circleSize      = Math.Min( circleSize, width );
            sliderThickness = sliderCount > 0 ? ( height - circleSize ) / sliderCount : 0;
        }

        // Measure children with their actual sizes so SkiaSharp renders correctly
        ( (IView)_colorCircle ).Measure( circleSize, circleSize );

        // Arrange the circle
        ( (IView)_colorCircle ).Arrange( new Rect( 0, 0, circleSize, circleSize ) );

        var offset = circleSize;

        if ( ShowLuminositySlider )
        {
            if ( Vertical )
            {
                ( (IView)_luminositySlider ).Measure( sliderThickness, circleSize );
                ( (IView)_luminositySlider ).Arrange( new Rect( offset, 0, sliderThickness, circleSize ) );
            }
            else
            {
                ( (IView)_luminositySlider ).Measure( circleSize, sliderThickness );
                ( (IView)_luminositySlider ).Arrange( new Rect( 0, offset, circleSize, sliderThickness ) );
            }

            offset += sliderThickness;
        }

        if ( ShowAlphaSlider )
        {
            if ( Vertical )
            {
                ( (IView)_alphaSlider ).Measure( sliderThickness, circleSize );
                ( (IView)_alphaSlider ).Arrange( new Rect( offset, 0, sliderThickness, circleSize ) );
            }
            else
            {
                ( (IView)_alphaSlider ).Measure( circleSize, sliderThickness );
                ( (IView)_alphaSlider ).Arrange( new Rect( 0, offset, circleSize, sliderThickness ) );
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
