using ColorPicker.Platform.IOS;

using System.Linq;
using UIKit;

[assembly: ResolutionGroupName("ColorPickerTouchEffect")]
[assembly: ExportEffect(typeof(ColorPickerTouchEffect), "TouchEffect")]

using ColorPicker.Platform.IOS
{
    public class ColorPickerTouchEffect : PlatformEffect
    {
        UIView view;
        ColorPickerTouchRecognizeriOS touchRecognizer;

        protected override void OnAttached()
        {
            // Get the iOS UIView corresponding to the Element that the effect is attached to
            view = Control ?? Container;

            // Get access to the TouchEffect class in the .NET Standard library
            ColorPickerTouchRoutingEffect effect = (ColorPickerTouchRoutingEffect)Element.Effects.FirstOrDefault(e => e is ColorPickerTouchRoutingEffect);

            if (effect is not null && view is not null)
            {
                // Create a TouchRecognizer for this UIView
                touchRecognizer = new ColorPickerTouchRecognizeriOS(Element, view, effect);
                view.AddGestureRecognizer(touchRecognizer);
            }
        }

        protected override void OnDetached()
        {
            if (touchRecognizer is not null)
            {
                // Clean up the TouchRecognizer object
                touchRecognizer.Detach();

                // Remove the TouchRecognizer from the UIView
                view.RemoveGestureRecognizer(touchRecognizer);
            }
        }
    }
}
