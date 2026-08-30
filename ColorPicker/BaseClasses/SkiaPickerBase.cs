namespace ColorPicker.BaseClasses;

using ColorPicker.Behaviors;
#if WINDOWS
using ColorPicker.Platforms.WinUI;
#elif ANDROID
using ColorPicker.Platforms.Droid;
#endif

using SkiaSharp.Views.Maui.Controls;

/// <summary>
/// Base class for the SkiaSharp-drawn pickers (disc, triangle, sliders). Owns the
/// canvas, touch handling, and the indicator dot.
/// </summary>
public abstract class SkiaPickerBase : ColorPickerBase
{
    protected readonly SKCanvasView     CanvasView;

    public static readonly BindableProperty IndicatorRadiusScaleProperty
                         = BindableProperty.Create(nameof(IndicatorRadiusScale),
                                                    typeof(float),
                                                    typeof(SkiaPickerBase),
                                                    0.05F,
                                                    propertyChanged: HandlePickerRadiusScaleSet);
    /// <summary>Picker-dot radius as a fraction of the canvas.</summary>
    public float IndicatorRadiusScale
    {
        get => (float)GetValue(IndicatorRadiusScaleProperty);
        set => SetValue(IndicatorRadiusScaleProperty, value);
    }

    static void HandlePickerRadiusScaleSet(BindableObject bindable, object oldValue, object newValue)
            => ((SkiaPickerBase)bindable).InvalidateSurface();

    public static readonly BindableProperty IndicatorPaddingProperty
                         = BindableProperty.Create(nameof(IndicatorPadding),
                                                    typeof(float),
                                                    typeof(SkiaPickerBase),
                                                    3F,
                                                    propertyChanged: HandleIndicatorPaddingSet);

    /// <summary>
    /// Gap in pixels between the drawn content (with its indicator at the extreme)
    /// and the canvas edge, so indicators never sit flush against the border.
    /// Applied consistently to the wheel, triangle and sliders. Default <c>3</c>.
    /// </summary>
    public float IndicatorPadding
    {
        get => (float)GetValue(IndicatorPaddingProperty);
        set => SetValue(IndicatorPaddingProperty, value);
    }

    static void HandleIndicatorPaddingSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (!Equals(oldValue, newValue))
        {
            var picker = (SkiaPickerBase)bindable;
            picker.InvalidateMeasure();
            picker.InvalidateSurface();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SkiaPickerBase()
    {
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        var touchBehavior           =   new TouchBehavior();

#if WINDOWS
        var touchImpl               =   new WindowsTouchActionBehavior(touchBehavior);
#elif ANDROID
        var touchImpl               =   new AndroidTouchActionBehavior(touchBehavior);
#else
        throw new NotImplementedException("Specified platform not yet implemented");
#endif

        var view                    =   new SKCanvasView();
        view.PaintSurface += OnPaintSurface;
        view.Loaded += OnCanvasViewLoaded;
        CanvasView = view;

        touchBehavior.Capture = true;
        touchBehavior.TouchAction += OnTouchAction;

        Behaviors.Add(touchImpl);
        Children.Add(CanvasView);
    }

    public abstract float GetIndicatorRadiusPixels();
    public abstract float GetIndicatorRadiusPixels(SKSize canvasSize);

    protected abstract SizeRequest GetMeasure(double widthConstraint, double heightConstraint);
    protected abstract float GetSize();
    protected abstract float GetSize(SKSize canvasSize);
    protected abstract void OnPaintSurface(SKCanvas canvas, int width, int height);
    protected abstract void OnTouchActionPressed(TouchActionEventArgs args);
    protected abstract void OnTouchActionMoved(TouchActionEventArgs args);
    protected abstract void OnTouchActionReleased(TouchActionEventArgs args);
    protected abstract void OnTouchActionCancelled(TouchActionEventArgs args);

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // Apply WidthRequest/HeightRequest as constraints
        if (WidthRequest >= 0)
            widthConstraint = Math.Min(widthConstraint, WidthRequest);
        if (HeightRequest >= 0)
            heightConstraint = Math.Min(heightConstraint, HeightRequest);

        var sizeRequest = GetMeasure(widthConstraint, heightConstraint);
        var size = sizeRequest.Request;

        // Measure the child SKCanvasView so MAUI knows it needs rendering
        ((IView)CanvasView).Measure(size.Width, size.Height);

        return size;
    }

    protected override Size ArrangeOverride(Rect bounds)
    {
        // Call base which sets Frame, calls PlatformArrange on the native container,
        // and calls LayoutManager.ArrangeChildren to position native child views.
        var result = base.ArrangeOverride(bounds);

        // MAUI's base ArrangeOverride passes the *stale* Frame size to LayoutManager.ArrangeChildren
        // instead of our fresh `bounds`, so the SKCanvasView child can end up arranged at the
        // previous size. Re-arrange explicitly using the freshly-updated Frame size (which is
        // the actual on-screen size of this control, after centering/alignment is applied).
        var size = Frame.Size;
        if (size.Width > 0 && size.Height > 0)
        {
            ((IView)CanvasView).Arrange(new Rect(0, 0, size.Width, size.Height));
        }

        InvalidateSurface();

        return result;
    }

    protected SKPoint ConvertToPixel(Point pt)
    {
        var canvasSize = GetCanvasSize();
        return new SKPoint((float)(canvasSize.Width * pt.X / CanvasView.Width),
                           (float)(canvasSize.Height * pt.Y / CanvasView.Height));
    }

    protected SKSize GetCanvasSize() => CanvasView.CanvasSize;
    protected void InvalidateSurface() => CanvasView.InvalidateSurface();

    void OnCanvasViewLoaded(object sender, EventArgs e)
    {
        InvalidateSurface();
    }

    protected void PaintIndicator(SKCanvas canvas, SKPoint point)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        paint.Color = Colors.White.ToSKColor();
        paint.StrokeWidth = 2;
        canvas.DrawCircle(point, GetIndicatorRadiusPixels() - 2, paint);

        paint.Color = Colors.Black.ToSKColor();
        paint.StrokeWidth = 1;
        canvas.DrawCircle(point, GetIndicatorRadiusPixels() - 4, paint);
        canvas.DrawCircle(point, GetIndicatorRadiusPixels(), paint);
    }

    void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
    }

    void OnTouchAction(object sender, TouchActionEventArgs e)
    {
        switch (e.Type)
        {
            case TouchActionType.Pressed:
                OnTouchActionPressed(e);
                break;
            case TouchActionType.Moved:
                OnTouchActionMoved(e);
                break;
            case TouchActionType.Released:
                OnTouchActionReleased(e);
                break;
            case TouchActionType.Cancelled:
                OnTouchActionCancelled(e);
                break;
        }
    }
}
