namespace ColorPicker.Rendering;

/// <summary>Identifies the semantic purpose of an indicator.</summary>
public enum IndicatorRole
{
    HueSaturation,
    Luminosity,
    SaturationValue,
    Hue,
    Slider
}

/// <summary>Identifies the color channel represented by a slider.</summary>
public enum SliderChannel
{
    Custom,
    Hue,
    Saturation,
    Luminosity,
    Red,
    Green,
    Blue,
    Alpha
}

/// <summary>Identifies the circular area whose background is being rendered.</summary>
public enum CircularBackgroundRole
{
    ColorDisc,
    ColorTriangle
}

/// <summary>
/// Base context supplied to a renderer. Coordinates are physical Skia canvas pixels.
/// </summary>
public abstract record ColorPickerDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor);

/// <summary>Starts a new picker frame. The classic renderer clears the canvas.</summary>
public sealed record CanvasDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the background behind a circular picker element.</summary>
public sealed record CircularBackgroundDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    Color BackgroundColor,
    CircularBackgroundRole Role)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the hue/saturation surface of a color disc.</summary>
public sealed record HueSaturationDiscDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    ColorGradient Gradient)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the luminosity ring surrounding a color disc.</summary>
public sealed record LuminosityRingDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    float IndicatorRadius)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the hue ring surrounding a saturation/value triangle.</summary>
public sealed record HueRingDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    float IndicatorRadius,
    ColorGradient Gradient)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>
/// Describes the saturation/value triangle, including its final vertices and rotation.
/// </summary>
public sealed record SaturationValueTriangleDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    double Hue,
    float RotationRadians,
    bool RotatesWithHue,
    SKMatrix Transform,
    SKPoint LocalHueVertex,
    SKPoint LocalWhiteVertex,
    SKPoint LocalBlackVertex,
    SKPoint HueVertex,
    SKPoint WhiteVertex,
    SKPoint BlackVertex)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the transparency pattern drawn beneath an alpha slider.</summary>
public sealed record SliderTransparencyDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    bool Vertical,
    SKPoint StartPoint,
    SKPoint EndPoint,
    float IndicatorRadius,
    float IndicatorPadding,
    SliderChannel Channel)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes a slider track and its semantic gradient.</summary>
public sealed record SliderTrackDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint StartPoint,
    SKPoint EndPoint,
    float IndicatorRadius,
    float Value,
    bool Vertical,
    SliderChannel Channel,
    ColorGradient Gradient)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes a point indicator.</summary>
public sealed record IndicatorDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint Center,
    float Radius,
    IndicatorRole Role,
    SliderChannel Channel = SliderChannel.Custom,
    SKPoint NormalizedPosition = default,
    float? AngleRadians = null,
    bool IsActive = false)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);

/// <summary>Describes the radial line indicator used by the rotating triangle hue ring.</summary>
public sealed record HueLineIndicatorDrawingContext(
    SKSize CanvasSize,
    Color SelectedColor,
    SKPoint OuterPoint,
    SKPoint InnerPoint,
    float AngleRadians,
    bool IsActive)
    : ColorPickerDrawingContext(CanvasSize, SelectedColor);
