namespace ColorPicker;

public abstract partial class ColorPickerBase : GraphicsView
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
        UpdateWithSelectedColor();
    }

    void UpdateReticle()
    {
        UpdateWithSelectedColor();
    }

    void UpdateCrossHairs()
    {
        UpdateWithSelectedColor();
    }
    #endregion

    #region Touch/Mouse interactions
    void OnStartInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdatePositionFromInteraction( touchPoint );
    }

    void OnDragInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdatePositionFromInteraction( touchPoint );
    }

    void OnEndInteraction( object? sender, TouchEventArgs e )
    {
        var touchPoint = e.Touches[ 0 ];
        UpdatePositionFromInteraction( touchPoint );
    }

    void UpdatePositionFromInteraction( PointF touchPoint )
    {
        SelectedColor = PickerMath!.UpdateColor( ScalePoint( touchPoint ), SelectedColor );
        UpdateWithSelectedColor();
    }

    void UpdateWithSelectedColor()
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