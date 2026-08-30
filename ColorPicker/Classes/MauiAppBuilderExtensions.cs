namespace ColorPicker.Classes;

using SkiaSharp.Views.Maui.Controls.Hosting;

public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers the SkiaSharp rendering backend that the ColorPicker controls
    /// depend on. Call this in <c>MauiProgram.CreateMauiApp</c> before using any
    /// <see cref="ColorPicker.Controls.ColorWheel"/>, <see cref="ColorPicker.Controls.ColorTriangle"/>
    /// or slider control.
    /// </summary>
    /// <param name="builder">The MAUI app builder to configure.</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    public static MauiAppBuilder UseColorPickersAndSliders(this MauiAppBuilder builder)
    {
        //  Using SkiaSharp
        //
        builder.UseSkiaSharp();

        return builder;
    }
}
