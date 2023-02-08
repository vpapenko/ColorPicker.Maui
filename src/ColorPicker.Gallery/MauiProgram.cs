﻿namespace ColorPicker.Gallery;

#if WINDOWS
using WinUIEx;
#endif

using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts( fonts =>
            {
                fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" );
                fonts.AddFont( "OpenSans-Semibold.ttf", "OpenSansSemibold" );
            } );

#if WINDOWS
        builder.ConfigureLifecycleEvents( lifecycle => 
            lifecycle.AddWindows( windows => 
                windows.OnWindowCreated( window =>
                {
                    window.ExtendsContentIntoTitleBar = true;
                    window.SetWindowSize( 1200, 1200 );
                    window.SetIsResizable( true );
                    window.MoveAndResize( window.Bounds.X + 3840, window.Bounds.Y, window.Bounds.Width, window.Bounds.Height );
                    window.SetForegroundWindow();
                    window.SetIsAlwaysOnTop( true );
                } ) ) );
#endif

        return builder.Build();
    }
}
