namespace ColorPickerMath;

public static class FloatExtensions
{
    public static float Clamp( this float self, float min, float max ) 
            => max < min ? max
                         : self < min ? min
                                      : self > max ? max
                                                   : self;
}
