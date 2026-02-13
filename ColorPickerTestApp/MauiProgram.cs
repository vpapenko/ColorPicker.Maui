namespace ColorPickerTestApp;

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
        //  It registers SkiaSharp which is required for the ColorPicker controls.
        //
        builder.UseColorPickersAndSliders();

        builder.ConfigureFonts( fonts => fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" ) );

        return builder.Build();
    }
}
