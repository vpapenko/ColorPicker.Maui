namespace ColorPickerTest;

using ColorPicker.Classes;

#if WINDOWS
using ColorPicker.Platforms.WinUI;
#elif ANDROID
using ColorPicker.Platforms.Droid;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

        //  You'll find this in ColorPicker.Classes.AppHostBuilderExtension.cs.
        //  It eliminates the need for the user to mess renderer/handler registration.
        //  It also registers SkiaSharp by default. If the user has already registered
        //  SkiaSharp for his own code use this instead:
        //  
        // builder.UseCompatibilityColorPickersAndSliders( alreadyUsingSkiaSharp: true );
        //
        builder.UseCompatibilityColorPickersAndSliders();

        builder.ConfigureFonts( fonts => fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" ) );

        return builder.Build();
    }
}
