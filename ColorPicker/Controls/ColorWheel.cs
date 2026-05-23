namespace ColorPicker.Controls;

public class ColorWheel : ColorPickerBase
{
    readonly ColorDisc        _disc        = new();
    readonly AlphaSlider        _alphaSlider        = new();
    readonly LuminositySlider   _luminositySlider   = new();

    protected const double LuminositySliderRowHeight    = 12;
    protected const double AlphaSliderRowHeight         = 12;

    public static readonly BindableProperty ShowLuminosityRingProperty
                         = BindableProperty.Create(nameof(ShowLuminosityRing),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    true,
                                                    propertyChanged: HandleShowLuminosity);

    public static readonly BindableProperty ShowLuminositySliderProperty
                         = BindableProperty.Create(nameof(ShowLuminositySlider),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleShowLuminositySlider);

    public static readonly BindableProperty ShowAlphaSliderProperty
                         = BindableProperty.Create(nameof(ShowAlphaSlider),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleShowAlphaSlider);

    public static readonly BindableProperty CanvasBackgroundColorProperty
                         = BindableProperty.Create(nameof(CanvasBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorWheel),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleCanvasBackgroundColor);

    public static readonly BindableProperty IndicatorRadiusScaleProperty
                         = BindableProperty.Create(nameof(IndicatorRadiusScale),
                                                    typeof(float),
                                                    typeof(ColorWheel),
                                                    0.05F,
                                                    propertyChanged: HandlePickerRadiusScale);

    public static readonly BindableProperty VerticalProperty
                         = BindableProperty.Create(nameof(Vertical),
                                                    typeof(bool),
                                                    typeof(ColorWheel),
                                                    false,
                                                    propertyChanged: HandleVertical);

    public bool ShowLuminosityRing
    {
        get => (bool)GetValue(ShowLuminosityRingProperty);
        set => SetValue(ShowLuminosityRingProperty, value);
    }
    static void HandleShowLuminosity(BindableObject bindable, object oldValue, object newValue)
            => ((ColorWheel)bindable)._disc.ShowLuminosityRing = (bool)newValue;


    public bool ShowLuminositySlider
    {
        get => (bool)GetValue(ShowLuminositySliderProperty);
        set => SetValue(ShowLuminositySliderProperty, value);
    }
    static void HandleShowLuminositySlider(BindableObject bindable, object oldValue, object newValue)
            => ((ColorWheel)bindable).UpdateLuminositySlider((bool)newValue);


    public bool ShowAlphaSlider
    {
        get => (bool)GetValue(ShowAlphaSliderProperty);
        set => SetValue(ShowAlphaSliderProperty, value);
    }
    static void HandleShowAlphaSlider(BindableObject bindable, object oldValue, object newValue)
            => ((ColorWheel)bindable).UpdateAlphaSlider((bool)newValue);


    public Color CanvasBackgroundColor
    {
        get => (Color)GetValue(CanvasBackgroundColorProperty);
        set => SetValue(CanvasBackgroundColorProperty, value);
    }
    static void HandleCanvasBackgroundColor(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
            ((ColorWheel)bindable)._disc.CanvasBackgroundColor = (Color)newValue;
    }

    public float IndicatorRadiusScale
    {
        get => (float)GetValue(IndicatorRadiusScaleProperty);
        set => SetValue(IndicatorRadiusScaleProperty, value);
    }
    static void HandlePickerRadiusScale(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            // Only propagate to the disc/ring. The embedded sliders use
            // SliderStack's auto-fill behavior (IndicatorRadiusScale = 0): their
            // picker radius is derived from the slim strip the wheel allots
            // them in ArrangeLayoutChildren, which already produces a picker
            // size visually consistent with the wheel's own picker. Forwarding
            // a non-zero scale here would force the slider into aspect-locked
            // mode and break the wheel's manual layout.
            ((ColorWheel)bindable)._disc.IndicatorRadiusScale = (float)newValue;
        }
    }

    public bool Vertical
    {
        get => (bool)GetValue(VerticalProperty);
        set => SetValue(VerticalProperty, value);
    }
    static void HandleVertical(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
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
        _disc.AttachedColorPicker = this;

        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        Children.Add(_disc);

        _alphaSlider.AttachedColorPicker = this;
        _luminositySlider.AttachedColorPicker = this;

        UpdateAlphaSlider(ShowAlphaSlider);
        UpdateLuminositySlider(ShowLuminositySlider);
    }

    protected override void OnSelectedColorChanging(Color color) { }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // Apply WidthRequest/HeightRequest as constraints
        if (WidthRequest >= 0)
            widthConstraint = Math.Min(widthConstraint, WidthRequest);
        if (HeightRequest >= 0)
            heightConstraint = Math.Min(heightConstraint, HeightRequest);

        if (double.IsPositiveInfinity(widthConstraint) &&
             double.IsPositiveInfinity(heightConstraint))
        {
            widthConstraint = 200;
            heightConstraint = 200;
        }

        var sliderCount = (ShowAlphaSlider ? 1 : 0) + (ShowLuminositySlider ? 1 : 0);
        var sliderFraction = 0.1 * sliderCount;

        double circleSize;
        double totalWidth;
        double totalHeight;

        if (Vertical)
        {
            // Circle fills height, sliders add to the right
            circleSize = Math.Min(heightConstraint, (1.0 - sliderFraction) * widthConstraint);
            totalHeight = circleSize;
            totalWidth = circleSize / (1.0 - sliderFraction);
        }
        else
        {
            // Circle fills width, sliders add below
            circleSize = Math.Min(widthConstraint, (1.0 - sliderFraction) * heightConstraint);
            totalWidth = circleSize;
            totalHeight = circleSize / (1.0 - sliderFraction);
        }

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeLayoutChildren(Rect bounds)
    {
        var width  = bounds.Width;
        var height = bounds.Height;

        var sliderCount    = (ShowLuminositySlider ? 1 : 0) + (ShowAlphaSlider ? 1 : 0);
        var sliderFraction = 0.1 * sliderCount;

        // Pick the largest circle that fits both the perpendicular axis and the
        // (1 - sliderFraction) share of the slider axis. Slider thickness then
        // stays at its natural proportion (10% of the circle per slider) rather
        // than stretching to fill leftover host space.
        double circleSize;
        double sliderThickness;

        if (Vertical)
        {
            circleSize = Math.Min(height,
                                        sliderCount > 0 ? (1.0 - sliderFraction) * width : width);
            sliderThickness = sliderCount > 0
                            ? (sliderFraction * circleSize) / ((1.0 - sliderFraction) * sliderCount)
                            : 0;
        }
        else
        {
            circleSize = Math.Min(width,
                                        sliderCount > 0 ? (1.0 - sliderFraction) * height : height);
            sliderThickness = sliderCount > 0
                            ? (sliderFraction * circleSize) / ((1.0 - sliderFraction) * sliderCount)
                            : 0;
        }

        // Bounds == natural size (parent honors our DesiredSize via HO/VO=Center),
        // so children are placed at (0,0) with no centering offset needed.
        ((IView)_disc).Measure(circleSize, circleSize);
        ((IView)_disc).Arrange(new Rect(0, 0, circleSize, circleSize));

        if (Vertical)
        {
            var x = circleSize;
            if (ShowLuminositySlider)
            {
                ((IView)_luminositySlider).Measure(sliderThickness, circleSize);
                ((IView)_luminositySlider).Arrange(new Rect(x, 0, sliderThickness, circleSize));
                x += sliderThickness;
            }
            if (ShowAlphaSlider)
            {
                ((IView)_alphaSlider).Measure(sliderThickness, circleSize);
                ((IView)_alphaSlider).Arrange(new Rect(x, 0, sliderThickness, circleSize));
            }
        }
        else
        {
            var y = circleSize;
            if (ShowLuminositySlider)
            {
                ((IView)_luminositySlider).Measure(circleSize, sliderThickness);
                ((IView)_luminositySlider).Arrange(new Rect(0, y, circleSize, sliderThickness));
                y += sliderThickness;
            }
            if (ShowAlphaSlider)
            {
                ((IView)_alphaSlider).Measure(circleSize, sliderThickness);
                ((IView)_alphaSlider).Arrange(new Rect(0, y, circleSize, sliderThickness));
            }
        }

        return bounds.Size;
    }

    void BoundColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedColor))
            SelectedColor = ((IColorPicker)sender).SelectedColor;
    }

    void UpdateAlphaSlider(bool show)
    {
        if (show)
            Children.Add(_alphaSlider);
        else
            Children.Remove(_alphaSlider);
    }

    void UpdateLuminositySlider(bool show)
    {
        if (show)
            Children.Add(_luminositySlider);
        else
            Children.Remove(_luminositySlider);
    }
}
