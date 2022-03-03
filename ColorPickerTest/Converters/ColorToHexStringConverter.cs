namespace ColorPickerTest.Converters;

using static ColorConverterExtensions;

public class ColorToHexStringConverter : IValueConverter
{
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
        if ( value is not Color color )
            throw new InvalidDataException( "Source is not a Color" );

        return (string) color.ToHexRgbaString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
}
