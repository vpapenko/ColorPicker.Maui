namespace ColorPicker.BaseClasses;

public abstract class SliderPicker : SkiaSharpPickerBase
{
    readonly List<SliderLocation> _sliders = new();

    //	Constructor
    //
    public SliderPicker() => UpdateSliders();

    public static readonly BindableProperty VerticalProperty 
                         = BindableProperty.Create( nameof(Vertical),
                                                    typeof(bool),
                                                    typeof(SliderPicker),
                                                    false,
                                                    propertyChanged: HandleVerticalSet );
    public bool Vertical
    {
        get => (bool)GetValue( VerticalProperty );
        set => SetValue( VerticalProperty, value );
    }

    static void HandleVerticalSet( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            ( (SliderPicker)bindable ).InvalidateMeasure();
            ( (SliderPicker)bindable ).UpdateSliders();
        }
    }

    public override float GetPickerRadiusPixels( SKSize canvasSize )    => GetSize( canvasSize ) / _sliders.Count / 2.2F;
    public override float GetPickerRadiusPixels()                       => GetPickerRadiusPixels( GetCanvasSize() );

    protected abstract IEnumerable<SliderBase> GetSliders();

    protected void UpdateSliders()
    {
        _sliders.Clear();
        var i = 1;
        foreach ( var slider in GetSliders() )
        {
            var sliderLocation = new SliderLocation(slider)
            {
                OffsetLocationMultiplier = (float)(-1.1 + (i * 2.2))
            };
            _sliders.Add( sliderLocation );
            i++;
        }

        InvalidateSurface();
    }

    protected override void OnPaintSurface( SKCanvas canvas, int width, int height )
    {
        var canvasSize = new SKSize(width, height);
        UpdateLocations( SelectedColor, canvasSize );
        canvas.Clear();

        foreach ( var slider in _sliders )
        {
            PaintSlider( canvas, slider, canvasSize );
            PaintPicker( canvas, slider.Location );
        }
    }

    protected override void OnSelectedColorChanging( Color color ) => InvalidateSurface();

    protected override void OnTouchActionPressed( ColorPickerTouchActionEventArgs args )
    {
        var canvasSize  = GetCanvasSize();
        var point       = ConvertToPixel(args.Location);

        foreach ( var slider in _sliders )
        {
            var slidersOffset = slider.GetSliderOffset(GetPickerRadiusPixels());
            if ( slider.LocationProgressId is null && IsInSliderArea( point, slidersOffset ) )
            {
                slider.LocationProgressId = args.Id;
                slider.Location = LimitToSliderLocation( point, slidersOffset, canvasSize );
                UpdateColors( slider, canvasSize );
            }
        }
    }

    protected override void OnTouchActionMoved( ColorPickerTouchActionEventArgs args )
    {
        var canvasSize  = GetCanvasSize();
        var point       = ConvertToPixel(args.Location);

        foreach ( var slider in _sliders )
        {
            if ( slider.LocationProgressId == args.Id )
            {
                var slidersOffset = slider.GetSliderOffset(GetPickerRadiusPixels());
                slider.Location = LimitToSliderLocation( point, slidersOffset, canvasSize );
                UpdateColors( slider, canvasSize );
            }
        }
    }

    protected override void OnTouchActionReleased( ColorPickerTouchActionEventArgs args )
    {
        var canvasSize  = GetCanvasSize();
        var point       = ConvertToPixel(args.Location);

        foreach ( var slider in _sliders )
        {
            if ( slider.LocationProgressId == args.Id )
            {
                slider.LocationProgressId = null;
                var slidersOffset = slider.GetSliderOffset(GetPickerRadiusPixels());
                slider.Location = LimitToSliderLocation( point, slidersOffset, canvasSize );
                UpdateColors( slider, canvasSize );
            }
        }
    }

    protected override void OnTouchActionCancelled( ColorPickerTouchActionEventArgs args )
    {
        foreach ( var slider in _sliders )
        {
            if ( slider.LocationProgressId == args.Id )
            {
                slider.LocationProgressId = null;
            }
        }
    }

    protected override SizeRequest GetMeasure( double widthConstraint, double heightConstraint )
    {
        if ( double.IsPositiveInfinity( widthConstraint ) &&
             double.IsPositiveInfinity( heightConstraint ) )
        {
            if ( Vertical )
            {
                widthConstraint = 200;
                heightConstraint = 50;
            }
            else
            {
                widthConstraint = 50;
                heightConstraint = 200;
            }
        }

        double height;
        double width;
        if ( Vertical )
        {
            width = double.IsInfinity( widthConstraint ) ? heightConstraint * 0.1 * _sliders.Count : widthConstraint;
            height = double.IsInfinity( heightConstraint ) ? 10 * width / _sliders.Count : heightConstraint;
        }
        else
        {
            height = double.IsInfinity( heightConstraint ) ? widthConstraint * 0.1 * _sliders.Count : heightConstraint;
            width = double.IsInfinity( widthConstraint ) ? 10 * heightConstraint / _sliders.Count : widthConstraint;
        }

        return new SizeRequest( new Size( width, height ) );
    }

    protected override float GetSize() => GetSize( GetCanvasSize() );

    protected override float GetSize( SKSize canvasSize ) => Vertical ? canvasSize.Width : canvasSize.Height;

    void UpdateLocations( Color color, SKSize canvasSize )
    {
        foreach ( var slider in _sliders )
        {
            if ( !IsInSliderArea( slider.Location, slider.GetSliderOffset( GetPickerRadiusPixels() ) ) )
            {
                var left = (GetPickerRadiusPixels() * 2) + (SlidersWidht(canvasSize) * slider.Slider.NewValue(color));
                slider.Location = Vertical
                    ? new SKPoint( slider.GetSliderOffset( GetPickerRadiusPixels() ), left )
                    : new SKPoint( left, slider.GetSliderOffset( GetPickerRadiusPixels() ) );
            }
        }
    }

    float SlidersWidht( SKSize canvasSize ) 
       => Vertical ? canvasSize.Height - ( GetPickerRadiusPixels() * 4 )
                   : canvasSize.Width - ( GetPickerRadiusPixels() * 4 );

    void UpdateColors( SliderLocation slider, SKSize canvasSize )
    {
        var newColor = SelectedColor;
        var newValue = Vertical ? ( slider.Location.Y - ( GetPickerRadiusPixels() * 2 ) ) / SlidersWidht( canvasSize )
                                : ( slider.Location.X - ( GetPickerRadiusPixels() * 2 ) ) / SlidersWidht( canvasSize );

        newColor = slider.Slider.GetNewColor( newValue, newColor );

        SelectedColor = newColor;
        InvalidateSurface();
    }

    void PaintSlider( SKCanvas canvas, SliderLocation slider, SKSize canvasSize )
    {
        var pickerRadiusPixels = GetPickerRadiusPixels();
        var sliderTop = slider.GetSliderOffset(pickerRadiusPixels);

        SKPoint startPoint;
        SKPoint endPoint;

        if ( Vertical )
        {
            startPoint = new SKPoint( sliderTop, pickerRadiusPixels * 2 );
            endPoint = new SKPoint( sliderTop, canvasSize.Height - ( pickerRadiusPixels * 2 ) );
        }
        else
        {
            startPoint = new SKPoint( pickerRadiusPixels * 2, sliderTop );
            endPoint = new SKPoint( canvasSize.Width - ( pickerRadiusPixels * 2 ), sliderTop );
        }

        var paint = slider.Slider.GetPaint(SelectedColor, startPoint, endPoint);
        paint.StrokeWidth = pickerRadiusPixels * 1.3F;

        if ( slider.Slider.PaintChessPattern )
        {
            PaintChessPattern( canvas, slider, canvasSize );
        }

        canvas.DrawLine( startPoint, endPoint, paint );
    }

    void PaintChessPattern( SKCanvas canvas, SliderLocation slider, SKSize canvasSize )
    {
        var pickerRadiusPixels  = GetPickerRadiusPixels();
        var sliderTop           = slider.GetSliderOffset(pickerRadiusPixels);
        var scale               = pickerRadiusPixels / 3;
        var path                = new SKPath();

        path.MoveTo( -1 * scale, -1 * scale );
        path.LineTo(  0 * scale, -1 * scale );
        path.LineTo(  0 * scale,  0 * scale );
        path.LineTo(  1 * scale,  0 * scale );
        path.LineTo(  1 * scale,  1 * scale );
        path.LineTo(  0 * scale,  1 * scale );
        path.LineTo(  0 * scale,  0 * scale );
        path.LineTo( -1 * scale,  0 * scale );
        path.LineTo( -1 * scale, -1 * scale );

        var matrix = SKMatrix.CreateScale( 2 * scale, 2 * scale );
        var paint = new SKPaint
        {
            PathEffect = SKPathEffect.Create2DPath( matrix, path ),
            Color = Colors.LightGray.ToSKColor(),
            IsAntialias = true
        };

        SKRect patternRect;
        SKRect clipRect;
        SKRoundRect clipRoundRect;

        if ( Vertical )
        {
            patternRect = new SKRect( sliderTop - pickerRadiusPixels, pickerRadiusPixels
                   , sliderTop + pickerRadiusPixels, canvasSize.Height - pickerRadiusPixels );
            clipRect = new SKRect( sliderTop - ( pickerRadiusPixels * 0.65f ), pickerRadiusPixels * 1.35f
                 , sliderTop + ( pickerRadiusPixels * 0.65f ), canvasSize.Height - ( pickerRadiusPixels * 1.35f ) );
            clipRoundRect = new SKRoundRect( clipRect, pickerRadiusPixels * 0.65f, pickerRadiusPixels * 0.65f );
        }
        else
        {
            patternRect = new SKRect( pickerRadiusPixels, sliderTop - pickerRadiusPixels
               , canvasSize.Width - pickerRadiusPixels, sliderTop + pickerRadiusPixels );
            clipRect = new SKRect( pickerRadiusPixels * 1.35f, sliderTop - ( pickerRadiusPixels * 0.65f )
               , canvasSize.Width - ( pickerRadiusPixels * 1.35f ), sliderTop + ( pickerRadiusPixels * 0.65f ) );
            clipRoundRect = new SKRoundRect( clipRect, pickerRadiusPixels * 0.65f, pickerRadiusPixels * 0.65f );
        }

        canvas.Save();
        canvas.ClipRoundRect( clipRoundRect );
        canvas.DrawRect( patternRect, paint );
        canvas.Restore();
    }

    bool IsInSliderArea( SKPoint point, float slidersHeight ) 
            => Vertical ? point.X >= slidersHeight - GetPickerRadiusPixels() && point.X <= slidersHeight + GetPickerRadiusPixels()
                        : point.Y >= slidersHeight - GetPickerRadiusPixels() && point.Y <= slidersHeight + GetPickerRadiusPixels();

    SKPoint LimitToSliderLocation( SKPoint point, float slidersOffset, SKSize canvasSize )
    {
        var result = new SKPoint( point.X, point.Y );

        if ( Vertical )
        {
            result.Y = result.Y >= GetPickerRadiusPixels() * 2 ? result.Y : GetPickerRadiusPixels() * 2;
            result.Y = result.Y <= canvasSize.Height - ( GetPickerRadiusPixels() * 2 ) ? result.Y
                : canvasSize.Height - ( GetPickerRadiusPixels() * 2 );
            result.X = slidersOffset;
        }
        else
        {
            result.X = result.X >= GetPickerRadiusPixels() * 2 ? result.X : GetPickerRadiusPixels() * 2;
            result.X = result.X <= canvasSize.Width - ( GetPickerRadiusPixels() * 2 ) ? result.X
                : canvasSize.Width - ( GetPickerRadiusPixels() * 2 );
            result.Y = slidersOffset;
        }

        return result;
    }
}
