namespace ColorPickerTest.Converters;

public static class ColorConverterExtensions
{
    //  To... functions
    public static string ToHexRgbString( this Color c )     =>  $"#{c.GetByteRed():X2}{c.GetByteGreen():X2}{c.GetByteBlue():X2}";
    public static string ToHexRgbaString( this Color c )    =>  $"#{c.GetByteRed():X2}{c.GetByteGreen():X2}{c.GetByteBlue():X2}{c.GetByteAlpha():X2}";
    public static string ToHexArgbString( this Color c )    =>  $"#{c.GetByteAlpha():X2}{c.GetByteRed():X2}{c.GetByteGreen():X2}{c.GetByteBlue():X2}";
    public static string ToRgbString( this Color c )        =>  $"RGB({c.GetByteRed()},{c.GetByteGreen()},{c.GetByteBlue()})";
    public static string ToRgbaString( this Color c )       =>  $"RGBA({c.GetByteRed()},{c.GetByteGreen()},{c.GetByteBlue()},{c.Alpha})";
    public static string ToCmykString( this Color c )       =>  $"CMYK({c.GetPercentCyan():P0},{c.GetPercentMagenta():P0},{c.GetPercentYellow():P0},{c.GetPercentBlackKey():P0})";
    public static string ToCmykaString( this Color c )      =>  $"CMYKA({c.GetPercentCyan():P0},{c.GetPercentMagenta():P0},{c.GetPercentYellow():P0},{c.GetPercentBlackKey():P0},{c.Alpha})";
    public static string ToHslString( this Color c )        =>  $"HSL({c.GetDegreeHue():0},{c.GetSaturation():P0},{c.GetLuminosity():P0})";
    public static string ToHslaString( this Color c )       =>  $"HSLA({c.GetDegreeHue():0},{c.GetSaturation():P0},{c.GetLuminosity():P0},{c.Alpha})";

    //  With functions from double
    public static Color WithRed( this Color baseColor, double newR ) =>
        newR < 0 || newR > 1
            ? throw new ArgumentOutOfRangeException( nameof( newR ) )
            : Color.FromRgba( newR, baseColor.Green, baseColor.Blue, baseColor.Alpha );

    public static Color WithGreen( this Color baseColor, double newG ) =>
        newG < 0 || newG > 1
            ? throw new ArgumentOutOfRangeException( nameof( newG ) )
            : Color.FromRgba( baseColor.Red, newG, baseColor.Blue, baseColor.Alpha );

    public static Color WithBlue( this Color baseColor, double newB ) =>
        newB < 0 || newB > 1
            ? throw new ArgumentOutOfRangeException( nameof( newB ) )
            : Color.FromRgba( baseColor.Red, baseColor.Green, newB, baseColor.Alpha );

    //  With functions from byte
    public static Color WithRed( this Color baseColor, byte newR )      => Color.FromRgba( (double)newR / 255, baseColor.Green, baseColor.Blue, baseColor.Alpha );
    public static Color WithGreen( this Color baseColor, byte newG )    => Color.FromRgba( baseColor.Red, (double)newG / 255, baseColor.Blue, baseColor.Alpha );
    public static Color WithBlue( this Color baseColor, byte newB )     => Color.FromRgba( baseColor.Red, baseColor.Green, (double)newB / 255, baseColor.Alpha );
    public static Color WithAlpha( this Color baseColor, byte newA )    => Color.FromRgba( baseColor.Red, baseColor.Green, baseColor.Blue, (double)newA / 255 );

    //  With CMY from double
    public static Color WithCyan( this Color baseColor, double newC ) =>
        Color.FromRgba( ( 1 - newC ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - baseColor.GetPercentMagenta() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - baseColor.GetPercentYellow() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       baseColor.Alpha );

    public static Color WithMagenta( this Color baseColor, double newM ) =>
        Color.FromRgba( ( 1 - baseColor.GetPercentCyan() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - newM ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - baseColor.GetPercentYellow() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       baseColor.Alpha );

    public static Color WithYellow( this Color baseColor, double newY ) =>
        Color.FromRgba( ( 1 - baseColor.GetPercentCyan() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - baseColor.GetPercentMagenta() ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       ( 1 - newY ) * ( 1 - baseColor.GetPercentBlackKey() ),
                       baseColor.Alpha );

    public static Color WithBlackKey( this Color baseColor, double newK ) =>
        Color.FromRgba( ( 1 - baseColor.GetPercentCyan() ) * ( 1 - newK ),
                       ( 1 - baseColor.GetPercentMagenta() ) * ( 1 - newK ),
                       ( 1 - baseColor.GetPercentYellow() ) * ( 1 - newK ),
                       baseColor.Alpha );

    //  Get byte functions
    public static byte GetByteRed( this Color c )       => ToByte( c.Red * 255 );
    public static byte GetByteGreen( this Color c )     => ToByte( c.Green * 255 );
    public static byte GetByteBlue( this Color c )      => ToByte( c.Blue * 255 );
    public static byte GetByteAlpha( this Color c )     => ToByte( c.Alpha * 255 );

    //  Get Degree of Hue
    public static double GetDegreeHue( this Color c )   => c.GetHue() * 360;

    //  Get Percent functions
    public static float GetPercentBlackKey( this Color c ) => 1 - Math.Max( Math.Max( c.Red, c.Green ), c.Blue );

    public static float GetPercentCyan( this Color c ) =>
        ( 1 - c.GetPercentBlackKey() == 0 ) ? 0 :
        ( 1 - c.Red - c.GetPercentBlackKey() ) / ( 1 - c.GetPercentBlackKey() );

    public static float GetPercentMagenta( this Color c ) =>
        ( 1 - c.GetPercentBlackKey() == 0 ) ? 0 :
        ( 1 - c.Green - c.GetPercentBlackKey() ) / ( 1 - c.GetPercentBlackKey() );

    public static float GetPercentYellow( this Color c ) =>
        ( 1 - c.GetPercentBlackKey() == 0 ) ? 0 :
        ( 1 - c.Blue - c.GetPercentBlackKey() ) / ( 1 - c.GetPercentBlackKey() );

    //  Invert color
    public static Color ToInverseColor( this Color baseColor ) =>
        Color.FromRgb( 1 - baseColor.Red, 1 - baseColor.Green, 1 - baseColor.Blue );

    //  To black and white or grayscale
    public static Color ToBlackOrWhite( this Color baseColor )          => baseColor.IsDark() ? Colors.Black : Colors.White;
    public static Color ToBlackOrWhiteToForText( this Color baseColor ) =>  baseColor.IsDarkToTheEye() ? Colors.White : Colors.Black;

    public static Color ToGrayScale( this Color baseColor )
    {
        var avg = (baseColor.Red + baseColor.Blue + baseColor.Green) / 3;
        return Color.FromRgb( avg, avg, avg );
    }

    //  Is dark or perceived as dark
    public static bool IsDark( this Color c ) => c.GetByteRed() + c.GetByteGreen() + c.GetByteBlue() <= 127 * 3;

    public static bool IsDarkToTheEye( this Color c ) =>
        ( c.GetByteRed() * 0.299 ) + ( c.GetByteGreen() * 0.587 ) + ( c.GetByteBlue() * 0.114 ) <= 186;

    //  Convert double to byte
    static byte ToByte( double input )
            =>  input < 0 ? (byte)0 
                          : input > 255 ? (byte)255 
                                        : (byte)Math.Round( input );
}
