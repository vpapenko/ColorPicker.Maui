namespace ColorPicker.Sample;

using CommunityToolkit.Maui;

#if WINDOWS
using WinUIEx;
#endif

using Microsoft.Maui.LifecycleEvents;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder.UseMauiApp<App>()
               .UseMauiCommunityToolkit()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
               });

#if WINDOWS
        builder.ConfigureLifecycleEvents( lifecycle => 
            lifecycle.AddWindows( windows => 
                windows.OnWindowCreated( window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    window.SetWindowSize( 600, 1000 );
                    window.SetIsResizable( false );
                    window.MoveAndResize( window.Bounds.X + 3840, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height );
                    window.SetForegroundWindow();
                    window.SetIsAlwaysOnTop( false );
                } ) ) );
#endif

        return builder.Build();
    }
}
