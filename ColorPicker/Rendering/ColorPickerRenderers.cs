namespace ColorPicker.Rendering;

/// <summary>Built-in color-picker renderers.</summary>
public static class ColorPickerRenderers
{
    /// <summary>Creates a renderer reproducing the library's original visual appearance.</summary>
    public static ClassicColorPickerRenderer CreateClassic() => new();
}
