namespace ColorPickerTest.Converters;

using static ColorConverterExtensions;

public class ColorToRGBAStringConverter : IValueConverter
{
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
        if ( value is not Color color )
            throw new InvalidDataException( "Source is not a Color" );

        return (string) color.ToRgbaString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
}
