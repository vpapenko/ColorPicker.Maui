namespace ColorPicker.BaseClasses;

public abstract class SliderPicker : SkiaSharpPickerBase
{
    readonly List<SliderLocation> _sliders = new();

    //	Constructor
    //
    public SliderPicker()
    {
        // Default to 0 (sentinel: "auto-fill"). Standalone sliders fill all
        // available space when picker radius is unset; explicit PickerRadiusScale
        // > 0 makes the slider stack aspect-locked (thickness derived from
        // radius, length still fills).
        PickerRadiusScale = 0F;
        UpdateSliders();
    }

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

    public override float GetPickerRadiusPixels( SKSize canvasSize )
    {
        // Explicit PickerRadiusScale > 0: derive picker radius from the LENGTH
        // axis (parallel to the slider's value direction). Thickness then becomes
        // a fixed function of radius (see GetMeasure), making the slider
        // aspect-locked.
        if ( PickerRadiusScale > 0F )
        {
            var length = Vertical ? canvasSize.Height : canvasSize.Width;
            return PickerRadiusScale * length;
        }
        // Auto: thickness fills available orthogonal space; picker scales to fit.
        return ( Vertical ? canvasSize.Width : canvasSize.Height ) / _sliders.Count / 2.2F;
    }
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
        // When PickerRadiusScale is explicitly set, the slider stack becomes
        // aspect-locked: the LENGTH axis fills, the THICKNESS axis is derived
        // from the picker radius. thickness = count * 2.2 * pickerRadius
        //                                   = count * 2.2 * scale * length.
        if ( PickerRadiusScale > 0F )
        {
            if ( Vertical )
            {
                // length = height, thickness = width
                var length = double.IsInfinity( heightConstraint ) ? 200 : heightConstraint;
                var thickness = _sliders.Count * 2.2 * PickerRadiusScale * length;
                if ( !double.IsInfinity( widthConstraint ) && thickness > widthConstraint )
                    thickness = widthConstraint;
                return new SizeRequest( new Size( thickness, length ) );
            }
            else
            {
                // length = width, thickness = height
                var length = double.IsInfinity( widthConstraint ) ? 200 : widthConstraint;
                var thickness = _sliders.Count * 2.2 * PickerRadiusScale * length;
                if ( !double.IsInfinity( heightConstraint ) && thickness > heightConstraint )
                    thickness = heightConstraint;
                return new SizeRequest( new Size( length, thickness ) );
            }
        }

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
            if ( slider.LocationProgressId is null )
            {
                var pr = GetPickerRadiusPixels();
                var left = ( pr * 1.1F ) + (SlidersWidht(canvasSize) * slider.Slider.NewValue(color));
                slider.Location = Vertical
                    ? new SKPoint( slider.GetSliderOffset( pr ), left )
                    : new SKPoint( left, slider.GetSliderOffset( pr ) );
            }
        }
    }

    float SlidersWidht( SKSize canvasSize ) 
       => Vertical ? canvasSize.Height - ( GetPickerRadiusPixels() * 2.2F )
                   : canvasSize.Width - ( GetPickerRadiusPixels() * 2.2F );

    void UpdateColors( SliderLocation slider, SKSize canvasSize )
    {
        var newColor = SelectedColor;
        var pr = GetPickerRadiusPixels();
        var newValue = Vertical ? ( slider.Location.Y - ( pr * 1.1F ) ) / SlidersWidht( canvasSize )
                                : ( slider.Location.X - ( pr * 1.1F ) ) / SlidersWidht( canvasSize );

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
            startPoint = new SKPoint( sliderTop, pickerRadiusPixels * 1.1F );
            endPoint = new SKPoint( sliderTop, canvasSize.Height - ( pickerRadiusPixels * 1.1F ) );
        }
        else
        {
            startPoint = new SKPoint( pickerRadiusPixels * 1.1F, sliderTop );
            endPoint = new SKPoint( canvasSize.Width - ( pickerRadiusPixels * 1.1F ), sliderTop );
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

        // Slider line center spans [pr*1.1, length - pr*1.1] but its round-cap stroke
        // (thickness = 1.3*pr) extends 0.65*pr beyond each endpoint, so the visible
        // pill spans [pr*0.45, length - pr*0.45]. Chess clip must match that pill.
        float endInset = pickerRadiusPixels * 0.45F;
        if ( Vertical )
        {
            patternRect = new SKRect( sliderTop - pickerRadiusPixels, endInset
                   , sliderTop + pickerRadiusPixels, canvasSize.Height - endInset );
            clipRect = new SKRect( sliderTop - ( pickerRadiusPixels * 0.65f ), endInset
                 , sliderTop + ( pickerRadiusPixels * 0.65f ), canvasSize.Height - endInset );
            clipRoundRect = new SKRoundRect( clipRect, pickerRadiusPixels * 0.65f, pickerRadiusPixels * 0.65f );
        }
        else
        {
            patternRect = new SKRect( endInset, sliderTop - pickerRadiusPixels
               , canvasSize.Width - endInset, sliderTop + pickerRadiusPixels );
            clipRect = new SKRect( endInset, sliderTop - ( pickerRadiusPixels * 0.65f )
               , canvasSize.Width - endInset, sliderTop + ( pickerRadiusPixels * 0.65f ) );
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
        var endMargin = GetPickerRadiusPixels() * 1.1F;

        if ( Vertical )
        {
            result.Y = result.Y >= endMargin ? result.Y : endMargin;
            result.Y = result.Y <= canvasSize.Height - endMargin ? result.Y
                : canvasSize.Height - endMargin;
            result.X = slidersOffset;
        }
        else
        {
            result.X = result.X >= endMargin ? result.X : endMargin;
            result.X = result.X <= canvasSize.Width - endMargin ? result.X
                : canvasSize.Width - endMargin;
            result.Y = slidersOffset;
        }

        return result;
    }
}
