namespace ColorPicker;

public class SelectedColorChangedEventArgs : EventArgs
{
    public Color OldColor { get; }
    public Color NewColor { get; }

    public SelectedColorChangedEventArgs( Color oldColor, Color newColor )
    {
        OldColor = oldColor;
        NewColor = newColor;
    }
}
