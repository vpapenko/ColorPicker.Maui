namespace ColorPicker.Classes;

using SkiaSharp.Views.Maui.Controls.Hosting;

public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Use this to register SkiaSharp and ColorPicker controls
    /// </summary>
    public static MauiAppBuilder UseColorPickersAndSliders( this MauiAppBuilder builder )
    {   
        //  Using SkiaSharp
        //
        builder.UseSkiaSharp();

        return builder;
    }
}
