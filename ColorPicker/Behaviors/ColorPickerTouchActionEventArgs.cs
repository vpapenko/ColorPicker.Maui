namespace ColorPicker.Behaviors;

public class ColorPickerTouchActionEventArgs : EventArgs
{
    public long     Id                      { get; }
    public Point    Location                { get; }
    public bool     IsInContact             { get; }
    public ColorPickerTouchActionType Type  { get; }

    public ColorPickerTouchActionEventArgs( long id, ColorPickerTouchActionType type, Point location, bool isInContact )
    {
        Id          = id;
        Location    = location;
        IsInContact = isInContact;
        Type        = type;
    }
}
