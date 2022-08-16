namespace ColorPicker;

public sealed class OrientationConverter : TypeConverter
{
    public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType )
            => sourceType == typeof( string );

    public override bool CanConvertTo( ITypeDescriptorContext? context, Type? destinationType )
            => destinationType == typeof( string );

    public override object ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object? value )
    {
        var strValue = value?.ToString();

        if ( string.IsNullOrEmpty( strValue ) )
            return Orientation.Horizontal;

        strValue = strValue.Trim();

        if ( Enum.TryParse( strValue, true, out Orientation result ) )
            return result;

        throw new InvalidOperationException( string.Format( "Cannot convert \"{0}\" into {1}", strValue, typeof( Orientation ) ) );
    }

    public override object ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
    {
        if ( value is Orientation result )
        {
            if ( result == Orientation.Horizontal )
                return nameof( Orientation.Horizontal );

            if ( result == Orientation.Vertical )
                return nameof( Orientation.Vertical );
        }

        throw new NotSupportedException();
    }
}
