namespace ColorPicker.Interfaces;

public interface ISlider
{
    public bool     PaintChessPattern { get; set; }
 
    public float    NewValue( Color color );
    public Color    GetNewColor( float newValue, Color oldColor );
    public SKPaint  GetPaint(Color color, SKPoint startPoint, SKPoint endPoint);
}
