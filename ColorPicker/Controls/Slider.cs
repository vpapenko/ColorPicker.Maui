namespace ColorPicker.Controls;

public class Slider : SliderBase, Interfaces.ISlider
{
    readonly Func<Color, float>                     _newValue;
    readonly Func<float, Color, Color>              _getNewColor;
    readonly Func<Color, SKPoint, SKPoint, SKPaint> _getPaint;

    public Slider( Func<Color, float>                       newValue, 
                   Func<float, Color, Color>                getNewColor, 
                   Func<Color, SKPoint, SKPoint, SKPaint>   getPaint )
    {
        _newValue      = newValue;
        _getNewColor   = getNewColor;
        _getPaint      = getPaint;
    }

    public override Color   GetNewColor( float newValue, Color oldColor )                   => _getNewColor( newValue, oldColor );
    public override SKPaint GetPaint( Color color, SKPoint startPoint, SKPoint endPoint )   => _getPaint( color, startPoint, endPoint );
    public override float   NewValue( Color color )                                         => _newValue( color );
}
