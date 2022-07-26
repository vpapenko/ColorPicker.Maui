namespace ColorPicker;

public abstract partial class ColorPickerBase<T> : GraphicsView, IColorPicker where T : ColorPickerMathBase, new()
{
    readonly T  _colorPickerMath;
    PointF      _pickerLocation;

    protected Action<double, double> _setAspectRatio;

    public double ReticleRadius { get; set; } = 20;

    public ColorPickerBase()
    {
        _setAspectRatio     = SetAspectRatioToHeight;
        _colorPickerMath    = new T();
        Drawable            = new ColorPickerBaseDrawable( DrawBackground, DrawReticle );

        StartInteraction   += OnStartInteraction;
        DragInteraction    += OnDragInteraction;
        EndInteraction     += OnEndInteraction;

        UpdateBySelectedColor();
    }

    protected abstract void DrawBackground( ICanvas canvas, RectF dirtyRect );

    protected void DrawReticle( ICanvas canvas, RectF dirtyRect )
    {
        canvas.StrokeSize = 2;

        canvas.StrokeColor = Colors.White;
        canvas.DrawCircle( _pickerLocation, ReticleRadius );

        canvas.StrokeColor = Colors.Black;
        canvas.DrawCircle( _pickerLocation, ReticleRadius - 2 );

        canvas.StrokeColor = Colors.White;
        canvas.DrawCircle( _pickerLocation, ReticleRadius - 4 );
    }

    protected void SetAspectRatioToHeight( double widthConstraint, double heightConstraint )
    {
        this.HeightRequest = ReticleRadius * 2 + 2;
    }

    protected void SetAspectRatioSquare( double widthConstraint, double heightConstraint )
    {
        var minConstraint = Math.Min(widthConstraint, heightConstraint);
        this.WidthRequest = minConstraint;
        this.HeightRequest = minConstraint;
    }

    protected override Size MeasureOverride( double widthConstraint, double heightConstraint )
    {
        _setAspectRatio( widthConstraint, heightConstraint );
        return base.MeasureOverride( widthConstraint, widthConstraint );
    }

    protected override Size ArrangeOverride( Rect bounds )
    {
        UpdateBySelectedColor();
        return base.ArrangeOverride( bounds );
    }

    void OnEndInteraction( object? sender, TouchEventArgs e )
    {
        UpdateColor( e.Touches[ 0 ] );
    }

    void OnDragInteraction( object? sender, TouchEventArgs e )
    {
        UpdateColor( e.Touches[ 0 ] );
    }

    void OnStartInteraction( object? sender, TouchEventArgs e )
    {
        UpdateColor( e.Touches[ 0 ] );
    }

    void UpdateColor( PointF pointF )
    {
        SelectedColor = _colorPickerMath.UpdateColor( ScalePoint( pointF ), SelectedColor );
        _pickerLocation = UnscalePoint( _colorPickerMath.ColorToPoint( SelectedColor ) );

        this.Invalidate();
    }

    void UpdateBySelectedColor()
    {
        _pickerLocation = UnscalePoint( _colorPickerMath.ColorToPoint( SelectedColor ) );

        this.Invalidate();
    }

    PointF ScalePoint( PointF point )
    {
        return new PointF( point.X / (float)this.Width, point.Y / (float)this.Height );
    }

    PointF UnscalePoint( PointF point )
    {
        return new PointF( point.X * (float)this.Width, point.Y * (float)this.Height );
    }
}