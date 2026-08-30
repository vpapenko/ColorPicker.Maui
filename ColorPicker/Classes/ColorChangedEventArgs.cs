namespace ColorPicker.Classes;

/// <summary>
/// Event data for <see cref="ColorPicker.BaseClasses.ColorPickerBase.SelectedColorChanged"/>,
/// carrying the color before and after the change.
/// </summary>
public class ColorChangedEventArgs : EventArgs
{
    /// <summary>The color before the change.</summary>
    public Color OldColor { get; }

    /// <summary>The color after the change.</summary>
    public Color NewColor { get; }

    /// <summary>Creates a new <see cref="ColorChangedEventArgs"/>.</summary>
    public ColorChangedEventArgs(Color oldColor, Color newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }
}
