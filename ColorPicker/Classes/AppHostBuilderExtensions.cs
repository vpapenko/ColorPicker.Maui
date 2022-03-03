namespace ColorPicker.Classes;

using ColorPicker.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;

public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Use this to use the SkiaSharp compatibility renderers
    /// </summary>
    public static MauiAppBuilder UseCompatibilityColorPickersAndSliders( this MauiAppBuilder builder, bool alreadyUsingSkiaSharp = false )
    {   
        //  Using SkiaSharp compatibility renderers
        //
        if ( ! alreadyUsingSkiaSharp )
            builder.UseSkiaSharp( true );

        //  Using ColorPickers and Sliders in compatibility mode
        //
        builder.ConfigureMauiHandlers( handlers =>
        {
            handlers.TryAddCompatibilityRenderer( typeof( ColorCircle ),      typeof( ColorCircle ) );
            handlers.TryAddCompatibilityRenderer( typeof( ColorWheel ),       typeof( ColorWheel ) );
            handlers.TryAddCompatibilityRenderer( typeof( ColorTriangle ),    typeof( ColorTriangle ) );
            handlers.TryAddCompatibilityRenderer( typeof( AlphaSlider ),      typeof( AlphaSlider ) );
            handlers.TryAddCompatibilityRenderer( typeof( LuminositySlider ), typeof( LuminositySlider ) );
            handlers.TryAddCompatibilityRenderer( typeof( RGBSliders ),       typeof( RGBSliders ) );
            handlers.TryAddCompatibilityRenderer( typeof( HSLSliders ),       typeof( HSLSliders ) );
        } );

        return builder;
    }
}
