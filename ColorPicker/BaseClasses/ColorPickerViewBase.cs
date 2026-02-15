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
    //  that properly delegates measurement and arrangement to children
    //
    protected override ILayoutManager CreateLayoutManager()
        => new ColorPickerLayoutManager( this );

    private class ColorPickerLayoutManager : ILayoutManager
    {
        readonly Layout _layout;

        public ColorPickerLayoutManager( Layout layout ) => _layout = layout;

        public Size Measure( double widthConstraint, double heightConstraint )
        {
            // Measure all children so MAUI knows they need rendering
            foreach ( var child in _layout.Children )
                ( (IView)child ).Measure( widthConstraint, heightConstraint );

            var width = double.IsInfinity( widthConstraint ) ? 0 : widthConstraint;
            var height = double.IsInfinity( heightConstraint ) ? 0 : heightConstraint;
            System.Diagnostics.Debug.WriteLine( $"[ColorPickerLayoutManager] Measure w={widthConstraint} h={heightConstraint} -> {width}x{height} children={_layout.Children.Count}" );
            return new Size( width, height );
        }

        public Size ArrangeChildren( Rect bounds )
        {
            // Arrange all children so native LayoutPanel positions them.
            // Without this, WinUI's LayoutPanel.ArrangeOverride never positions
            // the native child views and they remain invisible.
            System.Diagnostics.Debug.WriteLine( $"[ColorPickerLayoutManager] ArrangeChildren bounds={bounds} children={_layout.Children.Count}" );

            foreach ( var child in _layout.Children )
            {
                var childView = (IView)child;
                childView.Arrange( bounds );
                System.Diagnostics.Debug.WriteLine( $"[ColorPickerLayoutManager] Arranged child {child.GetType().Name} Frame={((VisualElement)child).Frame}" );
            }

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
