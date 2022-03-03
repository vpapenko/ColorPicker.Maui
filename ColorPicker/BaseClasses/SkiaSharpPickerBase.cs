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

        var view                    =   new SKCanvasView();  
        view.PaintSurface          +=   OnPaintSurface;
        MyCanvasView                =   view;

        touchBehavior.Capture       =   true;
        touchBehavior.TouchAction  +=   OnTouchAction;

        Behaviors.Add( touchImpl );
        Children.Add( MyCanvasView );
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

    protected override SizeRequest OnMeasure( double widthConstraint, double heightConstraint )
            => GetMeasure( widthConstraint, heightConstraint );

    protected override void LayoutChildren( double x, double y, double width, double height )
            => MyCanvasView.Layout( new Rectangle( x, y, width, height ) );

    protected SKPoint ConvertToPixel( Point pt )
    {
        var canvasSize = GetCanvasSize();
        return new SKPoint( (float)( canvasSize.Width * pt.X / MyCanvasView.Width ),
                           (float)( canvasSize.Height * pt.Y / MyCanvasView.Height ) );
    }

    protected SKSize GetCanvasSize()    => MyCanvasView.CanvasSize;
    protected void InvalidateSurface()  => MyCanvasView.InvalidateSurface();

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
      => OnPaintSurface( e.Surface.Canvas, e.Info.Width, e.Info.Height );

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
