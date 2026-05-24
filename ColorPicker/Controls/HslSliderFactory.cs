using ColorPicker.Core;

namespace ColorPicker.Controls;

public static class HslSliderFactory
{
    static readonly HueSlider             _hue = new();
    static readonly SaturationSlider      _sat = new();
    static readonly Core.LuminositySlider _lum = new();

    public static float NewValueH(Color color) => (float)_hue.Read(color.ToHsla());
    public static float NewValueS(Color color) => (float)_sat.Read(color.ToHsla());
    public static float NewValueL(Color color) => (float)_lum.Read(color.ToHsla());

    public static Color GetNewColorH(float newValue, Color oldColor)
            => _hue.Write(oldColor.ToHsla(), newValue).ToMauiColor();

    public static Color GetNewColorS(float newValue, Color oldColor)
            => _sat.Write(oldColor.ToHsla(), newValue).ToMauiColor();

    public static Color GetNewColorL(float newValue, Color oldColor)
            => _lum.Write(oldColor.ToHsla(), newValue).ToMauiColor();

    public static SKPaint GetPaintH(Color _, SKPoint startPoint, SKPoint endPoint)
    {
        var colors = new List<SKColor>();

        for (var i = 0; i <= 255; i++)
        {
            colors.Add(Color.FromHsla(i / 255D, 1.0, 0.5).ToSKColor());
        }

        var colorPos = new List<float>();

        for (var i = 0; i <= 255; i++)
        {
            colorPos.Add(i / 255F);
        }

        return GetPaint(colors.ToArray(), colorPos.ToArray(), startPoint, endPoint);
    }

    public static SKPaint GetPaintS(Color color, SKPoint startPoint, SKPoint endPoint)
    {
        var colors = new SKColor[]
            {
                Color.FromHsla(color.GetHue(), 0.0, color.GetLuminosity()).ToSKColor(),
                Color.FromHsla(color.GetHue(), 1.0, color.GetLuminosity()).ToSKColor()
            };

        var colorPos = new float[] { 0F, 1F };
        return GetPaint(colors, colorPos, startPoint, endPoint);
    }

    public static SKPaint GetPaintL(Color color, SKPoint startPoint, SKPoint endPoint)
    {
        var colors = new SKColor[]
            {
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.0).ToSKColor(),
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.5).ToSKColor(),
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 1.0).ToSKColor()
            };

        var colorPos = new float[] { 0F, 0.5F, 1F };
        return GetPaint(colors, colorPos, startPoint, endPoint);
    }

    public static SKPaint GetPaint(SKColor[] colors, float[] colorPos, SKPoint startPoint, SKPoint endPoint)
    {
        var shader = SKShader.CreateLinearGradient(startPoint, endPoint,
                                                        colors, colorPos, SKShaderTileMode.Clamp);
        var paint = new SKPaint()
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke,
            StrokeCap   = SKStrokeCap.Round,
            StrokeJoin  = SKStrokeJoin.Round,
            Shader      = shader
        };
        return paint;
    }
}
