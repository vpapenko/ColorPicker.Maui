namespace ColorPicker.BaseClasses;

using ColorPicker.Behaviors;
#if WINDOWS
using ColorPicker.Platforms.WinUI;
#elif ANDROID
using ColorPicker.Platforms.Droid;
#endif

using SkiaSharp.Views.Maui.Controls;

public abstract class SkiaSharpPickerBase : ColorPickerViewBase
{
    protected readonly SKCanvasView     MyCanvasView;

    public static readonly BindableProperty PickerRadiusScaleProperty
                         = BindableProperty.Create( nameof(PickerRadiusScale),
                                                    typeof(float),
                                                    typeof(SkiaSharpPickerBase),
                                                    0.05F,
                                                    propertyChanged: HandlePickerRadiusScaleSet );
    public float PickerRadiusScale
    {
        get => (float)GetValue( PickerRadiusScaleProperty );
        set => SetValue( PickerRadiusScaleProperty, value );
    }

    static void HandlePickerRadiusScaleSet( BindableObject bindable, object oldValue, object newValue )
            => ( (SkiaSharpPickerBase)bindable ).InvalidateSurface();

    /// <summary>
    /// Constructor
    /// </summary>
    public SkiaSharpPickerBase()
    {
        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] Constructor START ({GetType().Name})" );

        HorizontalOptions           =   LayoutOptions.Center;
        VerticalOptions             =   LayoutOptions.Center;

        var touchBehavior           =   new ColorPickerTouchBehavior();

#if WINDOWS
        var touchImpl               =   new ColorPickerTouchActionBehaviorWinUI( touchBehavior );
#elif ANDROID
        var touchImpl               =   new ColorPickerTouchActionBehaviorDroid( touchBehavior );
#else
        throw new NotImplementedException( "Specified platform not yet implemented" );
#endif

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] Touch behavior created" );

        var view                    =   new SKCanvasView();  
        view.PaintSurface          +=   OnPaintSurface;
        view.Loaded                +=   OnCanvasViewLoaded;
        MyCanvasView                =   view;

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] SKCanvasView created" );

        touchBehavior.Capture       =   true;
        touchBehavior.TouchAction  +=   OnTouchAction;

        Behaviors.Add( touchImpl );
        Children.Add( MyCanvasView );

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] Constructor END, Children.Count={Children.Count}" );
    }

    public abstract     float       GetPickerRadiusPixels();
    public abstract     float       GetPickerRadiusPixels( SKSize canvasSize );

    protected abstract  SizeRequest GetMeasure( double widthConstraint, double heightConstraint );
    protected abstract  float       GetSize();
    protected abstract  float       GetSize( SKSize canvasSize );
    protected abstract  void        OnPaintSurface( SKCanvas canvas, int width, int height );
    protected abstract  void        OnTouchActionPressed( ColorPickerTouchActionEventArgs args );
    protected abstract  void        OnTouchActionMoved( ColorPickerTouchActionEventArgs args );
    protected abstract  void        OnTouchActionReleased( ColorPickerTouchActionEventArgs args );
    protected abstract  void        OnTouchActionCancelled( ColorPickerTouchActionEventArgs args );

    protected override Size MeasureOverride( double widthConstraint, double heightConstraint )
    {
        // Apply WidthRequest/HeightRequest as constraints
        if ( WidthRequest >= 0 )
            widthConstraint = Math.Min( widthConstraint, WidthRequest );
        if ( HeightRequest >= 0 )
            heightConstraint = Math.Min( heightConstraint, HeightRequest );

        var sizeRequest = GetMeasure( widthConstraint, heightConstraint );
        var size = sizeRequest.Request;

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] MeasureOverride({GetType().Name}) w={widthConstraint} h={heightConstraint} -> {size}" );

        // Measure the child SKCanvasView so MAUI knows it needs rendering
        ( (IView)MyCanvasView ).Measure( size.Width, size.Height );

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] MeasureOverride({GetType().Name}) child measured: DesiredSize={MyCanvasView.DesiredSize}" );

        return size;
    }

    protected override Size ArrangeOverride( Rect bounds )
    {
        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] ArrangeOverride({GetType().Name}) bounds={bounds}" );

        // Call base which sets Frame, calls PlatformArrange on the native container,
        // and calls LayoutManager.ArrangeChildren to position native child views.
        var result = base.ArrangeOverride( bounds );

        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] ArrangeOverride({GetType().Name}) canvasView.Width={MyCanvasView.Width} Height={MyCanvasView.Height} Handler={MyCanvasView.Handler?.GetType().Name ?? "NULL"}" );

        InvalidateSurface();

        return result;
    }

    protected SKPoint ConvertToPixel( Point pt )
    {
        var canvasSize = GetCanvasSize();
        return new SKPoint( (float)( canvasSize.Width * pt.X / MyCanvasView.Width ),
                           (float)( canvasSize.Height * pt.Y / MyCanvasView.Height ) );
    }

    protected SKSize GetCanvasSize()    => MyCanvasView.CanvasSize;
    protected void InvalidateSurface()  => MyCanvasView.InvalidateSurface();

    void OnCanvasViewLoaded( object sender, EventArgs e )
    {
        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] OnCanvasViewLoaded({GetType().Name}) canvasView.Handler={MyCanvasView.Handler?.GetType().Name ?? "NULL"} Width={MyCanvasView.Width} Height={MyCanvasView.Height}" );
        InvalidateSurface();
    }

    protected void PaintPicker( SKCanvas canvas, SKPoint point )
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        paint.Color         = Colors.White.ToSKColor();
        paint.StrokeWidth   = 2;
        canvas.DrawCircle( point, GetPickerRadiusPixels() - 2, paint );

        paint.Color         = Colors.Black.ToSKColor();
        paint.StrokeWidth   = 1;
        canvas.DrawCircle( point, GetPickerRadiusPixels() - 4, paint );
        canvas.DrawCircle( point, GetPickerRadiusPixels(), paint );
    }

    void OnPaintSurface( object sender, SKPaintSurfaceEventArgs e )
    {
        System.Diagnostics.Debug.WriteLine( $"[SkiaSharpPickerBase] OnPaintSurface({GetType().Name}) info={e.Info.Width}x{e.Info.Height} canvasSize={MyCanvasView.CanvasSize}" );
        OnPaintSurface( e.Surface.Canvas, e.Info.Width, e.Info.Height );
    }

    void OnTouchAction( object sender, ColorPickerTouchActionEventArgs e )
    {
        switch ( e.Type )
        {
            case ColorPickerTouchActionType.Pressed:
                OnTouchActionPressed( e );
                break;
            case ColorPickerTouchActionType.Moved:
                OnTouchActionMoved( e );
                break;
            case ColorPickerTouchActionType.Released:
                OnTouchActionReleased( e );
                break;
            case ColorPickerTouchActionType.Cancelled:
                OnTouchActionCancelled( e );
                break;
        }
    }
}
