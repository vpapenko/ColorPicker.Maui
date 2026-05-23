namespace ColorPicker.Behaviors;

public class TouchActionEventArgs : EventArgs
{
    public long Id { get; }
    public Point Location { get; }
    public bool IsInContact { get; }
    public TouchActionType Type { get; }

    public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)
    {
        Id = id;
        Location = location;
        IsInContact = isInContact;
        Type = type;
    }
}
