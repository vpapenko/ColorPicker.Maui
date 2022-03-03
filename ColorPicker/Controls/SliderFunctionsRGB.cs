namespace ColorPicker.Controls;

public static class SliderFunctionsRGB
{
    public static float NewValueR( Color color ) => color.Red;
    public static float NewValueG( Color color ) => color.Green;
    public static float NewValueB( Color color ) => color.Blue;

    public static Color GetNewColorR( float newValue, Color oldColor ) 
            => Color.FromRgba( newValue, oldColor.Green, oldColor.Blue, oldColor.Alpha );

    public static Color GetNewColorG( float newValue, Color oldColor ) 
            => Color.FromRgba( oldColor.Red, newValue, oldColor.Blue, oldColor.Alpha );

    public static Color GetNewColorB( float newValue, Color oldColor ) 
            => Color.FromRgba( oldColor.Red, oldColor.Green, newValue, oldColor.Alpha );

    public static SKPaint GetPaintR( Color color, SKPoint startPoint, SKPoint endPoint )
    {
        var startColor = new Color(0, color.Green, color.Blue).ToSKColor();
        var endColor = new Color(1, color.Green, color.Blue).ToSKColor();
        return GetPaint( startColor, endColor, startPoint, endPoint );
    }

    public static SKPaint GetPaintG( Color color, SKPoint startPoint, SKPoint endPoint )
    {
        var startColor = new Color(color.Red, 0, color.Blue).ToSKColor();
        var endColor = new Color(color.Red, 1, color.Blue).ToSKColor();
        return GetPaint( startColor, endColor, startPoint, endPoint );
    }

    public static SKPaint GetPaintB( Color color, SKPoint startPoint, SKPoint endPoint )
    {
        var startColor = new Color(color.Red, color.Green, 0).ToSKColor();
        var endColor = new Color(color.Red, color.Green, 1).ToSKColor();
        return GetPaint( startColor, endColor, startPoint, endPoint );
    }

    public static SKPaint GetPaint( SKColor startColor, SKColor endColor, SKPoint startPoint, SKPoint endPoint )
    {
        var paint = new SKPaint()
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke,
            StrokeCap   = SKStrokeCap.Round,
            StrokeJoin  = SKStrokeJoin.Round,
            Shader      = SKShader.CreateLinearGradient(startPoint, endPoint, new SKColor[] { startColor, endColor }, 
                                                        new float[] { 0, 1 }, SKShaderTileMode.Clamp)
        };
        return paint;
    }
}
