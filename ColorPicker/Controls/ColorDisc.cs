using ColorPicker.Core;
using ColorPicker.Core.Interaction;

namespace ColorPicker.Controls;

/// <summary>
/// The raw hue/saturation disc that powers <see cref="ColorWheel"/>. Can be used on
/// its own; shows an optional luminosity ring.
/// </summary>
public class ColorDisc : SkiaPickerBase
{
    // Interaction state lives in a pure controller so the per-region
    // split (HS-only updates from HS touches, L-only updates from L
    // touches) can be exercised by deterministic Core unit tests.
    readonly ColorDiscInteraction _interaction = new();

    long?   _locationHsProgressId    = null;
    long?   _locationLProgressId     = null;

    readonly SKColor[] _sweepGradientColors = new SKColor[256];

    public static readonly BindableProperty ShowLuminosityRingProperty
                         = BindableProperty.Create(nameof(ShowLuminosityRing),
                                                    typeof(bool),
                                                    typeof(ColorDisc),
                                                    true,
                                                    propertyChanged: HandleShowLuminosity);
    /// <summary>Whether to draw the luminosity ring around the disc. Default <c>true</c>.</summary>
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
    /// <summary>Fill drawn behind the disc. Default <see cref="Colors.Transparent"/>.</summary>
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

        var hsUnit = PixelToUnit(point, canvasRadius, HsRadius(canvasRadius));
        var lUnit  = PixelToUnit(point, canvasRadius, LRadius(canvasRadius));

        if (_locationHsProgressId is null && _interaction.IsInHs(hsUnit))
        {
            _locationHsProgressId = args.Id;
            WriteSelectedColor(_interaction.UpdateFromHs(hsUnit));
        }
        else if (_locationLProgressId is null && IsInLRing(lUnit, canvasRadius))
        {
            _locationLProgressId = args.Id;
            WriteSelectedColor(_interaction.UpdateFromL(lUnit));
        }
    }

    protected override void OnTouchActionMoved(TouchActionEventArgs args)
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if (_locationHsProgressId == args.Id)
        {
            var hsUnit = PixelToUnit(point, canvasRadius, HsRadius(canvasRadius));
            WriteSelectedColor(_interaction.UpdateFromHs(hsUnit));
        }
        else if (_locationLProgressId == args.Id)
        {
            var lUnit = PixelToUnit(point, canvasRadius, LRadius(canvasRadius));
            WriteSelectedColor(_interaction.UpdateFromL(lUnit));
        }
    }

    protected override void OnTouchActionReleased(TouchActionEventArgs args)
    {
        var canvasRadius    = GetCanvasSize().Width / 2F;
        var point           = ConvertToPixel(args.Location);

        if (_locationHsProgressId == args.Id)
        {
            _locationHsProgressId = null;
            var hsUnit = PixelToUnit(point, canvasRadius, HsRadius(canvasRadius));
            WriteSelectedColor(_interaction.UpdateFromHs(hsUnit));
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationLProgressId = null;
            var lUnit = PixelToUnit(point, canvasRadius, LRadius(canvasRadius));
            WriteSelectedColor(_interaction.UpdateFromL(lUnit));
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

        // Re-sync from SelectedColor each paint so the controller picks up
        // any external bindable-property change (incl. the initial default
        // that never fires OnSelectedColorChanging). Mirrors baseline's
        // UpdateLocations(SelectedColor, ...) call.
        _interaction.SyncFromColor(SelectedColor.ToHsla());

        var locationHs = UnitToPixel(_interaction.LocationHs, canvasRadius, HsRadius(canvasRadius));
        var locationL  = UnitToPixel(_interaction.LocationL,  canvasRadius, LRadius(canvasRadius));

        canvas.Clear();
        PaintBackground(canvas, canvasRadius);

        if (ShowLuminosityRing)
        {
            PaintLGradient(canvas, canvasRadius);
            PaintIndicator(canvas, locationL);
        }

        PaintColorSweepGradient(canvas, canvasRadius);
        PaintGrayRadialGradient(canvas, canvasRadius);
        PaintIndicator(canvas, locationHs);
    }

    protected override void OnSelectedColorChanging(Color color)
    {
        _interaction.SyncFromColor(color.ToHsla());
        InvalidateSurface();
    }

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

    void WriteSelectedColor(HslaColor hsla)
    {
        SelectedColor = hsla.ToMauiColor();
    }

    bool IsInLRing(UnitPoint lUnit, float canvasRadius)
    {
        if (!ShowLuminosityRing)
            return false;
        // Hit tolerance lives in pixel space (half the indicator radius); convert
        // to unit-square units by scaling against the L-ring radius.
        var tolUnits = (GetIndicatorRadiusPixels() / 2F) / (2F * LRadius(canvasRadius));
        return _interaction.IsInL(lUnit, tolUnits);
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

    // Pixel ↔ unit-square coordinate bridge (per active-area radius).
    static UnitPoint PixelToUnit(SKPoint pixel, float canvasRadius, float activeRadius)
        => new((float)((pixel.X - canvasRadius) / (2.0 * activeRadius) + 0.5),
               (float)((pixel.Y - canvasRadius) / (2.0 * activeRadius) + 0.5));

    static SKPoint UnitToPixel(UnitPoint unit, float canvasRadius, float activeRadius)
        => new((float)((unit.X - 0.5) * 2.0 * activeRadius + canvasRadius),
               (float)((unit.Y - 0.5) * 2.0 * activeRadius + canvasRadius));

    // Uses the bindable IndicatorPadding (default 3px) so the indicator's outer
    // stroke + antialiasing never clip at the canvas edge, consistently with the
    // triangle and sliders.
    float HsRadius(float canvasRadius)
       => !ShowLuminosityRing ? canvasRadius - GetIndicatorRadiusPixels() - IndicatorPadding
                                : canvasRadius - (3 * GetIndicatorRadiusPixels()) - 2;

    float LRadius(float canvasRadius)
       => canvasRadius - GetIndicatorRadiusPixels() - IndicatorPadding;
}
