namespace ColorPicker.Controls;

public static class SliderFunctionsAlpha
{
    public static float NewValueAlpha( Color color ) => (float)color.Alpha;

    public static Color GetNewColorAlpha( float newValue, Color oldColor )
        => Color.FromRgba( oldColor.Red, oldColor.Green, oldColor.Blue, newValue );

    public static SKPaint GetPaintAlpha( Color color, SKPoint startPoint, SKPoint endPoint )
    {
        var startColor = Color.FromRgba(color.Red, color.Green, color.Blue, 0).ToSKColor();
        var endColor = Color.FromRgba(color.Red, color.Green, color.Blue, 1).ToSKColor();
        return GetPaint( startColor, endColor, startPoint, endPoint );
    }

    public static SKPaint GetPaint( SKColor startColor, SKColor endColor, SKPoint startPoint, SKPoint endPoint )
    {
        var paint = new SKPaint()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            Shader = SKShader.CreateLinearGradient(startPoint, endPoint
                    , new SKColor[] { startColor, endColor }, new float[] { 0, 1 }, SKShaderTileMode.Clamp)
        };
        return paint;
    }
}
