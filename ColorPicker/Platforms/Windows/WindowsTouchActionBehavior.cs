namespace ColorPicker.Platforms.WinUI;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

public class WindowsTouchActionBehavior : Behavior<SkiaPickerBase>
{
    Action<Element, TouchActionEventArgs>    _onTouchAction;
    FrameworkElement                                    _frameworkElement;
    TouchBehavior                            _sharedBehavior;
    Element                                             _boundElement;

    public WindowsTouchActionBehavior( TouchBehavior sharedBehavior )
    {
        ArgumentNullException.ThrowIfNull( sharedBehavior );

        _sharedBehavior = sharedBehavior;
    }

    protected override void OnAttachedTo( BindableObject sender )
    {
        if ( sender is not SkiaPickerBase bindable )
            return;

        bindable.HandlerChanged     += OnHandlerChanged;
        base.OnAttachedTo( bindable );
    }

    void OnHandlerChanged( object sender, EventArgs e )
    {
        if ( sender is not SkiaPickerBase bindable )
            return;

        // Get the Windows FrameworkElement corresponding to the Element that the Behavior is attached to
        _boundElement       =   bindable;
        var context         =   bindable.Handler.MauiContext ?? bindable.Parent.Handler.MauiContext;
        _frameworkElement   =   bindable.ToPlatform( context );

        if ( _sharedBehavior is not null && _frameworkElement is not null )
        {
            // Save the method to call on touch events
            _onTouchAction = _sharedBehavior.OnTouchAction;

            // Set event handlers on FrameworkElement
            _frameworkElement.PointerEntered    += OnPointerEntered;
            _frameworkElement.PointerPressed    += OnPointerPressed;
            _frameworkElement.PointerMoved      += OnPointerMoved;
            _frameworkElement.PointerReleased   += OnPointerReleased;
            _frameworkElement.PointerExited     += OnPointerExited;
            _frameworkElement.PointerCanceled   += OnPointerCancelled;
        }
    }

    protected override void OnDetachingFrom( SkiaPickerBase bindable )
    {
        bindable.HandlerChanged -= OnHandlerChanged;

        if ( _onTouchAction is not null )
        {
            // Release event handlers on FrameworkElement
            _frameworkElement.PointerEntered    -= OnPointerEntered;
            _frameworkElement.PointerPressed    -= OnPointerPressed;
            _frameworkElement.PointerMoved      -= OnPointerMoved;
            _frameworkElement.PointerReleased   -= OnPointerReleased;
            _frameworkElement.PointerExited     -= OnPointerEntered;
            _frameworkElement.PointerCanceled   -= OnPointerCancelled;
        }

        base.OnDetachingFrom( bindable );
    }

    void OnPointerEntered( object sender, PointerRoutedEventArgs args )
            => CommonHandler( sender, TouchActionType.Entered, args );

    void OnPointerMoved( object sender, PointerRoutedEventArgs args )
            => CommonHandler( sender, TouchActionType.Moved, args );

    void OnPointerReleased( object sender, PointerRoutedEventArgs args )
            => CommonHandler( sender, TouchActionType.Released, args );

    void OnPointerExited( object sender, PointerRoutedEventArgs args )
            => CommonHandler( sender, TouchActionType.Exited, args );

    void OnPointerCancelled( object sender, PointerRoutedEventArgs args )
            => CommonHandler( sender, TouchActionType.Cancelled, args );

    void OnPointerPressed( object sender, PointerRoutedEventArgs args )
    {
        CommonHandler( sender, TouchActionType.Pressed, args );

        // Check setting of Capture property
        if ( _sharedBehavior.Capture )
            ( sender as FrameworkElement ).CapturePointer( args.Pointer );
    }

    void CommonHandler( object sender, TouchActionType touchActionType, PointerRoutedEventArgs args )
    {
        var pointerPoint                    = args.GetCurrentPoint( sender as UIElement );
        Windows.Foundation.Point winPoint   = pointerPoint.Position;

        _onTouchAction( _boundElement,
                        new TouchActionEventArgs( args.Pointer.PointerId,
                                                             touchActionType,
                                                             new Point( winPoint.X, winPoint.Y ),
                                                             args.Pointer.IsInContact ) );
    }
}
