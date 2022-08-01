﻿namespace ColorPicker;

public abstract partial class ColorPickerBase<T> : GraphicsView, IColorPicker where T : ColorPickerMathBase, new()
{
    readonly T  _colorPickerMath;
    PointF      _pickerLocation;

    protected Action<double, double> SetAspectRatio;

    public ColorPickerBase()
    {
        SetAspectRatio      = SetAspectRatioToHeight;
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

        if ( ShowReticleCrossHairs )
        {
            canvas.StrokeColor = Colors.Black;
            DrawCrossHairsHorizontal( canvas, _pickerLocation, (float)(ReticleRadius - 4) );
            DrawCrossHairsVertical( canvas, _pickerLocation, (float)(ReticleRadius - 4) );
        }
    }

    void DrawCrossHairsHorizontal( ICanvas canvas, PointF c, float r )
    { 
        canvas.DrawLine( c.X - r + 1, c.Y, c.X - 4f, c.Y );
        canvas.DrawLine( c.X + 4f, c.Y, c.X + r - 1, c.Y );       
    }

    void DrawCrossHairsVertical( ICanvas canvas, PointF c, float r )
    {
        canvas.DrawLine( c.X, c.Y - r + 1, c.X, c.Y - 4f );
        canvas.DrawLine( c.X, c.Y + 4f, c.X, c.Y + r - 1 );
    }


    protected void SetAspectRatioToHeight( double widthConstraint, double heightConstraint )
    {
        HeightRequest = ReticleRadius * 2 + 2;
    }

    protected void SetAspectRatioSquare( double widthConstraint, double heightConstraint )
    {
        var minConstraint = Math.Min(widthConstraint, heightConstraint);
        WidthRequest = minConstraint;
        HeightRequest = minConstraint;
    }

    protected override Size MeasureOverride( double widthConstraint, double heightConstraint )
    {
        SetAspectRatio( widthConstraint, heightConstraint );
        return base.MeasureOverride( widthConstraint, widthConstraint );
    }

    protected override Size ArrangeOverride( Rect bounds )
    {
        UpdateBySelectedColor();
        return base.ArrangeOverride( bounds );
    }

    void OnStartInteraction( object? sender, TouchEventArgs e ) =>  UpdateColor( e.Touches[ 0 ] );
    void OnDragInteraction( object? sender, TouchEventArgs e )  =>  UpdateColor( e.Touches[ 0 ] );
    void OnEndInteraction( object? sender, TouchEventArgs e )   =>  UpdateColor( e.Touches[ 0 ] );

    void UpdateColor( PointF pointF )
    {
        SelectedColor = _colorPickerMath.UpdateColor( ScalePoint( pointF ), SelectedColor );
        _pickerLocation = UnscalePoint( _colorPickerMath.ColorToPoint( SelectedColor ) );
        Invalidate();
    }

    void UpdateBySelectedColor()
    {
        _pickerLocation = UnscalePoint( _colorPickerMath.ColorToPoint( SelectedColor ) );
        Invalidate();
    }

    PointF ScalePoint( PointF point )
    {
        return new PointF( point.X / (float)Width, point.Y / (float)Height );
    }

    PointF UnscalePoint( PointF point )
    {
        return new PointF( point.X * (float)Width, point.Y * (float)Height );
    }
}