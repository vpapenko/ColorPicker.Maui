namespace ColorPicker;

public partial class ColorPickerBase : GraphicsView, IColorPicker, IMathAbstractions
{
    public ColorPickerBaseDrawable? PickerDrawable  { get; set; }

    public ColorPickerBase()
    {
        StartInteraction        += OnStartInteraction;
        DragInteraction         += OnDragInteraction;
        EndInteraction          += OnEndInteraction;

        StartHoverInteraction   += OnStartHoverInteraction;
        MoveHoverInteraction    += OnMoveHoverInteraction;
        EndHoverInteraction     += OnEndHoverInteraction;

        CancelInteraction       += OnCancelInteraction;
    }

    #region UI Updates
    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        if ( Parent is not null )
            UpdateSelectedColor();
    }
    #endregion

    #region Touch/Mouse interactions (overridable)
    public virtual void OnStartInteraction( object? sender, TouchEventArgs e )  =>  UpdateColorFromTouchPoint( e.Touches[ 0 ] );
    public virtual void OnDragInteraction( object? sender, TouchEventArgs e )   =>  UpdateColorFromTouchPoint( e.Touches[ 0 ] );
    public virtual void OnEndInteraction( object? sender, TouchEventArgs e )    =>  UpdateColorFromTouchPoint( e.Touches[ 0 ] );

    public virtual void OnStartHoverInteraction( object? sender, TouchEventArgs e ) { }
    public virtual void OnMoveHoverInteraction( object? sender, TouchEventArgs e ) { }
    public virtual void OnEndHoverInteraction( object? sender, EventArgs e ) { }

    public virtual void OnCancelInteraction( object? sender, EventArgs e ) { }

    public void UpdateColorFromTouchPoint( PointF touchPoint )
    {
        SelectedColor = UpdateColor( ScalePoint( touchPoint ), SelectedColor );
        UpdateSelectedColor();
    }

    public void UpdateSelectedColor( Color? color = null )
    {
        color ??= SelectedColor;
        PickerDrawable!.Center = UnscalePoint( ColorToPoint( color ) );
        Invalidate();
    }

    public PointF ScalePoint( PointF point )    => new PointF( point.X / (float)Width, point.Y / (float)Height );
    public PointF UnscalePoint( PointF point )  => new PointF( point.X * (float)Width, point.Y * (float)Height );

    //  Math Abstractions - must be implemented by subclass
    public virtual bool     IsInActiveArea( PointF point )              => throw new NotImplementedException();
    public virtual PointF   FitToActiveArea( PointF point )             => throw new NotImplementedException();
    public virtual PointF   ColorToPoint( Color color )                 => throw new NotImplementedException();
    public virtual Color    UpdateColor( PointF point, Color color )    => throw new NotImplementedException();
    #endregion
}
