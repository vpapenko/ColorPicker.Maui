namespace ConsumerSmoke;

using ColorPicker.Classes;

/// <summary>
/// Compile-only validation of the public MauiAppBuilder extension that
/// consumers are expected to call from their <c>MauiProgram.cs</c>.
/// If the package ships without this extension (e.g. namespace renamed or
/// method made internal), the smoke build fails.
/// </summary>
public static class SmokeHostBuilder
{
    public static MauiAppBuilder Wire(MauiAppBuilder builder)
        => builder.UseColorPickersAndSliders();
}
