namespace ColorPicker.Behaviors;

public class ColorPickerTouchBehavior : Behavior
{
    public delegate void ColorPickerTouchActionEventHandler( object sender, ColorPickerTouchActionEventArgs args );

    public bool Capture { set; get; }
    public event ColorPickerTouchActionEventHandler TouchAction;

    public void OnTouchAction( Element element, ColorPickerTouchActionEventArgs args )
             => TouchAction?.Invoke( element, args );
}
