namespace ColorPicker.Controls;

public class ColorTriangle : ColorPickerBase
{
    readonly ColorTriangleArea  _area           = new();
    readonly AlphaSlider        _alphaSlider    = new();

    public static readonly BindableProperty ShowAlphaSliderProperty
                         = BindableProperty.Create( nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(ColorTriangle),
                                                    false,
                                                    propertyChanged: HandleShowAlphaSlider );

    public static readonly BindableProperty VerticalProperty
                         = BindableProperty.Create( nameof(Vertical),
                                                    typeof(bool),
                                                    typeof(ColorTriangle),
                                                    false,
                                                    propertyChanged: HandleVertical );

    public static readonly BindableProperty RotateTriangleByHueProperty
                         = BindableProperty.Create( nameof(RotateTriangleByHue),
                                                    typeof(bool),
                                                    typeof(ColorTriangle),
                                                    true,
                                                    propertyChanged: HandleRotateTriangleByHue );

    public static readonly BindableProperty CanvasBackgroundColorProperty
                         = BindableProperty.Create( nameof(CanvasBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorTriangle),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleCanvasBackgroundColor );

    public static readonly BindableProperty IndicatorRadiusScaleProperty
                         = BindableProperty.Create( nameof(IndicatorRadiusScale),
                                                    typeof(float),
                                                    typeof(ColorTriangle),
                                                    0.035F,
                                                    propertyChanged: HandleIndicatorRadiusScale );

    public bool ShowAlphaSlider
    {
        get => (bool)GetValue( ShowAlphaSliderProperty );
        set => SetValue( ShowAlphaSliderProperty, value );
    }
    static void HandleShowAlphaSlider( BindableObject bindable, object oldValue, object newValue )
            => ( (ColorTriangle)bindable ).UpdateAlphaSlider( (bool)newValue );

    public bool Vertical
    {
        get => (bool)GetValue( VerticalProperty );
        set => SetValue( VerticalProperty, value );
    }
    static void HandleVertical( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            var triangle = (ColorTriangle)bindable;
            triangle._alphaSlider.Vertical = (bool)newValue;
            triangle.InvalidateMeasure();
        }
    }

    public bool RotateTriangleByHue
    {
        get => (bool)GetValue( RotateTriangleByHueProperty );
        set => SetValue( RotateTriangleByHueProperty, value );
    }
    static void HandleRotateTriangleByHue( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
            ( (ColorTriangle)bindable )._area.RotateTriangleByHue = (bool)newValue;
    }

    public Color CanvasBackgroundColor
    {
        get => (Color)GetValue( CanvasBackgroundColorProperty );
        set => SetValue( CanvasBackgroundColorProperty, value );
    }
    static void HandleCanvasBackgroundColor( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
            ( (ColorTriangle)bindable )._area.CanvasBackgroundColor = (Color)newValue;
    }

    public float IndicatorRadiusScale
    {
        get => (float)GetValue( IndicatorRadiusScaleProperty );
        set => SetValue( IndicatorRadiusScaleProperty, value );
    }
    static void HandleIndicatorRadiusScale( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            // Only propagate to the inner triangle area. The embedded alpha
            // slider uses SliderStack's auto-fill behavior (its own
            // IndicatorRadiusScale = 0): its picker radius is derived from
            // the slim strip we allot it in ArrangeLayoutChildren, mirroring
            // the pattern used by ColorWheel.
            ( (ColorTriangle)bindable )._area.IndicatorRadiusScale = (float)newValue;
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ColorTriangle()
    {
        _area.AttachedColorPicker = this;

        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions   = LayoutOptions.Center;

        Children.Add( _area );

        _alphaSlider.AttachedColorPicker = this;

        UpdateAlphaSlider( ShowAlphaSlider );
    }

    protected override void OnSelectedColorChanging( Color color ) { }

    protected override Size MeasureOverride( double widthConstraint, double heightConstraint )
    {
        if ( WidthRequest >= 0 )
            widthConstraint = Math.Min( widthConstraint, WidthRequest );
        if ( HeightRequest >= 0 )
            heightConstraint = Math.Min( heightConstraint, HeightRequest );

        if ( double.IsPositiveInfinity( widthConstraint ) &&
             double.IsPositiveInfinity( heightConstraint ) )
        {
            widthConstraint  = 200;
            heightConstraint = 200;
        }

        var sliderCount    = ShowAlphaSlider ? 1 : 0;
        var sliderFraction = 0.1 * sliderCount;

        double triangleSize;
        double totalWidth;
        double totalHeight;

        if ( Vertical )
        {
            triangleSize = Math.Min( heightConstraint, ( 1.0 - sliderFraction ) * widthConstraint );
            totalHeight  = triangleSize;
            totalWidth   = sliderCount > 0 ? triangleSize / ( 1.0 - sliderFraction ) : triangleSize;
        }
        else
        {
            triangleSize = Math.Min( widthConstraint, ( 1.0 - sliderFraction ) * heightConstraint );
            totalWidth   = triangleSize;
            totalHeight  = sliderCount > 0 ? triangleSize / ( 1.0 - sliderFraction ) : triangleSize;
        }

        return new Size( totalWidth, totalHeight );
    }

    protected override Size ArrangeLayoutChildren( Rect bounds )
    {
        var width  = bounds.Width;
        var height = bounds.Height;

        var sliderCount    = ShowAlphaSlider ? 1 : 0;
        var sliderFraction = 0.1 * sliderCount;

        double triangleSize;
        double sliderThickness;

        if ( Vertical )
        {
            triangleSize    = Math.Min( height,
                                        sliderCount > 0 ? ( 1.0 - sliderFraction ) * width : width );
            sliderThickness = sliderCount > 0
                            ? ( sliderFraction * triangleSize ) / ( ( 1.0 - sliderFraction ) * sliderCount )
                            : 0;
        }
        else
        {
            triangleSize    = Math.Min( width,
                                        sliderCount > 0 ? ( 1.0 - sliderFraction ) * height : height );
            sliderThickness = sliderCount > 0
                            ? ( sliderFraction * triangleSize ) / ( ( 1.0 - sliderFraction ) * sliderCount )
                            : 0;
        }

        ( (IView)_area ).Measure( triangleSize, triangleSize );
        ( (IView)_area ).Arrange( new Rect( 0, 0, triangleSize, triangleSize ) );

        if ( ShowAlphaSlider )
        {
            if ( Vertical )
            {
                ( (IView)_alphaSlider ).Measure( sliderThickness, triangleSize );
                ( (IView)_alphaSlider ).Arrange( new Rect( triangleSize, 0, sliderThickness, triangleSize ) );
            }
            else
            {
                ( (IView)_alphaSlider ).Measure( triangleSize, sliderThickness );
                ( (IView)_alphaSlider ).Arrange( new Rect( 0, triangleSize, triangleSize, sliderThickness ) );
            }
        }

        return bounds.Size;
    }

    void UpdateAlphaSlider( bool show )
    {
        if ( show )
        {
            if ( !Children.Contains( _alphaSlider ) )
                Children.Add( _alphaSlider );
        }
        else
        {
            Children.Remove( _alphaSlider );
        }
    }
}
