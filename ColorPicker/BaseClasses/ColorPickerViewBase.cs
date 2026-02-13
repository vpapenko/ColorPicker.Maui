namespace ColorPicker.BaseClasses;

using System.ComponentModel;

/// <summary>
/// ColorPicker base class
/// 
/// This class exposes the SelectedColor and AttachedColorPicker bound properties to any 
/// ColorPicker implementation.
/// 
/// </summary>
public abstract class ColorPickerViewBase : Layout, IColorPicker, IRegisterable
{
    //  Bindable objects
    //
    public static readonly BindableProperty SelectedColorProperty
                         = BindableProperty.Create( nameof(SelectedColor),
                                                    typeof(Color),
                                                    typeof(ColorPickerViewBase),
                                                    Color.FromHsla(0, 0, 0.5),
                                                    propertyChanged: HandleSelectedColor );

    public static readonly BindableProperty AttachedColorPickerProperty
                         = BindableProperty.Create( nameof(AttachedColorPicker),
                                                    typeof(IColorPicker),
                                                    typeof(ColorPickerViewBase),
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
    //  that just respects the measure and arrange overrides in derived classes
    //
    protected override ILayoutManager CreateLayoutManager()
        => new ColorPickerLayoutManager( this );

    private class ColorPickerLayoutManager : ILayoutManager
    {
        private readonly Layout _layout;

        public ColorPickerLayoutManager( Layout layout )
        {
            _layout = layout;
        }

        public Size Measure( double widthConstraint, double heightConstraint )
        {
            // Let the layout handle its own measurement through MeasureOverride
            return Size.Zero;
        }

        public Size ArrangeChildren( Rect bounds )
        {
            // Let the layout handle its own arrangement through ArrangeOverride
            return bounds.Size;
        }
    }

    //  Handles SelectedColor change
    //
    static void HandleSelectedColor( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is not ColorPickerViewBase viewBase )
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
        if (bindable is not ColorPickerViewBase viewBase)
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
