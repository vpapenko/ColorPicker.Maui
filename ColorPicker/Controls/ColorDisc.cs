using ColorPicker.Core;

namespace ColorPicker.Controls;

public class ColorDisc : SkiaPickerBase
{
    static readonly HueSaturationDisc _hsDisc = new();
    static readonly Core.LuminosityRing _lRing = new();

    long?   _locationHsProgressId    = null;
    long?   _locationLProgressId     = null;

    SKPoint _locationHs              = new();
    SKPoint _locationL               = new();

    readonly SKColor[] _sweepGradientColors = new SKColor[256];

    public static readonly BindableProperty ShowLuminosityRingProperty
                         = BindableProperty.Create(nameof(ShowLuminosityRing),
                                                    typeof(bool),
                                                    typeof(ColorDisc),
                                                    true,
                                                    propertyChanged: HandleShowLuminosity);
    public bool ShowLuminosityRing
    {
        get => (bool)GetValue(ShowLuminosityRingProperty);
        set => SetValue(ShowLuminosityRingProperty, value);
    }
    static void HandleShowLuminosity(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
            ((ColorDisc)bindable).InvalidateSurface();
    }

    public static readonly BindableProperty CanvasBackgroundColorProperty
                         = BindableProperty.Create(nameof(CanvasBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorDisc),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleCanvasBackgroundColor);
    public Color CanvasBackgroundColor
    {
        get => (Color)GetValue(CanvasBackgroundColorProperty);
        set => SetValue(CanvasBackgroundColorProperty, value);
    }
    static void HandleCanvasBackgroundColor(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((ColorDisc)bindable).InvalidateSurface();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ColorDisc()
    {
        for (var i = 128; i >= -127; i--)
            _sweepGradientColors[255 - (i + 127)] = Color.FromHsla((i < 0 ? 255 + i : i) / 255D, 1, 0.5).ToSKColor();
    }

    public override float GetIndicatorRadiusPixels() => GetIndicatorRadiusPixels(GetCanvasSize());
    public override float GetIndicatorRadiusPixels(SKSize canvasSize) => GetSize(canvasSize) * IndicatorRadiusScale;

    protected override void OnTouchActionPressed(TouchActionEventArgs args)
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if (_locationHsProgressId is null && IsInHsArea(point, canvasRadius))
        {
            _locationHsProgressId = args.Id;
            _locationHs = LimitToHsRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId is null && IsInLArea(point, canvasRadius))
        {
            _locationLProgressId = args.Id;
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
    }

    protected override void OnTouchActionMoved(TouchActionEventArgs args)
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if (_locationHsProgressId == args.Id)
        {
            _locationHs = LimitToHsRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
    }

    protected override void OnTouchActionReleased(TouchActionEventArgs args)
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if (_locationHsProgressId == args.Id)
        {
            _locationHsProgressId = null;
            _locationHs = LimitToHsRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationLProgressId = null;
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
    }

    protected override void OnTouchActionCancelled(TouchActionEventArgs args)
    {
        if (_locationHsProgressId == args.Id)
            _locationHsProgressId = null;
        else if (_locationLProgressId == args.Id)
            _locationLProgressId = null;
    }

    protected override void OnPaintSurface(SKCanvas canvas, int width, int height)
    {
        var canvasRadius = GetSize() / 2F;

        UpdateLocations(SelectedColor, canvasRadius);
        canvas.Clear();
        PaintBackground(canvas, canvasRadius);

        if (ShowLuminosityRing)
        {
            PaintLGradient(canvas, canvasRadius);
            PaintIndicator(canvas, _locationL);
        }

        PaintColorSweepGradient(canvas, canvasRadius);
        PaintGrayRadialGradient(canvas, canvasRadius);
        PaintIndicator(canvas, _locationHs);
    }

    protected override void OnSelectedColorChanging(Color color)
            => InvalidateSurface();

    protected override SizeRequest GetMeasure(double widthConstraint, double heightConstraint)
    {
        if (double.IsPositiveInfinity(widthConstraint) &&
             double.IsPositiveInfinity(heightConstraint))
        {
            widthConstraint = 200;
            heightConstraint = 200;
        }

        var size = Math.Min(widthConstraint, heightConstraint);

        return new SizeRequest(new Size(size, size));
    }

    protected override float GetSize(SKSize canvasSize) => canvasSize.Width;
    protected override float GetSize() => GetSize(GetCanvasSize());

    void UpdateLocations(Color color, float canvasRadius)
    {
        var hsl = color.ToHsla();

        if (color.GetLuminosity() != 0 || !IsInHsArea(_locationHs, canvasRadius))
        {
            var hsUnit = _hsDisc.ColorToPoint(hsl);
            _locationHs = FromUnit(hsUnit, canvasRadius, HsRadius(canvasRadius));
        }

        var prevLUnit = ToUnit(_locationL, canvasRadius, LRadius(canvasRadius));
        var lUnit     = _lRing.ColorToPoint(hsl, prevLUnit);
        _locationL    = FromUnit(lUnit, canvasRadius, LRadius(canvasRadius));
    }

    void UpdateColors(float canvasRadius)
    {
        var hsl = SelectedColor.ToHsla();
        var hsUnit = ToUnit(_locationHs, canvasRadius, HsRadius(canvasRadius));
        var lUnit  = ToUnit(_locationL,  canvasRadius, LRadius(canvasRadius));

        hsl = _hsDisc.UpdateColor(hsUnit, hsl);
        hsl = _lRing .UpdateColor(lUnit,  hsl);

        SelectedColor = hsl.ToMauiColor();
    }

    bool IsInHsArea(SKPoint point, float canvasRadius)
        => _hsDisc.IsInActiveArea(ToUnit(point, canvasRadius, HsRadius(canvasRadius)), default);

    bool IsInLArea(SKPoint point, float canvasRadius)
    {
        if (!ShowLuminosityRing)
            return false;

        // Hit tolerance lives in pixel space (half the indicator radius); convert
        // to unit-square units by scaling against the L-ring radius.
        var tolUnits = (GetIndicatorRadiusPixels() / 2F) / (2F * LRadius(canvasRadius));
        var ring = new Core.LuminosityRing(tolUnits);
        return ring.IsInActiveArea(ToUnit(point, canvasRadius, LRadius(canvasRadius)), default);
    }

    void PaintBackground(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Color = CanvasBackgroundColor.ToSKColor()
        };

        canvas.DrawCircle(center, canvasRadius - GetIndicatorRadiusPixels(), paint);
    }

    void PaintLGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var colors = new List<SKColor>()
        {
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 1.0).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.0).ToSKColor(),
            Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor()
        };

        var shader = SKShader.CreateSweepGradient(center, colors.ToArray(), null);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Stroke,
            StrokeWidth = GetIndicatorRadiusPixels()
        };
        canvas.DrawCircle(center, LRadius(canvasRadius), paint);
    }

    void PaintColorSweepGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var shader = SKShader.CreateSweepGradient(center, _sweepGradientColors, null);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };
        canvas.DrawCircle(center, HsRadius(canvasRadius), paint);
    }

    void PaintGrayRadialGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var colors = new SKColor[]
        {
            SKColors.Gray,
            SKColors.Transparent
        };

        var shader = SKShader.CreateRadialGradient(center, HsRadius(canvasRadius), colors, null, SKShaderTileMode.Clamp);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };
        canvas.DrawPaint(paint);
    }

    SKPoint LimitToHsRadius(SKPoint point, float canvasRadius)
    {
        var unit = ToUnit(point, canvasRadius, HsRadius(canvasRadius));
        var fit  = _hsDisc.FitToActiveArea(unit, default);
        return FromUnit(fit, canvasRadius, HsRadius(canvasRadius));
    }

    SKPoint LimitToLRadius(SKPoint point, float canvasRadius)
    {
        var unit = ToUnit(point, canvasRadius, LRadius(canvasRadius));
        var fit  = _lRing.FitToActiveArea(unit, default);
        return FromUnit(fit, canvasRadius, LRadius(canvasRadius));
    }

    // Pixel ↔ unit-square coordinate bridge (per active-area radius).
    static UnitPoint ToUnit(SKPoint pixel, float canvasRadius, float activeRadius)
        => new((float)((pixel.X - canvasRadius) / (2.0 * activeRadius) + 0.5),
               (float)((pixel.Y - canvasRadius) / (2.0 * activeRadius) + 0.5));

    static SKPoint FromUnit(UnitPoint unit, float canvasRadius, float activeRadius)
        => new((float)((unit.X - 0.5) * 2.0 * activeRadius + canvasRadius),
               (float)((unit.Y - 0.5) * 2.0 * activeRadius + canvasRadius));

    // Small margin so the picker indicator (outer stroke + antialiasing)
    // does not get clipped at the canvas edge.
    const float PickerEdgeMargin = 3F;

    float HsRadius(float canvasRadius)
       => !ShowLuminosityRing ? canvasRadius - GetIndicatorRadiusPixels() - PickerEdgeMargin
                                : canvasRadius - (3 * GetIndicatorRadiusPixels()) - 2;

    float LRadius(float canvasRadius)
       => canvasRadius - GetIndicatorRadiusPixels() - PickerEdgeMargin;
}
