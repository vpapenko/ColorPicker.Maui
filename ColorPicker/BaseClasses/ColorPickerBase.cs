namespace ColorPicker.BaseClasses;

using ColorPicker.Core.Connection;

/// <summary>
/// ColorPicker base class
/// 
/// This class exposes the SelectedColor and AttachedColorPicker bound properties to any 
/// ColorPicker implementation.
/// 
/// </summary>
public abstract class ColorPickerBase : Layout, IColorPicker, IRegisterable
{
    //  Bindable objects
    //
    public static readonly BindableProperty SelectedColorProperty
                         = BindableProperty.Create(nameof(SelectedColor),
                                                    typeof(Color),
                                                    typeof(ColorPickerBase),
                                                    Color.FromHsla(0, 0, 0.5),
                                                    propertyChanged: HandleSelectedColor);

    public static readonly BindableProperty AttachedColorPickerProperty
                         = BindableProperty.Create(nameof(AttachedColorPicker),
                                                    typeof(IColorPicker),
                                                    typeof(ColorPickerBase),
                                                    null,
                                                    propertyChanged: HandleConnectedColorPicker);

    //  Backing store
    //
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public IColorPicker AttachedColorPicker
    {
        get => (IColorPicker)GetValue(AttachedColorPickerProperty);
        set => SetValue(AttachedColorPickerProperty, value);
    }

    //  Shared, cycle-safe connection graph. Every AttachedColorPicker link is an
    //  undirected edge; all pickers in one connected component share a color.
    //
    static readonly ConnectionGraph<IColorPicker> ConnectionGraph = new();

    //  Guards against update storms while a color change fans out across a
    //  connected component: nested SelectedColor setters still repaint (via
    //  OnSelectedColorChanging) but must not start their own propagation. This is
    //  what makes cyclic links (A-B-C-A) safe.
    //
    [ThreadStatic] static bool _propagatingColor;

    //  ColorPicker Subclass must implement to intercept SelectedColor change
    //
    protected abstract void OnSelectedColorChanging(Color color);

    //  Required for .NET 8 MAUI Layout - use a simple layout manager
    //  that properly delegates measurement and arrangement to children
    //
    protected override ILayoutManager CreateLayoutManager()
        => new ColorPickerLayoutManager(this);

    /// <summary>
    /// Called by the layout manager to arrange children within the layout.
    /// The native LayoutPanel uses this to position native child views.
    /// Override in subclasses for custom child positioning.
    /// </summary>
    protected virtual Size ArrangeLayoutChildren(Rect bounds)
    {
        // Default: arrange all children to fill bounds
        foreach (var child in Children)
        {
            ((IView)child).Arrange(bounds);
        }
        return bounds.Size;
    }

    private class ColorPickerLayoutManager : ILayoutManager
    {
        readonly ColorPickerBase _layout;
        bool _measuring;

        public ColorPickerLayoutManager(ColorPickerBase layout) => _layout = layout;

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            // Apply the layout's WidthRequest/HeightRequest before measuring children.
            // Without this, children get the raw parent constraints (e.g. 1192??)
            // and SkiaSharp renders at that size, causing clipping.
            if (_layout.WidthRequest >= 0)
                widthConstraint = Math.Min(widthConstraint, _layout.WidthRequest);
            if (_layout.HeightRequest >= 0)
                heightConstraint = Math.Min(heightConstraint, _layout.HeightRequest);

            // Measure all children with constrained values
            foreach (var child in _layout.Children)
            {
                ((IView)child).Measure(widthConstraint, heightConstraint);
            }

            // Return the same size as MeasureOverride to keep native panel
            // and MAUI layout in sync.
            if (!_measuring)
            {
                _measuring = true;
                var result = _layout.MeasureOverride(widthConstraint, heightConstraint);
                _measuring = false;
                return result;
            }

            // Fallback for recursive calls
            var width = double.IsInfinity(widthConstraint) ? 0 : widthConstraint;
            var height = double.IsInfinity(heightConstraint) ? 0 : heightConstraint;
            return new Size(width, height);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            return _layout.ArrangeLayoutChildren(bounds);
        }
    }

    //  Handles SelectedColor change
    //
    static void HandleSelectedColor(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ColorPickerBase viewBase)
            return;

        if (Equals(oldValue, newValue))
            return;

        //  Repaint this control.
        viewBase.OnSelectedColorChanging((Color)newValue);

        //  Fan the new color out to every linked picker, exactly once. Nested
        //  setters triggered during an in-flight propagation are skipped.
        if (!_propagatingColor)
            PropagateColor(viewBase, (Color)newValue);

        viewBase.RaiseSelectedColorChanged((Color)oldValue, (Color)newValue);
    }

    //  Pushes a color to every picker in the source's connected component, once.
    //
    static void PropagateColor(IColorPicker source, Color color)
    {
        _propagatingColor = true;
        try
        {
            foreach (var picker in ConnectionGraph.ConnectedComponent(source))
            {
                if (!ReferenceEquals(picker, source))
                    picker.SelectedColor = color;
            }
        }
        finally
        {
            _propagatingColor = false;
        }
    }

    //  Connects to and/or disconnects from a bound ColorPicker. Links are stored
    //  as undirected edges, so any picker can be attached to any other in any
    //  order to form an arbitrary graph.
    //
    static void HandleConnectedColorPicker(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ColorPickerBase viewBase)
            return;

        if (oldValue is IColorPicker oldPeer)
            ConnectionGraph.RemoveEdge(viewBase, oldPeer);

        if (newValue is IColorPicker newPeer)
        {
            ConnectionGraph.AddEdge(viewBase, newPeer);

            //  Unify the freshly merged component on this control's color.
            if (!_propagatingColor)
                PropagateColor(viewBase, viewBase.SelectedColor);
        }
    }

    /// <summary>
    /// Custom event handler for changes in SelectedColor
    /// </summary>
    public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;

    protected virtual void RaiseSelectedColorChanged(Color oldColor, Color newColor)
                        => SelectedColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
}
