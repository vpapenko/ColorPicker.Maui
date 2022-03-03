namespace ColorPickerTest.Converters;

using System.Globalization;

public class BoolToSwitchConverter : IValueConverter
{
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => (bool)value ? "Show Wheel" : "Show Triangle";

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
            => (string)value == "Show Wheel";
}
