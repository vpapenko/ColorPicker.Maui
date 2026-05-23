namespace ColorPicker.BaseClasses;

using System.ComponentModel;

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
                         = BindableProperty.Create( nameof(SelectedColor),
                                                    typeof(Color),
                                                    typeof(ColorPickerBase),
                                                    Color.FromHsla(0, 0, 0.5),
                                                    propertyChanged: HandleSelectedColor );

    public static readonly BindableProperty AttachedColorPickerProperty
                         = BindableProperty.Create( nameof(AttachedColorPicker),
                                                    typeof(IColorPicker),
                                                    typeof(ColorPickerBase),
                                                    null,
                                                    propertyChanged: HandleConnectedColorPicker );

    //  Backing store
    //
    public Color SelectedColor 
    { 
        get => (Color)GetValue( SelectedColorProperty ); 
        set => SetValue( SelectedColorProperty, value ); 
    }

    public IColorPicker AttachedColorPicker
    {
        get => (IColorPicker)GetValue( AttachedColorPickerProperty );
        set => SetValue( AttachedColorPickerProperty, value );
    }

    //  ColorPicker Subclass must implement to intercept SelectedColor change
    //
    protected abstract void OnSelectedColorChanging( Color color );

    //  Required for .NET 8 MAUI Layout - use a simple layout manager
    //  that properly delegates measurement and arrangement to children
    //
    protected override ILayoutManager CreateLayoutManager()
        => new ColorPickerLayoutManager( this );

    /// <summary>
    /// Called by the layout manager to arrange children within the layout.
    /// The native LayoutPanel uses this to position native child views.
    /// Override in subclasses for custom child positioning.
    /// </summary>
    protected virtual Size ArrangeLayoutChildren( Rect bounds )
    {
        // Default: arrange all children to fill bounds
        foreach ( var child in Children )
        {
            ( (IView)child ).Arrange( bounds );
        }
        return bounds.Size;
    }

    private class ColorPickerLayoutManager : ILayoutManager
    {
        readonly ColorPickerBase _layout;
        bool _measuring;

        public ColorPickerLayoutManager( ColorPickerBase layout ) => _layout = layout;

        public Size Measure( double widthConstraint, double heightConstraint )
        {
            // Apply the layout's WidthRequest/HeightRequest before measuring children.
            // Without this, children get the raw parent constraints (e.g. 1192??)
            // and SkiaSharp renders at that size, causing clipping.
            if ( _layout.WidthRequest >= 0 )
                widthConstraint = Math.Min( widthConstraint, _layout.WidthRequest );
            if ( _layout.HeightRequest >= 0 )
                heightConstraint = Math.Min( heightConstraint, _layout.HeightRequest );

            // Measure all children with constrained values
            foreach ( var child in _layout.Children )
            {
                ( (IView)child ).Measure( widthConstraint, heightConstraint );
            }

            // Return the same size as MeasureOverride to keep native panel
            // and MAUI layout in sync.
            if ( !_measuring )
            {
                _measuring = true;
                var result = _layout.MeasureOverride( widthConstraint, heightConstraint );
                _measuring = false;
                return result;
            }

            // Fallback for recursive calls
            var width = double.IsInfinity( widthConstraint ) ? 0 : widthConstraint;
            var height = double.IsInfinity( heightConstraint ) ? 0 : heightConstraint;
            return new Size( width, height );
        }

        public Size ArrangeChildren( Rect bounds )
        {
            return _layout.ArrangeLayoutChildren( bounds );
        }
    }

    //  Handles SelectedColor change
    //
    static void HandleSelectedColor( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is not ColorPickerBase viewBase )
            return;

        if (oldValue != newValue)
        {
            //  Calls subclass implementation
            viewBase.OnSelectedColorChanging( (Color)newValue );

            if ( viewBase.AttachedColorPicker is not null )
            {
                viewBase.AttachedColorPicker.SelectedColor = (Color)newValue;
            }

            viewBase.RaiseSelectedColorChanged( (Color)oldValue, (Color)newValue );
        }
    }

    //  Connects to and/or disconnects from bound ColorPicker 
    //
    static void HandleConnectedColorPicker( BindableObject bindable, object oldValue, object newValue )
    {
        if (bindable is not ColorPickerBase viewBase)
            return;

        if ( oldValue is not null )
        {
            ((IColorPicker)oldValue).PropertyChanged -= viewBase.BoundColorPicker_PropertyChanged;
        }

        if (newValue is not null)
        {
            ((IColorPicker)newValue).PropertyChanged += viewBase.BoundColorPicker_PropertyChanged;
            ((IColorPicker)newValue).SelectedColor    = viewBase.SelectedColor;
        }
    }

    /// <summary>
    /// Property changed event handler
    /// </summary>
    void BoundColorPicker_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
        if (e.PropertyName == nameof( SelectedColor ))
        {
            SelectedColor = ((IColorPicker)sender).SelectedColor;
        }
    }

    /// <summary>
    /// Custom event handler for changes in SelectedColor
    /// </summary>
    public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;

    protected virtual void RaiseSelectedColorChanged( Color oldColor, Color newColor )
                        => SelectedColorChanged?.Invoke( this, new ColorChangedEventArgs( oldColor, newColor ) );
}
