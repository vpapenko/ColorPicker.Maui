namespace ColorPicker.Platforms.Droid;

using Android.Views;

public class ColorPickerTouchActionBehaviorDroid : Behavior<SkiaSharpPickerBase>
{
    Android.Views.View              _nativeView;
    Element                         _formsElement;
    ColorPickerTouchBehavior        _commonBehavior;
    bool                            _capture;
    Func<double, double>            _fromPixels;
    readonly int[]                  _screenLocationArray = new int[2];

    static readonly Dictionary<Android.Views.View, ColorPickerTouchActionBehaviorDroid> _viewDictionary       = new();
    static readonly Dictionary<int, ColorPickerTouchActionBehaviorDroid>                _idToEffectDictionary = new();

    public ColorPickerTouchActionBehaviorDroid( ColorPickerTouchBehavior sharedBehavior )
    {
        ArgumentNullException.ThrowIfNull( sharedBehavior );

        _commonBehavior = sharedBehavior;
    }

    protected override void OnAttachedTo( SkiaSharpPickerBase bindable )
    {
        bindable.HandlerChanged += OnHandlerChangedAction;
        base.OnAttachedTo( bindable );
    }

    void OnHandlerChangedAction( object sender, EventArgs e )
    {
        if ( sender is not SkiaSharpPickerBase bindable )
            return;

        var mauiContext =   bindable.Handler.MauiContext ?? bindable.Parent.Handler.MauiContext;

        // Get the Android View corresponding to the Element that the effect is attached to
        _nativeView =   bindable.ToNative( mauiContext );

        if ( _commonBehavior is null || _nativeView is null )
            return;

        _viewDictionary.Add( _nativeView, this );
        _formsElement = bindable;

        // Save fromPixels function
        _fromPixels     = _nativeView.Context.FromPixels;

        // Set event handler on View
        _nativeView.Touch += OnTouch;
    }

    protected override void OnDetachingFrom( SkiaSharpPickerBase bindable )
    {
        if ( _viewDictionary.ContainsKey( _nativeView ) )
        {
            _viewDictionary.Remove( _nativeView );
            _nativeView.Touch -= OnTouch;
        }

        base.OnDetachingFrom( bindable );
    }

    void OnTouch( object sender, Android.Views.View.TouchEventArgs args )
    {
        // Two object common to all the events
        var senderView = sender as Android.Views.View;
        var motionEvent = args.Event;

        // Get the pointer index
        var pointerIndex = motionEvent.ActionIndex;

        // Get the id that identifies a finger over the course of its progress
        var id = motionEvent.GetPointerId(pointerIndex);

        senderView.GetLocationOnScreen( _screenLocationArray );

        Point screenPointerCoords = new(_screenLocationArray[0] + motionEvent.GetX(pointerIndex),
                                        _screenLocationArray[1] + motionEvent.GetY(pointerIndex));

        // Use ActionMasked here rather than Action to reduce the number of possibilities
        switch ( args.Event.ActionMasked )
        {
            case MotionEventActions.Down:
            case MotionEventActions.PointerDown:
                FireEvent( this, id, ColorPickerTouchActionType.Pressed, screenPointerCoords, true );

                _idToEffectDictionary.Add( id, this );

                _capture = _commonBehavior.Capture;
                break;

            case MotionEventActions.Move:
                // Multiple Move events are bundled, so handle them in a loop
                for ( pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++ )
                {
                    id = motionEvent.GetPointerId( pointerIndex );

                    if ( _capture )
                    {
                        senderView.GetLocationOnScreen( _screenLocationArray );

                        screenPointerCoords = new Point( _screenLocationArray[ 0 ] + motionEvent.GetX( pointerIndex ),
                                                         _screenLocationArray[ 1 ] + motionEvent.GetY( pointerIndex ) );

                        FireEvent( this, id, ColorPickerTouchActionType.Moved, screenPointerCoords, true );
                    }
                    else
                    {
                        CheckForBoundaryHop( id, screenPointerCoords );

                        if ( _idToEffectDictionary[ id ] is not null )
                        {
                            FireEvent( _idToEffectDictionary[ id ], id, ColorPickerTouchActionType.Moved, screenPointerCoords, true );
                        }
                    }
                }

                break;

            case MotionEventActions.Up:
            case MotionEventActions.Pointer1Up:
                if ( _capture )
                {
                    FireEvent( this, id, ColorPickerTouchActionType.Released, screenPointerCoords, false );
                }
                else
                {
                    CheckForBoundaryHop( id, screenPointerCoords );

                    if ( _idToEffectDictionary[ id ] is not null )
                    {
                        FireEvent( _idToEffectDictionary[ id ], id, ColorPickerTouchActionType.Released, screenPointerCoords, false );
                    }
                }

                _idToEffectDictionary.Remove( id );
                break;

            case MotionEventActions.Cancel:
                if ( _capture )
                {
                    FireEvent( this, id, ColorPickerTouchActionType.Cancelled, screenPointerCoords, false );
                }
                else
                {
                    if ( _idToEffectDictionary[ id ] is not null )
                    {
                        FireEvent( _idToEffectDictionary[ id ], id, ColorPickerTouchActionType.Cancelled, screenPointerCoords, false );
                    }
                }

                _idToEffectDictionary.Remove( id );
                break;
        }
    }

    void CheckForBoundaryHop( int id, Point pointerLocation )
    {
        ColorPickerTouchActionBehaviorDroid touchEffectHit = null;

        foreach ( var view in _viewDictionary.Keys )
        {
            // Get the view rectangle
            try
            {
                view.GetLocationOnScreen( _screenLocationArray );
            }
            catch // System.ObjectDisposedException: Cannot access a disposed object.
            {
                continue;
            }

            Rectangle viewRect = new( _screenLocationArray[0], 
                                      _screenLocationArray[1], 
                                      view.Width, 
                                      view.Height );

            if ( viewRect.Contains( pointerLocation ) )
            {
                touchEffectHit = _viewDictionary[ view ];
            }
        }

        if ( touchEffectHit != _idToEffectDictionary[ id ] )
        {
            if ( _idToEffectDictionary[ id ] is not null )
            {
                FireEvent( _idToEffectDictionary[ id ], id, ColorPickerTouchActionType.Exited, pointerLocation, true );
            }

            if ( touchEffectHit is not null )
            {
                FireEvent( touchEffectHit, id, ColorPickerTouchActionType.Entered, pointerLocation, true );
            }

            _idToEffectDictionary[ id ] = touchEffectHit;
        }
    }

    void FireEvent( ColorPickerTouchActionBehaviorDroid behavior, int id, ColorPickerTouchActionType actionType, Point pointerLocation, bool isInContact )
    {
        // Get the location of the pointer within the view
        behavior._nativeView.GetLocationOnScreen( _screenLocationArray );

        var x = pointerLocation.X - _screenLocationArray[0];
        var y = pointerLocation.Y - _screenLocationArray[1];

        Point point = new( _fromPixels(x), _fromPixels(y) );

        // Call the method
        behavior._commonBehavior.OnTouchAction( behavior._formsElement, 
                                                new ColorPickerTouchActionEventArgs( id, actionType, point, isInContact ) );
    }
}
