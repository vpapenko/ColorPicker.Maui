using ColorPicker.Core;
using PolarPoint = ColorPicker.Classes.PolarPoint;

namespace ColorPicker.Controls;

public class ColorTriangleArea : SkiaPickerBase
{
    static readonly SaturationValueTriangle _triangleRotated = new(rotateByHue: true);
    static readonly SaturationValueTriangle _triangleFixed   = new(rotateByHue: false);
    static readonly HueRing                 _hueRing         = new();

    double _lastHue = 0;
    bool _zeroSL = false;
    long? _locationSvProgressId = null;
    long? _locationHProgressId = null;
    SKPoint _locationSv = new();
    SKPoint _locationH1 = new();
    SKPoint _locationH2 = new();
    SKPoint _locationMiddleH = new();

    readonly SKColor[] _sweepGradientColors = new SKColor[256];

    public static readonly BindableProperty CanvasBackgroundColorProperty
                         = BindableProperty.Create(nameof(CanvasBackgroundColor),
                                                    typeof(Color),
                                                    typeof(ColorTriangleArea),
                                                    Colors.Transparent,
                                                    propertyChanged: HandleCanvasBackgroundColorChanged);
    public Color CanvasBackgroundColor
    {
        get => (Color)GetValue(CanvasBackgroundColorProperty);
        set => SetValue(CanvasBackgroundColorProperty, value);
    }

    static void HandleCanvasBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((ColorTriangleArea)bindable).InvalidateSurface();
        }
    }

    public static readonly BindableProperty RotateTriangleByHueProperty
                         = BindableProperty.Create(nameof(RotateTriangleByHue),
                                                    typeof(bool),
                                                    typeof(ColorTriangleArea),
                                                    true,
                                                    propertyChanged: HandleRotateTriangleByHueSet);
    public bool RotateTriangleByHue
    {
        get => (bool)GetValue(RotateTriangleByHueProperty);
        set => SetValue(RotateTriangleByHueProperty, value);
    }

    static void HandleRotateTriangleByHueSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((ColorTriangleArea)bindable).InvalidateSurface();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ColorTriangleArea() : base()
    {
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        IndicatorRadiusScale = 0.035F;
        for (var i = 128; i >= -127; i--)
        {
            _sweepGradientColors[255 - (i + 127)] = Color.FromHsla((i < 0 ? 255 + i : i) / 255D, 1, 0.5).ToSKColor();
        }
    }

    public override float GetIndicatorRadiusPixels(SKSize canvasSize) => GetSize(canvasSize) * IndicatorRadiusScale;
    public override float GetIndicatorRadiusPixels() => GetIndicatorRadiusPixels(GetCanvasSize());

    protected override void OnTouchActionPressed(TouchActionEventArgs args)
    {
        var canvasRadius = GetSize() / 2F;
        var (offX, offY) = GetDrawingOffset();
        var point = ConvertToPixel(args.Location);
        point.X -= offX;
        point.Y -= offY;

        if (_locationSvProgressId is null && IsInSvArea(point, canvasRadius))
        {
            _locationSvProgressId = args.Id;
            _locationSv = LimitToSvTriangle(point, canvasRadius);
            UpdateColorsFromSv(canvasRadius);
        }
        else if (_locationHProgressId is null && IsInHArea(point, canvasRadius))
        {
            _locationHProgressId = args.Id;
            LimitToHRadius(point, canvasRadius);
            UpdateColorsFromH(canvasRadius);
        }
    }

    protected override void OnTouchActionMoved(TouchActionEventArgs args)
    {
        var canvasRadius = GetSize() / 2F;
        var (offX, offY) = GetDrawingOffset();
        var point = ConvertToPixel(args.Location);
        point.X -= offX;
        point.Y -= offY;

        if (_locationSvProgressId == args.Id)
        {
            _locationSv = LimitToSvTriangle(point, canvasRadius);
            UpdateColorsFromSv(canvasRadius);
        }
        else if (_locationHProgressId == args.Id)
        {
            LimitToHRadius(point, canvasRadius);
            UpdateColorsFromH(canvasRadius);
        }
    }

    protected override void OnTouchActionReleased(TouchActionEventArgs args)
    {
        var canvasRadius = GetSize() / 2F;
        var (offX, offY) = GetDrawingOffset();
        var point = ConvertToPixel(args.Location);
        point.X -= offX;
        point.Y -= offY;

        if (_locationSvProgressId == args.Id)
        {
            _locationSvProgressId = null;
            _locationSv = LimitToSvTriangle(point, canvasRadius);
            UpdateColorsFromSv(canvasRadius);
        }
        else if (_locationHProgressId == args.Id)
        {
            _locationHProgressId = null;
            LimitToHRadius(point, canvasRadius);
            UpdateColorsFromH(canvasRadius);
        }
    }

    protected override void OnTouchActionCancelled(TouchActionEventArgs args)
    {
        if (_locationSvProgressId == args.Id)
            _locationSvProgressId = null;
        else if (_locationHProgressId == args.Id)
            _locationHProgressId = null;
    }

    protected override void OnPaintSurface(SKCanvas canvas, int width, int height)
    {
        var canvasRadius = GetSize() / 2F;
        var (offX, offY) = GetDrawingOffset();

        UpdateLocations(SelectedColor, canvasRadius);
        canvas.Clear();

        canvas.Save();
        canvas.Translate(offX, offY);

        PaintBackground(canvas, canvasRadius);
        PaintHGradient(canvas, canvasRadius);

        if (RotateTriangleByHue)
            PaintLinePicker(canvas);
        else
            PaintIndicator(canvas, _locationMiddleH);

        PaintSvTriangle(canvas, canvasRadius);
        PaintIndicator(canvas, _locationSv);

        canvas.Restore();
    }

    protected override void OnSelectedColorChanging(Color color)
    {
        if (color.GetSaturation() > 0.00390625D)
        {
            _lastHue = color.GetHue();
            _zeroSL = false;
        }
        else
        {
            _zeroSL = true;
        }

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

    protected override float GetSize(SKSize canvasSize) => Math.Min(canvasSize.Width, canvasSize.Height);
    protected override float GetSize() => GetSize(GetCanvasSize());

    (float offsetX, float offsetY) GetDrawingOffset()
    {
        var canvas = GetCanvasSize();
        var size   = Math.Min(canvas.Width, canvas.Height);
        return ((canvas.Width - size) / 2F, (canvas.Height - size) / 2F);
    }

    SaturationValueTriangle Triangle => RotateTriangleByHue ? _triangleRotated : _triangleFixed;

    void UpdateLocations(Color color, float canvasRadius)
    {
        // Use _lastHue (not color.GetHue()) so the SV indicator stays put when
        // the selected color is grayscale — matches the existing MAUI behavior.
        var hsla = new HslaColor(_lastHue,
                                 color.GetSaturation(),
                                 color.GetLuminosity(),
                                 color.Alpha);

        var svUnit = Triangle.ColorToPoint(hsla);
        _locationSv = FromUnit(svUnit, canvasRadius, SvRadius(canvasRadius));

        var angleH = _lastHue * Math.PI * 2;
        _locationMiddleH = FromPolar(new PolarPoint(HRadius(canvasRadius),                                  (float)(Math.PI - angleH)));
        _locationMiddleH = OffsetByCenter(_locationMiddleH, canvasRadius);

        _locationH1 = FromPolar(new PolarPoint(HRadius(canvasRadius) + GetIndicatorRadiusPixels(),          (float)(Math.PI - angleH)));
        _locationH1 = OffsetByCenter(_locationH1, canvasRadius);

        _locationH2 = FromPolar(new PolarPoint(HRadius(canvasRadius) - GetIndicatorRadiusPixels(),          (float)(Math.PI - angleH)));
        _locationH2 = OffsetByCenter(_locationH2, canvasRadius);
    }

    // Decode only the SV indicator. Hue is left untouched so the SV-only
    // drag never roundtrips H through pixel quantization.
    void UpdateColorsFromSv(float canvasRadius)
    {
        var hsla = new HslaColor(_lastHue,
                                 SelectedColor.GetSaturation(),
                                 SelectedColor.GetLuminosity(),
                                 SelectedColor.Alpha);

        var svUnit = ToUnit(_locationSv, canvasRadius, SvRadius(canvasRadius));
        hsla = Triangle.UpdateColor(svUnit, hsla);

        WriteSelectedColor(hsla);
    }

    // Decode only the hue ring. SV is left untouched so dragging the hue
    // ring (especially on the rotating triangle) never re-quantizes S/L
    // through the encode/decode roundtrip — that was the source of the
    // speed-dependent S/L drift.
    void UpdateColorsFromH(float canvasRadius)
    {
        var hsla = new HslaColor(_lastHue,
                                 SelectedColor.GetSaturation(),
                                 SelectedColor.GetLuminosity(),
                                 SelectedColor.Alpha);

        var hUnit = ToUnit(_locationH1, canvasRadius, HRadius(canvasRadius));
        hsla = _hueRing.UpdateColor(hUnit, hsla);

        WriteSelectedColor(hsla);
    }

    void WriteSelectedColor(HslaColor hsla)
    {
        var newColor = hsla.ToMauiColor();

        if (_zeroSL && (newColor.GetSaturation() > 0))
        {
            newColor = Color.FromHsla(_lastHue, newColor.GetSaturation(), newColor.GetLuminosity(), newColor.Alpha);
        }

        _lastHue = hsla.H;
        SelectedColor = newColor;
    }

    bool IsInSvArea(SKPoint point, float canvasRadius)
        => Triangle.IsInActiveArea(ToUnit(point, canvasRadius, SvRadius(canvasRadius)), default);

    bool IsInHArea(SKPoint point, float canvasRadius)
    {
        // MAUI tolerance: ±indicatorPx in pixel space.
        var tolUnits = GetIndicatorRadiusPixels() / (2F * HRadius(canvasRadius));
        return new HueRing(tolUnits).IsInActiveArea(ToUnit(point, canvasRadius, HRadius(canvasRadius)), default);
    }

    void PaintBackground(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);
        var paint = new SKPaint
        {
            IsAntialias = true,
            Color = CanvasBackgroundColor.ToSKColor()
        };

        canvas.DrawCircle(center, canvasRadius, paint);
    }

    void PaintHGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);
        var shader = SKShader.CreateSweepGradient(center, _sweepGradientColors, null);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = shader,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = GetIndicatorRadiusPixels() * 2
        };
        canvas.DrawCircle(center, HRadius(canvasRadius), paint);
    }

    void PaintSvTriangle(SKCanvas canvas, float canvasRadius)
    {
        canvas.Save();

        var rotationHue = SKMatrix.CreateRotation(-(float)((2D * Math.PI * _lastHue) + (Math.PI / 2D)),
                                                   canvasRadius, canvasRadius);

        if (RotateTriangleByHue)
        {
            canvas.Concat(ref rotationHue);
        }

        var point1 = new SKPoint(canvasRadius, canvasRadius - SvRadius(canvasRadius));
        var point2 = new SKPoint(canvasRadius + (_triangleSide * SvRadius(canvasRadius))
                , canvasRadius + (_triangleVerticalOffset * SvRadius(canvasRadius)));

        var point3 = new SKPoint(canvasRadius - (_triangleSide * SvRadius(canvasRadius))
                , canvasRadius + (_triangleVerticalOffset * SvRadius(canvasRadius)));

        using (var pathTriangle = new SKPath())
        {
            pathTriangle.MoveTo(point1);
            pathTriangle.LineTo(point2);
            pathTriangle.LineTo(point3);

            canvas.ClipPath(pathTriangle, SKClipOperation.Intersect, true);
        }

        canvas.Save();

        var gradientRotation = SKMatrix.CreateRotation(-(float)Math.PI / 3F, point3.X, point3.Y);
        canvas.Concat(ref gradientRotation);

        var shader = SKShader.CreateSweepGradient(point3,
                                                   new SKColor[]
                                                   {
                                                       Color.FromHsla(_lastHue, 1, 0.5).ToSKColor(),
                                                       Colors.White.ToSKColor(),
                                                       Color.FromHsla(_lastHue, 1, 0.5).ToSKColor()
                                                   },
                                                   new float[]
                                                   {
                                                       0F, 0.16666666666666F, 1F
                                                   });

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };

        canvas.DrawCircle(point3, SvRadius(canvasRadius) * 2, paint);

        canvas.Restore();

        var colors = new SKColor[]
        {
            SKColors.Black,
            SKColors.Transparent
        };

        PaintGradient(canvas, canvasRadius, colors, point3);

        canvas.Restore();
    }

    void PaintGradient(SKCanvas canvas, float canvasRadius, SKColor[] colors, SKPoint centerGradient)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);
        var polar = ToPolar(new SKPoint(center.X - centerGradient.X, center.Y - centerGradient.Y));

        polar.Radius *= _triangleHeight;

        var p2 = FromPolar(polar);
        p2.X += centerGradient.X;
        p2.Y += centerGradient.Y;

        var shader = SKShader.CreateLinearGradient(centerGradient, p2, colors, null, SKShaderTileMode.Clamp);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader      = shader,
            Style       = SKPaintStyle.Fill
        };

        canvas.DrawCircle(center, SvRadius(canvasRadius), paint);
    }

    SKPoint LimitToSvTriangle(SKPoint point, float canvasRadius)
    {
        var unit = ToUnit(point, canvasRadius, SvRadius(canvasRadius));
        var fit  = Triangle.FitToActiveArea(unit, default);
        return FromUnit(fit, canvasRadius, SvRadius(canvasRadius));
    }

    // Triangle constants — used by the SV-triangle rendering path (vertices,
    // gradient stretch). The encoding/decoding math has moved to
    // ColorPicker.Core.SaturationValueTriangle which carries its own copies.
    const float _triangleHeight         = 1.5000001F;
    const float _triangleSide           = 0.8660244F;
    const float _triangleVerticalOffset = 0.5000001F;

    void LimitToHRadius(SKPoint point, float canvasRadius)
    {
        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        var pOuter = new PolarPoint(HRadius(canvasRadius) + GetIndicatorRadiusPixels(), polar.Angle);
        var pInner = new PolarPoint(HRadius(canvasRadius) - GetIndicatorRadiusPixels(), polar.Angle);

        _locationH1 = OffsetByCenter(FromPolar(pOuter), canvasRadius);
        _locationH2 = OffsetByCenter(FromPolar(pInner), canvasRadius);
    }

    static SKPoint OffsetByCenter(SKPoint p, float canvasRadius)
        => new(p.X + canvasRadius, p.Y + canvasRadius);

    // Pixel ↔ unit-square coordinate bridge (per active-area radius).
    static UnitPoint ToUnit(SKPoint pixel, float canvasRadius, float activeRadius)
        => new((float)((pixel.X - canvasRadius) / (2.0 * activeRadius) + 0.5),
               (float)((pixel.Y - canvasRadius) / (2.0 * activeRadius) + 0.5));

    static SKPoint FromUnit(UnitPoint unit, float canvasRadius, float activeRadius)
        => new((float)((unit.X - 0.5) * 2.0 * activeRadius + canvasRadius),
               (float)((unit.Y - 0.5) * 2.0 * activeRadius + canvasRadius));

    static PolarPoint ToPolar(SKPoint point)
    {
        var radius = (float)Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        var angle = (float)Math.Atan2(point.Y, point.X);
        return new PolarPoint(radius, angle);
    }

    static SKPoint FromPolar(PolarPoint point)
    {
        var x = (float)(point.Radius * Math.Cos(point.Angle));
        var y = (float)(point.Radius * Math.Sin(point.Angle));
        return new SKPoint(x, y);
    }

    float SvRadius(float canvasRadius) => canvasRadius - (2 * GetIndicatorRadiusPixels()) - 2;
    float HRadius(float canvasRadius) => canvasRadius - GetIndicatorRadiusPixels();

    void PaintLinePicker(SKCanvas canvas)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke
        };

        paint.Color = Colors.Black.ToSKColor();
        paint.StrokeWidth = 4;

        using var pathTriangle = new SKPath();
        pathTriangle.MoveTo(_locationH1);
        pathTriangle.LineTo(_locationH2);

        canvas.DrawPath(pathTriangle, paint);
    }

    public static void ColorToHsv(Color color, out double hue, out double saturation, out double value)
    {
        var rgb = new Rgb { R = Math.Round(color.Red * 255F), G = Math.Round(color.Green * 255F), B = Math.Round(color.Blue * 255F) };
        var hsv = rgb.To<Hsv>();

        hue = color.GetHue();
        saturation = hsv.S;
        value = hsv.V;
    }

    public static Color ColorFromHsv(double hue, double saturation, double value, double a)
    {
        var result = Color.FromHsv((float)hue, (float)saturation, (float)value);
        return new Color(result.Red, result.Green, result.Blue, (float)a);
    }
}
