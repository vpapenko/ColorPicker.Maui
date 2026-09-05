namespace ColorPicker.Rendering;

/// <summary>
/// The original ColorPicker.Maui appearance, implemented as a reusable and
/// extensible renderer. Override individual drawing methods for selective
/// customization.
/// </summary>
public class ClassicColorPickerRenderer : ColorPickerRenderer
{
    const float TriangleHeight = 1.5000001F;

    public static readonly BindableProperty IsAntialiasProperty =
        BindableProperty.Create(nameof(IsAntialias), typeof(bool), typeof(ClassicColorPickerRenderer), true);

    public static readonly BindableProperty IndicatorOuterColorProperty =
        BindableProperty.Create(nameof(IndicatorOuterColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.Black);

    public static readonly BindableProperty IndicatorFillColorProperty =
        BindableProperty.Create(nameof(IndicatorFillColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.Transparent);

    public static readonly BindableProperty IndicatorOuterThicknessProperty =
        BindableProperty.Create(
            nameof(IndicatorOuterThickness),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            1F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty IndicatorHighlightColorProperty =
        BindableProperty.Create(nameof(IndicatorHighlightColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.White);

    public static readonly BindableProperty IndicatorHighlightThicknessProperty =
        BindableProperty.Create(
            nameof(IndicatorHighlightThickness),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            2F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty IndicatorHighlightInsetProperty =
        BindableProperty.Create(
            nameof(IndicatorHighlightInset),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            2F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty IndicatorInnerColorProperty =
        BindableProperty.Create(nameof(IndicatorInnerColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.Black);

    public static readonly BindableProperty IndicatorInnerThicknessProperty =
        BindableProperty.Create(
            nameof(IndicatorInnerThickness),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            1F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty IndicatorInnerInsetProperty =
        BindableProperty.Create(
            nameof(IndicatorInnerInset),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            4F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty SliderTrackThicknessScaleProperty =
        BindableProperty.Create(
            nameof(SliderTrackThicknessScale),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            1.3F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty SliderStrokeCapProperty =
        BindableProperty.Create(nameof(SliderStrokeCap), typeof(SKStrokeCap), typeof(ClassicColorPickerRenderer), SKStrokeCap.Round);

    public static readonly BindableProperty SliderStrokeJoinProperty =
        BindableProperty.Create(nameof(SliderStrokeJoin), typeof(SKStrokeJoin), typeof(ClassicColorPickerRenderer), SKStrokeJoin.Round);

    public static readonly BindableProperty AlphaPatternLightColorProperty =
        BindableProperty.Create(nameof(AlphaPatternLightColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.LightGray);

    public static readonly BindableProperty AlphaPatternDarkColorProperty =
        BindableProperty.Create(nameof(AlphaPatternDarkColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.Transparent);

    public static readonly BindableProperty AlphaPatternCellSizeScaleProperty =
        BindableProperty.Create(
            nameof(AlphaPatternCellSizeScale),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            1F / 3F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty HueRingThicknessScaleProperty =
        BindableProperty.Create(
            nameof(HueRingThicknessScale),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            2F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty LuminosityRingThicknessScaleProperty =
        BindableProperty.Create(
            nameof(LuminosityRingThicknessScale),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            1F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty TriangleHueIndicatorColorProperty =
        BindableProperty.Create(nameof(TriangleHueIndicatorColor), typeof(Color), typeof(ClassicColorPickerRenderer), Colors.Black);

    public static readonly BindableProperty TriangleHueIndicatorThicknessProperty =
        BindableProperty.Create(
            nameof(TriangleHueIndicatorThickness),
            typeof(float),
            typeof(ClassicColorPickerRenderer),
            4F,
            validateValue: IsNonNegativeFloat);

    public static readonly BindableProperty TriangleHueIndicatorStrokeCapProperty =
        BindableProperty.Create(
            nameof(TriangleHueIndicatorStrokeCap),
            typeof(SKStrokeCap),
            typeof(ClassicColorPickerRenderer),
            SKStrokeCap.Butt);

    /// <summary>Whether classic drawing uses antialiasing. Default <c>true</c>.</summary>
    public bool IsAntialias
    {
        get => (bool)GetValue(IsAntialiasProperty);
        set => SetValue(IsAntialiasProperty, value);
    }

    /// <summary>Color of the indicator's outer outline. Default black.</summary>
    public Color IndicatorOuterColor
    {
        get => (Color)GetValue(IndicatorOuterColorProperty);
        set => SetValue(IndicatorOuterColorProperty, value);
    }

    /// <summary>Indicator fill color. Default transparent.</summary>
    public Color IndicatorFillColor
    {
        get => (Color)GetValue(IndicatorFillColorProperty);
        set => SetValue(IndicatorFillColorProperty, value);
    }

    /// <summary>Thickness of the indicator's outer outline in pixels. Default <c>1</c>.</summary>
    public float IndicatorOuterThickness
    {
        get => (float)GetValue(IndicatorOuterThicknessProperty);
        set => SetValue(IndicatorOuterThicknessProperty, value);
    }

    /// <summary>Color of the indicator's highlighted middle outline. Default white.</summary>
    public Color IndicatorHighlightColor
    {
        get => (Color)GetValue(IndicatorHighlightColorProperty);
        set => SetValue(IndicatorHighlightColorProperty, value);
    }

    /// <summary>Thickness of the highlighted middle outline in pixels. Default <c>2</c>.</summary>
    public float IndicatorHighlightThickness
    {
        get => (float)GetValue(IndicatorHighlightThicknessProperty);
        set => SetValue(IndicatorHighlightThicknessProperty, value);
    }

    /// <summary>Inset of the highlighted outline from the indicator radius. Default <c>2</c>.</summary>
    public float IndicatorHighlightInset
    {
        get => (float)GetValue(IndicatorHighlightInsetProperty);
        set => SetValue(IndicatorHighlightInsetProperty, value);
    }

    /// <summary>Color of the indicator's inner outline. Default black.</summary>
    public Color IndicatorInnerColor
    {
        get => (Color)GetValue(IndicatorInnerColorProperty);
        set => SetValue(IndicatorInnerColorProperty, value);
    }

    /// <summary>Thickness of the indicator's inner outline in pixels. Default <c>1</c>.</summary>
    public float IndicatorInnerThickness
    {
        get => (float)GetValue(IndicatorInnerThicknessProperty);
        set => SetValue(IndicatorInnerThicknessProperty, value);
    }

    /// <summary>Inset of the inner outline from the indicator radius. Default <c>4</c>.</summary>
    public float IndicatorInnerInset
    {
        get => (float)GetValue(IndicatorInnerInsetProperty);
        set => SetValue(IndicatorInnerInsetProperty, value);
    }

    /// <summary>Slider-track thickness divided by indicator radius. Default <c>1.3</c>.</summary>
    public float SliderTrackThicknessScale
    {
        get => (float)GetValue(SliderTrackThicknessScaleProperty);
        set => SetValue(SliderTrackThicknessScaleProperty, value);
    }

    /// <summary>Stroke cap used for slider tracks. Default round.</summary>
    public SKStrokeCap SliderStrokeCap
    {
        get => (SKStrokeCap)GetValue(SliderStrokeCapProperty);
        set => SetValue(SliderStrokeCapProperty, value);
    }

    /// <summary>Stroke join used for slider tracks. Default round.</summary>
    public SKStrokeJoin SliderStrokeJoin
    {
        get => (SKStrokeJoin)GetValue(SliderStrokeJoinProperty);
        set => SetValue(SliderStrokeJoinProperty, value);
    }

    /// <summary>Light color in the alpha-slider transparency pattern.</summary>
    public Color AlphaPatternLightColor
    {
        get => (Color)GetValue(AlphaPatternLightColorProperty);
        set => SetValue(AlphaPatternLightColorProperty, value);
    }

    /// <summary>Dark color in the alpha-slider transparency pattern. Default transparent.</summary>
    public Color AlphaPatternDarkColor
    {
        get => (Color)GetValue(AlphaPatternDarkColorProperty);
        set => SetValue(AlphaPatternDarkColorProperty, value);
    }

    /// <summary>Transparency-pattern cell size divided by indicator radius.</summary>
    public float AlphaPatternCellSizeScale
    {
        get => (float)GetValue(AlphaPatternCellSizeScaleProperty);
        set => SetValue(AlphaPatternCellSizeScaleProperty, value);
    }

    /// <summary>Triangle hue-ring thickness divided by indicator radius. Default <c>2</c>.</summary>
    public float HueRingThicknessScale
    {
        get => (float)GetValue(HueRingThicknessScaleProperty);
        set => SetValue(HueRingThicknessScaleProperty, value);
    }

    /// <summary>Wheel luminosity-ring thickness divided by indicator radius. Default <c>1</c>.</summary>
    public float LuminosityRingThicknessScale
    {
        get => (float)GetValue(LuminosityRingThicknessScaleProperty);
        set => SetValue(LuminosityRingThicknessScaleProperty, value);
    }

    /// <summary>Color of the rotating triangle's radial hue indicator.</summary>
    public Color TriangleHueIndicatorColor
    {
        get => (Color)GetValue(TriangleHueIndicatorColorProperty);
        set => SetValue(TriangleHueIndicatorColorProperty, value);
    }

    /// <summary>Thickness of the rotating triangle's hue indicator in pixels.</summary>
    public float TriangleHueIndicatorThickness
    {
        get => (float)GetValue(TriangleHueIndicatorThicknessProperty);
        set => SetValue(TriangleHueIndicatorThicknessProperty, value);
    }

    /// <summary>Stroke cap of the rotating triangle's hue indicator.</summary>
    public SKStrokeCap TriangleHueIndicatorStrokeCap
    {
        get => (SKStrokeCap)GetValue(TriangleHueIndicatorStrokeCapProperty);
        set => SetValue(TriangleHueIndicatorStrokeCapProperty, value);
    }

    static bool IsNonNegativeFloat(BindableObject _, object value)
        => value is float number && float.IsFinite(number) && number >= 0;

    public override void Render(SKCanvas canvas, ColorPickerDrawingContext context)
    {
        switch (context)
        {
            case CanvasDrawingContext canvasContext:
                DrawCanvas(canvas, canvasContext);
                break;
            case CircularBackgroundDrawingContext background:
                DrawCircularBackground(canvas, background);
                break;
            case HueSaturationDiscDrawingContext disc:
                DrawHueSaturationDisc(canvas, disc);
                break;
            case LuminosityRingDrawingContext luminosityRing:
                DrawLuminosityRing(canvas, luminosityRing);
                break;
            case HueRingDrawingContext hueRing:
                DrawHueRing(canvas, hueRing);
                break;
            case SaturationValueTriangleDrawingContext triangle:
                DrawSaturationValueTriangle(canvas, triangle);
                break;
            case SliderTransparencyDrawingContext transparency:
                DrawSliderTransparency(canvas, transparency);
                break;
            case SliderTrackDrawingContext slider:
                DrawSliderTrack(canvas, slider);
                break;
            case IndicatorDrawingContext indicator:
                DrawIndicator(canvas, indicator);
                break;
            case HueLineIndicatorDrawingContext lineIndicator:
                DrawHueLineIndicator(canvas, lineIndicator);
                break;
            default:
                DrawUnknown(canvas, context);
                break;
        }
    }

    protected virtual void DrawUnknown(SKCanvas canvas, ColorPickerDrawingContext context)
    {
    }

    protected virtual void DrawCanvas(SKCanvas canvas, CanvasDrawingContext context)
        => canvas.Clear();

    protected virtual void DrawCircularBackground(SKCanvas canvas, CircularBackgroundDrawingContext context)
    {
        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Color = context.BackgroundColor.ToSKColor()
        };
        canvas.DrawCircle(context.Center, context.Radius, paint);
    }

    protected virtual void DrawHueSaturationDisc(SKCanvas canvas, HueSaturationDiscDrawingContext context)
    {
        using var hueShader = SKShader.CreateSweepGradient(
            context.Center,
            context.Gradient.ColorArray,
            context.Gradient.PositionArray);
        using (var huePaint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = hueShader,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(context.Center, context.Radius, huePaint);
        }

        var colors = new[] { SKColors.Gray, SKColors.Transparent };
        using var grayShader = SKShader.CreateRadialGradient(
            context.Center, context.Radius, colors, null, SKShaderTileMode.Clamp);
        using var grayPaint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = grayShader,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawPaint(grayPaint);
    }

    protected virtual void DrawLuminosityRing(SKCanvas canvas, LuminosityRingDrawingContext context)
    {
        var thickness = context.IndicatorRadius * LuminosityRingThicknessScale;
        if (thickness <= 0)
            return;

        var color = context.SelectedColor;
        var colors = new[]
        {
            Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(color.GetHue(), color.GetSaturation(), 1.0).ToSKColor(),
            Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.5).ToSKColor(),
            Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.0).ToSKColor(),
            Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.5).ToSKColor()
        };

        using var shader = SKShader.CreateSweepGradient(context.Center, colors, null);
        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = shader,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        canvas.DrawCircle(context.Center, context.Radius, paint);
    }

    protected virtual void DrawHueRing(SKCanvas canvas, HueRingDrawingContext context)
    {
        var thickness = context.IndicatorRadius * HueRingThicknessScale;
        if (thickness <= 0)
            return;

        using var shader = SKShader.CreateSweepGradient(
            context.Center,
            context.Gradient.ColorArray,
            context.Gradient.PositionArray);
        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = shader,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        canvas.DrawCircle(context.Center, context.Radius, paint);
    }

    protected virtual void DrawSaturationValueTriangle(
        SKCanvas canvas,
        SaturationValueTriangleDrawingContext context)
    {
        canvas.Save();
        var transform = context.Transform;
        canvas.Concat(in transform);

        var point1 = context.LocalHueVertex;
        var point2 = context.LocalWhiteVertex;
        var point3 = context.LocalBlackVertex;

        using (var triangleBuilder = new SKPathBuilder())
        {
            triangleBuilder.MoveTo(point1);
            triangleBuilder.LineTo(point2);
            triangleBuilder.LineTo(point3);
            using var triangle = triangleBuilder.Detach();
            canvas.ClipPath(triangle, SKClipOperation.Intersect, true);
        }

        canvas.Save();
        canvas.RotateRadians(-(float)Math.PI / 3F, point3.X, point3.Y);

        var hueColor = Color.FromHsla(context.Hue, 1, 0.5).ToSKColor();
        var hueColors = new[] { hueColor, Colors.White.ToSKColor(), hueColor };
        var huePositions = new[] { 0F, 0.16666666666666F, 1F };
        using (var hueShader = SKShader.CreateSweepGradient(point3, hueColors, huePositions))
        using (var huePaint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = hueShader,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(point3, context.Radius * 2, huePaint);
        }
        canvas.Restore();

        var polarRadius = Distance(context.Center, point3) * TriangleHeight;
        var angle = MathF.Atan2(context.Center.Y - point3.Y, context.Center.X - point3.X);
        var gradientEnd = new SKPoint(
            point3.X + (polarRadius * MathF.Cos(angle)),
            point3.Y + (polarRadius * MathF.Sin(angle)));

        var valueColors = new[] { SKColors.Black, SKColors.Transparent };
        using var valueShader = SKShader.CreateLinearGradient(
            point3, gradientEnd, valueColors, null, SKShaderTileMode.Clamp);
        using var valuePaint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Shader = valueShader,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(context.Center, context.Radius, valuePaint);
        canvas.Restore();
    }

    protected virtual void DrawSliderTransparency(
        SKCanvas canvas,
        SliderTransparencyDrawingContext context)
    {
        var scale = context.IndicatorRadius * AlphaPatternCellSizeScale;
        var trackThickness = context.IndicatorRadius * SliderTrackThicknessScale;
        if (scale <= 0 || trackThickness <= 0)
            return;

        using var pathBuilder = new SKPathBuilder();
        pathBuilder.MoveTo(-scale, -scale);
        pathBuilder.LineTo(0, -scale);
        pathBuilder.LineTo(0, 0);
        pathBuilder.LineTo(scale, 0);
        pathBuilder.LineTo(scale, scale);
        pathBuilder.LineTo(0, scale);
        pathBuilder.LineTo(0, 0);
        pathBuilder.LineTo(-scale, 0);
        pathBuilder.LineTo(-scale, -scale);
        using var path = pathBuilder.Detach();

        var halfThickness = trackThickness / 2F;
        var capExtension = SliderStrokeCap == SKStrokeCap.Butt ? 0 : halfThickness;
        var longitudinalExtension = capExtension + context.IndicatorPadding;
        SKRect trackBounds;
        if (context.Vertical)
        {
            trackBounds = new SKRect(
                context.StartPoint.X - halfThickness,
                Math.Min(context.StartPoint.Y, context.EndPoint.Y) - longitudinalExtension,
                context.StartPoint.X + halfThickness,
                Math.Max(context.StartPoint.Y, context.EndPoint.Y) + longitudinalExtension);
        }
        else
        {
            trackBounds = new SKRect(
                Math.Min(context.StartPoint.X, context.EndPoint.X) - longitudinalExtension,
                context.StartPoint.Y - halfThickness,
                Math.Max(context.StartPoint.X, context.EndPoint.X) + longitudinalExtension,
                context.StartPoint.Y + halfThickness);
        }

        var cornerRadius = SliderStrokeCap == SKStrokeCap.Round ? halfThickness : 0;
        var clipRoundRect = new SKRoundRect(trackBounds, cornerRadius, cornerRadius);
        canvas.Save();
        canvas.ClipRoundRect(clipRoundRect);

        if (AlphaPatternDarkColor.Alpha > 0)
        {
            using var backgroundPaint = new SKPaint
            {
                IsAntialias = IsAntialias,
                Color = AlphaPatternDarkColor.ToSKColor()
            };
            canvas.DrawRect(trackBounds, backgroundPaint);
        }

        var matrix = SKMatrix.CreateScale(2 * scale, 2 * scale);
        using var pathEffect = SKPathEffect.Create2DPath(matrix, path);
        using var patternPaint = new SKPaint
        {
            PathEffect = pathEffect,
            Color = AlphaPatternLightColor.ToSKColor(),
            IsAntialias = IsAntialias
        };
        canvas.DrawRect(trackBounds, patternPaint);
        canvas.Restore();
    }

    protected virtual void DrawSliderTrack(SKCanvas canvas, SliderTrackDrawingContext context)
    {
        var thickness = context.IndicatorRadius * SliderTrackThicknessScale;
        if (thickness <= 0)
            return;

        using var shader = SKShader.CreateLinearGradient(
            context.StartPoint,
            context.EndPoint,
            context.Gradient.ColorArray,
            context.Gradient.PositionArray,
            SKShaderTileMode.Clamp);
        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SliderStrokeCap,
            StrokeJoin = SliderStrokeJoin,
            StrokeWidth = thickness,
            Shader = shader
        };
        canvas.DrawLine(context.StartPoint, context.EndPoint, paint);
    }

    protected virtual void DrawIndicator(SKCanvas canvas, IndicatorDrawingContext context)
    {
        if (IndicatorFillColor.Alpha > 0)
        {
            using var fill = new SKPaint
            {
                IsAntialias = IsAntialias,
                Style = SKPaintStyle.Fill,
                Color = IndicatorFillColor.ToSKColor()
            };
            canvas.DrawCircle(context.Center, context.Radius, fill);
        }

        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Style = SKPaintStyle.Stroke,
            Color = IndicatorHighlightColor.ToSKColor(),
            StrokeWidth = IndicatorHighlightThickness
        };

        if (IndicatorHighlightThickness > 0)
        {
            canvas.DrawCircle(
                context.Center,
                Math.Max(0, context.Radius - IndicatorHighlightInset),
                paint);
        }

        paint.Color = IndicatorInnerColor.ToSKColor();
        paint.StrokeWidth = IndicatorInnerThickness;
        if (IndicatorInnerThickness > 0)
        {
            canvas.DrawCircle(
                context.Center,
                Math.Max(0, context.Radius - IndicatorInnerInset),
                paint);
        }

        paint.Color = IndicatorOuterColor.ToSKColor();
        paint.StrokeWidth = IndicatorOuterThickness;
        if (IndicatorOuterThickness > 0)
            canvas.DrawCircle(context.Center, context.Radius, paint);
    }

    protected virtual void DrawHueLineIndicator(SKCanvas canvas, HueLineIndicatorDrawingContext context)
    {
        if (TriangleHueIndicatorThickness <= 0)
            return;

        using var paint = new SKPaint
        {
            IsAntialias = IsAntialias,
            Style = SKPaintStyle.Stroke,
            Color = TriangleHueIndicatorColor.ToSKColor(),
            StrokeWidth = TriangleHueIndicatorThickness,
            StrokeCap = TriangleHueIndicatorStrokeCap
        };
        canvas.DrawLine(context.OuterPoint, context.InnerPoint, paint);
    }

    static float Distance(SKPoint first, SKPoint second)
    {
        var x = first.X - second.X;
        var y = first.Y - second.Y;
        return MathF.Sqrt((x * x) + (y * y));
    }
}
