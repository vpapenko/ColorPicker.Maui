namespace ColorPicker.Behaviors;

public class TouchBehavior : Behavior
{
    public delegate void ColorPickerTouchActionEventHandler(object sender, TouchActionEventArgs args);

    public bool Capture { set; get; }
    public event ColorPickerTouchActionEventHandler TouchAction;

    public void OnTouchAction(Element element, TouchActionEventArgs args)
             => TouchAction?.Invoke(element, args);
}
