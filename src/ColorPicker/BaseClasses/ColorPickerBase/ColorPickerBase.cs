namespace ColorPicker;

public partial class ColorPickerBase : GraphicsView, IColorPicker
{
    public ColorPickerBaseDrawable? PickerDrawable { get; set; }
    public IMathAbstractions? PickerMath { get; set; }

    public ColorPickerBase()
    {
        StartInteraction += OnStartInteraction;
        DragInteraction += OnDragInteraction;
        EndInteraction += OnEndInteraction;
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

    public PointF ScalePoint( PointF point )
    {
        return new PointF( point.X / (float)Width, point.Y / (float)Height );
    }

    public PointF UnscalePoint( PointF point )
    {
        return new PointF( point.X * (float)Width, point.Y * (float)Height );
    }
    #endregion
}