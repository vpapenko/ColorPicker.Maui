namespace ColorPicker.Classes;

public class ColorChangedEventArgs : EventArgs
{
    public Color OldColor { get; }
    public Color NewColor { get; }

    public ColorChangedEventArgs( Color oldColor, Color newColor )
    {
        OldColor = oldColor;
        NewColor = newColor;
    }
}