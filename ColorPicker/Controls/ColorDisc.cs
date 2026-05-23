namespace ColorPicker.Controls;

public class ColorDisc : SkiaPickerBase
{
    long?   _locationHsProgressId    = null;
    long?   _locationLProgressId     = null;

    SKPoint _locationHs              = new();
    SKPoint _locationL               = new();

    readonly SKColor[] _sweepGradientColors = new SKColor[256];

    public static readonly BindableProperty ShowLuminosityRingProperty 
                         = BindableProperty.Create( nameof(ShowLuminosityRing),
                                                    typeof(bool),
                                                    typeof(ColorDisc),
                                                    true,
                                                    propertyChanged: HandleShowLuminosity );
    public bool ShowLuminosityRing
    {
        get => (bool)GetValue( ShowLuminosityRingProperty );
        set => SetValue( ShowLuminosityRingProperty, value );
    }
    static void HandleShowLuminosity( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
            ( (ColorDisc)bindable ).InvalidateSurface();
    }

    public static readonly BindableProperty CanvasBackgroundColorProperty 
                         = BindableProperty.Create( nameof(CanvasBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorDisc),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleCanvasBackgroundColor );
    public Color CanvasBackgroundColor
    {
        get => (Color)GetValue( CanvasBackgroundColorProperty );
        set => SetValue( CanvasBackgroundColorProperty, value );
    }
    static void HandleCanvasBackgroundColor( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue != oldValue )
        {
            ( (ColorDisc)bindable ).InvalidateSurface();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ColorDisc()
    {
        for ( var i = 128; i >= -127; i-- )
            _sweepGradientColors[ 255 - ( i + 127 ) ] = Color.FromHsla( ( i < 0 ? 255 + i : i ) / 255D, 1, 0.5 ).ToSKColor();
    }

    public override float GetIndicatorRadiusPixels() => GetIndicatorRadiusPixels( GetCanvasSize() );
    public override float GetIndicatorRadiusPixels( SKSize canvasSize ) => GetSize( canvasSize ) * IndicatorRadiusScale;

    protected override void OnTouchActionPressed( TouchActionEventArgs args )
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if ( _locationHsProgressId is null && IsInHsArea( point, canvasRadius ) )
        {
            _locationHsProgressId    = args.Id;
            _locationHs              = LimitToHsRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
        else if ( _locationLProgressId is null && IsInLArea( point, canvasRadius ) )
        {
            _locationLProgressId     = args.Id;
            _locationL               = LimitToLRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
    }

    protected override void OnTouchActionMoved( TouchActionEventArgs args )
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if ( _locationHsProgressId == args.Id )
        {
            _locationHs = LimitToHsRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
        else if ( _locationLProgressId == args.Id )
        {
            _locationL = LimitToLRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
    }

    protected override void OnTouchActionReleased( TouchActionEventArgs args )
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if ( _locationHsProgressId == args.Id )
        {
            _locationHsProgressId    = null;
            _locationHs              = LimitToHsRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
        else if ( _locationLProgressId == args.Id )
        {
            _locationLProgressId     = null;
            _locationL               = LimitToLRadius( point, canvasRadius );
            UpdateColors( canvasRadius );
        }
    }

    protected override void OnTouchActionCancelled( TouchActionEventArgs args )
    {
        if ( _locationHsProgressId == args.Id )
            _locationHsProgressId = null;
        else if ( _locationLProgressId == args.Id )
            _locationLProgressId = null;
    }

    protected override void OnPaintSurface( SKCanvas canvas, int width, int height )
    {
        var canvasRadius = GetSize() / 2F;

        UpdateLocations( SelectedColor, canvasRadius );
        canvas.Clear();
        PaintBackground( canvas, canvasRadius );

        if ( ShowLuminosityRing )
        {
            PaintLGradient( canvas, canvasRadius );
            PaintIndicator( canvas, _locationL );
        }

        PaintColorSweepGradient( canvas, canvasRadius );
        PaintGrayRadialGradient( canvas, canvasRadius );
        PaintIndicator( canvas, _locationHs );
    }

    protected override void OnSelectedColorChanging( Color color ) 
            => InvalidateSurface();

    protected override SizeRequest GetMeasure( double widthConstraint, double heightConstraint )
    {
        if ( double.IsPositiveInfinity( widthConstraint ) &&
             double.IsPositiveInfinity( heightConstraint ) )
        {
            widthConstraint  = 200;
            heightConstraint = 200;
        }

        var size = Math.Min( widthConstraint, heightConstraint );

        return new SizeRequest( new Size( size, size ) );
    }

    protected override float GetSize( SKSize canvasSize )   => canvasSize.Width;
    protected override float GetSize()                      => GetSize( GetCanvasSize() );

    void UpdateLocations( Color color, float canvasRadius )
    {
        if ( color.GetLuminosity() != 0 || !IsInHsArea( _locationHs, canvasRadius ) )
        {
            var angleHs  = (0.5 - color.GetHue()) * (2 * Math.PI);
            var radiusHs = HsRadius(canvasRadius) * color.GetSaturation();

            var resultHs = FromPolar(new PolarPoint( (float)radiusHs, (float)angleHs) );
            resultHs.X  += canvasRadius;
            resultHs.Y  += canvasRadius;
            _locationHs   = resultHs;
        }

        var polarL      = ToPolar(ToLCoordinates(_locationL, canvasRadius));
        polarL.Angle   -= (float)Math.PI / 2F;
        var signOld     = polarL.Angle <= 0 ? 1 : -1;
        var angleL      = color.GetLuminosity() * Math.PI * signOld;

        var resultL     = FromPolar( new PolarPoint( LRadius(canvasRadius), (float)(angleL - (Math.PI / 2)) ) );
        resultL.X      += canvasRadius;
        resultL.Y      += canvasRadius;
        _locationL       = resultL;
    }

    void UpdateColors( float canvasRadius )
    {
        var hsPoint    = ToHsCoordinates(_locationHs, canvasRadius);
        var lPoint     = ToLCoordinates(_locationL, canvasRadius);

        var newColor        = WheelPointToColor(hsPoint, lPoint);
        SelectedColor       = newColor;
    }

    bool IsInHsArea( SKPoint point, float canvasRadius )
    {
        var polar = ToPolar( new SKPoint( point.X - canvasRadius, point.Y - canvasRadius ) );
        return polar.Radius <= HsRadius( canvasRadius );
    }

    bool IsInLArea( SKPoint point, float canvasRadius )
    {
        if ( !ShowLuminosityRing )
            return false;

        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));

        return polar.Radius <= LRadius( canvasRadius ) + ( GetIndicatorRadiusPixels() / 2F )
            && polar.Radius >= LRadius( canvasRadius ) - ( GetIndicatorRadiusPixels() / 2F );
    }

    void PaintBackground( SKCanvas canvas, float canvasRadius )
    {
        var center = new SKPoint( canvasRadius, canvasRadius );

        var paint = new SKPaint
        {
            IsAntialias = true,
            Color = CanvasBackgroundColor.ToSKColor()
        };

        canvas.DrawCircle( center, canvasRadius - GetIndicatorRadiusPixels(), paint );
    }

    void PaintLGradient( SKCanvas canvas, float canvasRadius )
    {
        var center = new SKPoint( canvasRadius, canvasRadius );

        var colors = new List<SKColor>()
        {
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 1.0).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.0).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor()
        };

        var shader = SKShader.CreateSweepGradient( center, colors.ToArray(), null );

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Stroke,
            StrokeWidth = GetIndicatorRadiusPixels()
        };
        canvas.DrawCircle( center, LRadius( canvasRadius ), paint );
    }

    void PaintColorSweepGradient( SKCanvas canvas, float canvasRadius )
    {
        var center = new SKPoint( canvasRadius, canvasRadius );

        var shader = SKShader.CreateSweepGradient( center, _sweepGradientColors, null );

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };
        canvas.DrawCircle( center, HsRadius( canvasRadius ), paint );
    }

    void PaintGrayRadialGradient( SKCanvas canvas, float canvasRadius )
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var colors = new SKColor[] 
        {
            SKColors.Gray,
            SKColors.Transparent
        };

        var shader = SKShader.CreateRadialGradient( center, HsRadius(canvasRadius), colors, null, SKShaderTileMode.Clamp );

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };
        canvas.DrawPaint( paint );
    }

    SKPoint ToHsCoordinates( SKPoint point, float canvasRadius )
    {
        var result = new SKPoint( point.X, point.Y );

        result.X  -= canvasRadius;
        result.Y  -= canvasRadius;
        result.X  /= HsRadius( canvasRadius );
        result.Y  /= HsRadius( canvasRadius );

        return result;
    }

    SKPoint ToLCoordinates( SKPoint point, float canvasRadius )
    {
        var result = new SKPoint( point.X, point.Y );

        result.X  -= canvasRadius;
        result.Y  -= canvasRadius;
        result.X  /= LRadius( canvasRadius );
        result.Y  /= LRadius( canvasRadius );

        return result;
    }

    Color WheelPointToColor( SKPoint pointHS, SKPoint pointL )
    {
        var polarHS     = ToPolar(pointHS);
        var polarL      = ToPolar(pointL);

        polarL.Angle   += (float)Math.PI / 2F;
        polarL          = ToPolar( FromPolar( polarL ) );

        var h   = (Math.PI - polarHS.Angle) / (2 * Math.PI);
        var s   = polarHS.Radius;
        var l   = Math.Abs(polarL.Angle) / Math.PI;

        return Color.FromHsla( h, s, l, SelectedColor.Alpha );
    }

    SKPoint LimitToHsRadius( SKPoint point, float canvasRadius )
    {
        var polar       = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        polar.Radius    = polar.Radius < HsRadius( canvasRadius ) ? polar.Radius : HsRadius( canvasRadius );
        var result      = FromPolar(polar);

        result.X       += canvasRadius;
        result.Y       += canvasRadius;

        return result;
    }

    SKPoint LimitToLRadius( SKPoint point, float canvasRadius )
    {
        var polar       = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        polar.Radius    = LRadius( canvasRadius );
        var result      = FromPolar(polar);

        result.X       += canvasRadius;
        result.Y       += canvasRadius;

        return result;
    }

    static PolarPoint ToPolar( SKPoint point )
    {
        var radius    = (float)Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        var angle     = (float)Math.Atan2(point.Y, point.X);

        return new PolarPoint( radius, angle );
    }

    static SKPoint FromPolar( PolarPoint point )
    {
        var x     = (float)(point.Radius * Math.Cos(point.Angle));
        var y     = (float)(point.Radius * Math.Sin(point.Angle));

        return new SKPoint( x, y );
    }

    // Small margin so the picker indicator (outer stroke + antialiasing)
    // does not get clipped at the canvas edge.
    const float PickerEdgeMargin = 3F;

    float HsRadius( float canvasRadius )
       => ! ShowLuminosityRing ? canvasRadius - GetIndicatorRadiusPixels() - PickerEdgeMargin
                                : canvasRadius - ( 3 * GetIndicatorRadiusPixels() ) - 2;

    float LRadius( float canvasRadius )
       => canvasRadius - GetIndicatorRadiusPixels() - PickerEdgeMargin;
}
