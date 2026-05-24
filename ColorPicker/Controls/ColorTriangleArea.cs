using ColorPicker.Core;
using ColorPicker.Core.Interaction;

namespace ColorPicker.Controls;

public class ColorTriangleArea : SkiaPickerBase
{
    static readonly HueRing _hueRing = new();

    // Interaction state lives in a pure controller so it can be exercised
    // by deterministic Core unit tests. Two controllers (rotating / fixed)
    // are kept so we don't have to re-sync when RotateTriangleByHue toggles.
    readonly TriangleAreaInteraction _interactionRotated = new(rotateByHue: true);
    readonly TriangleAreaInteraction _interactionFixed   = new(rotateByHue: false);

    TriangleAreaInteraction Interaction
        => RotateTriangleByHue ? _interactionRotated : _interactionFixed;

    long? _locationSvProgressId = null;
    long? _locationHProgressId = null;

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

        var svUnit = PixelToUnit(point, canvasRadius, SvRadius(canvasRadius));
        var hUnit  = PixelToUnit(point, canvasRadius, HRadius(canvasRadius));

        if (_locationSvProgressId is null && Interaction.IsInSv(svUnit))
        {
            _locationSvProgressId = args.Id;
            WriteSelectedColor(Interaction.UpdateFromSv(svUnit));
        }
        else if (_locationHProgressId is null && IsInHueRing(hUnit, canvasRadius))
        {
            _locationHProgressId = args.Id;
            WriteSelectedColor(Interaction.UpdateFromH(hUnit));
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
            var svUnit = PixelToUnit(point, canvasRadius, SvRadius(canvasRadius));
            WriteSelectedColor(Interaction.UpdateFromSv(svUnit));
        }
        else if (_locationHProgressId == args.Id)
        {
            var hUnit = PixelToUnit(point, canvasRadius, HRadius(canvasRadius));
            WriteSelectedColor(Interaction.UpdateFromH(hUnit));
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
            var svUnit = PixelToUnit(point, canvasRadius, SvRadius(canvasRadius));
            WriteSelectedColor(Interaction.UpdateFromSv(svUnit));
        }
        else if (_locationHProgressId == args.Id)
        {
            _locationHProgressId = null;
            var hUnit = PixelToUnit(point, canvasRadius, HRadius(canvasRadius));
            WriteSelectedColor(Interaction.UpdateFromH(hUnit));
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

        // Re-sync from SelectedColor each paint so the controller picks up
        // any external bindable-property change (incl. the initial default
        // that never fires OnSelectedColorChanging).
        Interaction.SyncFromColor(SelectedColor.ToHsla());

        // Compute paint-time pixel positions of indicators from the
        // controller's unit-space locations.
        var locationSv = UnitToPixel(Interaction.LocationSv, canvasRadius, SvRadius(canvasRadius));

        var hLocations = ComputeHueIndicatorPixels(canvasRadius);

        canvas.Clear();

        canvas.Save();
        canvas.Translate(offX, offY);

        PaintBackground(canvas, canvasRadius);
        PaintHGradient(canvas, canvasRadius);

        if (RotateTriangleByHue)
            PaintLinePicker(canvas, hLocations.outer, hLocations.inner);
        else
            PaintIndicator(canvas, hLocations.middle);

        PaintSvTriangle(canvas, canvasRadius);
        PaintIndicator(canvas, locationSv);

        canvas.Restore();
    }

    protected override void OnSelectedColorChanging(Color color)
    {
        Interaction.SyncFromColor(color.ToHsla());
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

    SaturationValueTriangle Triangle => RotateTriangleByHue ? SaturationValueTriangleRotated : SaturationValueTriangleFixed;
    static readonly SaturationValueTriangle SaturationValueTriangleRotated = new(rotateByHue: true);
    static readonly SaturationValueTriangle SaturationValueTriangleFixed   = new(rotateByHue: false);

    // Compute the three hue-indicator pixel positions from the controller's
    // unit-space LocationH at paint time.
    (SKPoint outer, SKPoint inner, SKPoint middle) ComputeHueIndicatorPixels(float canvasRadius)
    {
        var hUnit = Interaction.LocationH;
        // Recover the angle from the unit point (the radius from the
        // controller is exactly 0.5 in unit space).
        var centered = new SKPoint(hUnit.X - 0.5f, hUnit.Y - 0.5f);
        var angle = (float)Math.Atan2(centered.Y, centered.X);

        var middle = OffsetByCenter(FromPolar(new PolarPoint(HRadius(canvasRadius),                                  angle)), canvasRadius);
        var outer  = OffsetByCenter(FromPolar(new PolarPoint(HRadius(canvasRadius) + GetIndicatorRadiusPixels(),     angle)), canvasRadius);
        var inner  = OffsetByCenter(FromPolar(new PolarPoint(HRadius(canvasRadius) - GetIndicatorRadiusPixels(),     angle)), canvasRadius);
        return (outer, inner, middle);
    }

    bool IsInHueRing(UnitPoint hUnit, float canvasRadius)
    {
        // MAUI tolerance: ±indicatorPx in pixel space, expressed in unit space.
        var tolUnits = GetIndicatorRadiusPixels() / (2F * HRadius(canvasRadius));
        return Interaction.IsInH(hUnit, tolUnits);
    }

    void WriteSelectedColor(HslaColor hsla)
    {
        SelectedColor = hsla.ToMauiColor();
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
        var lastHue = Interaction.LastHue;
        canvas.Save();

        var rotationHue = SKMatrix.CreateRotation(-(float)((2D * Math.PI * lastHue) + (Math.PI / 2D)),
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
                                                       Color.FromHsla(lastHue, 1, 0.5).ToSKColor(),
                                                       Colors.White.ToSKColor(),
                                                       Color.FromHsla(lastHue, 1, 0.5).ToSKColor()
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

        polar = polar.WithRadius(polar.Radius * _triangleHeight);

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

    // Triangle constants — used by the SV-triangle rendering path (vertices,
    // gradient stretch). The encoding/decoding math has moved to
    // ColorPicker.Core.SaturationValueTriangle which carries its own copies.
    const float _triangleHeight         = 1.5000001F;
    const float _triangleSide           = 0.8660244F;
    const float _triangleVerticalOffset = 0.5000001F;

    static SKPoint OffsetByCenter(SKPoint p, float canvasRadius)
        => new(p.X + canvasRadius, p.Y + canvasRadius);

    // Pixel ↔ unit-square coordinate bridge (per active-area radius).
    static UnitPoint PixelToUnit(SKPoint pixel, float canvasRadius, float activeRadius)
        => new((float)((pixel.X - canvasRadius) / (2.0 * activeRadius) + 0.5),
               (float)((pixel.Y - canvasRadius) / (2.0 * activeRadius) + 0.5));

    static SKPoint UnitToPixel(UnitPoint unit, float canvasRadius, float activeRadius)
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

    void PaintLinePicker(SKCanvas canvas, SKPoint outer, SKPoint inner)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke
        };

        paint.Color = Colors.Black.ToSKColor();
        paint.StrokeWidth = 4;

        using var pathTriangle = new SKPath();
        pathTriangle.MoveTo(outer);
        pathTriangle.LineTo(inner);

        canvas.DrawPath(pathTriangle, paint);
    }
}
