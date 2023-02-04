namespace ColorPicker;

public partial class ColorPickerBase : GraphicsView, IColorPicker
{
    public ColorPickerBaseDrawable? PickerDrawable  { get; set; }
    public IMathAbstractions? PickerMath            { get; set; }

    public ColorPickerBase()
    {
        StartInteraction        += OnStartInteraction;
        DragInteraction         += OnDragInteraction;
        EndInteraction          += OnEndInteraction;
        CancelInteraction       += OnCancelInteraction;
        StartHoverInteraction   += OnStartHoverInteraction;
        MoveHoverInteraction    += OnMoveHoverInteraction;
        EndHoverInteraction     += OnEndHoverInteraction;
    }

    #region UI Updates
    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        if ( Parent is not null )
        {
            UpdateSelectedColor();
            UpdateReticle();
            UpdateCrossHairs();
        }
    }

    void UpdateSelectedColor()
    {
        UpdateBySelectedColor();
    }

    void UpdateReticle()
    {
        UpdateBySelectedColor();
    }

    void UpdateCrossHairs()
    {
        UpdateBySelectedColor();
    }
    #endregion

    #region Touch/Mouse interactions (overridable)
    public virtual void OnStartInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdateColorFromInteraction( touchPoint );
    }

    public virtual void OnDragInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdateColorFromInteraction( touchPoint );
    }

    public virtual void OnEndInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdateColorFromInteraction( touchPoint );
    }

    public virtual void OnStartHoverInteraction( object? sender, TouchEventArgs e ) { }
    public virtual void OnMoveHoverInteraction( object? sender, TouchEventArgs e ) { }
    public virtual void OnEndHoverInteraction( object? sender, EventArgs e ) { }
    public virtual void OnCancelInteraction( object? sender, EventArgs e ) { }

    public void UpdateColorFromInteraction( PointF touchPoint )
    {
        SelectedColor = PickerMath!.UpdateColor( ScalePoint( touchPoint ), SelectedColor );
        UpdateBySelectedColor();
    }

    public void UpdateBySelectedColor()
    {
        PickerDrawable!.Center = UnscalePoint( PickerMath!.ColorToPoint( SelectedColor ) );
        Invalidate();
    }

    public PointF ScalePoint( PointF point )    => new PointF( point.X / (float)Width, point.Y / (float)Height );
    public PointF UnscalePoint( PointF point )  => new PointF( point.X * (float)Width, point.Y * (float)Height );
    #endregion
}
