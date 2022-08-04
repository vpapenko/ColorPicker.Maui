namespace ColorPicker;

public abstract partial class ColorPickerBase<T>
{
    #region SelectedColor implementation
    /// <summary>
    /// SelectedColor bindable property
    /// </summary>
    public static readonly BindableProperty SelectedColorProperty
            = BindableProperty.Create( nameof(SelectedColor),
                                       typeof(Color),
                                       typeof(ColorPickerBase<T>),
                                       Color.FromHsla(0, 0.5, 0.5),
                                       propertyChanged: OnSelectedColorChanged );

    static void OnSelectedColorChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue )
        { 
            colorPickerBase.SelectedColorChanged( (Color)newValue );

            if ( colorPickerBase.AttachedTo is not null )
                colorPickerBase.AttachedTo.SelectedColor = (Color)newValue;

            colorPickerBase.RaiseUpdateSelectedColor( (Color)oldValue, (Color)newValue );
        }
    }

    public Color SelectedColor
    {
        get => (Color)GetValue( SelectedColorProperty );
        set => SetValue( SelectedColorProperty, value );
    }
   
    protected virtual void SelectedColorChanged( Color newColor )
    {
        UpdateBySelectedColor();
    }

    public event EventHandler<ColorChangedEventArgs>? SelectedColorChangedEvent;

    protected virtual void RaiseUpdateSelectedColor(Color oldColor, Color newColor)
            => SelectedColorChangedEvent?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
    #endregion

    #region AttachedTo implementation

    /// <summary>
    /// AttachedTo bindable property
    /// </summary>
    public static readonly BindableProperty AttachedToProperty
            = BindableProperty.Create( nameof(AttachedTo),
                                       typeof(IColorPicker),
                                       typeof(ColorPickerBase<T>),
                                       null,
                                       propertyChanged: OnAttachedToChanged );

    static void OnAttachedToChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase )
        {
            if ( oldValue is IColorPicker oldPicker )
                oldPicker.PropertyChanged  -= colorPickerBase.AttachedToPropertyChanged;

            if ( newValue is IColorPicker newPicker )
            {
                newPicker.PropertyChanged  += colorPickerBase.AttachedToPropertyChanged;
                newPicker.SelectedColor     = colorPickerBase.SelectedColor;
            }
        }
    }

    public IColorPicker AttachedTo
    {
        get => (IColorPicker)GetValue( AttachedToProperty );
        set => SetValue( AttachedToProperty, value );
    }

    void AttachedToPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is IColorPicker picker && e.PropertyName == nameof(SelectedColor) )
            SelectedColor = picker.SelectedColor;
    }
    #endregion

    #region ReticalRadius implementation

    /// <summary>
    /// ReticalRadius bindable property
    /// </summary>
    public static readonly BindableProperty ReticleRadiusProperty
            = BindableProperty.Create( nameof(ReticleRadius),
                                       typeof(double),
                                       typeof(ColorPickerBase<T>),
                                       20.0,
                                       propertyChanged: OnReticleRadiusChanged );

    private static void OnReticleRadiusChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue )
            colorPickerBase.UpdateBySelectedColor();
    }

    public double   ReticleRadius
    {
        get => (double)GetValue( ReticleRadiusProperty );
        set => SetValue( ReticleRadiusProperty, value < 15.0 ? 15.0 : value );
    }

    #endregion 

    #region ShowReticleCrossHairs implementation

    /// <summary>
    /// ReticalRadius bindable property
    /// </summary>
    public static readonly BindableProperty ShowReticleCrossHairsProperty
            = BindableProperty.Create( nameof(ShowReticleCrossHairs),
                                       typeof(bool),
                                       typeof(ColorPickerBase<T>),
                                       false,
                                       propertyChanged: OnShowReticleCrossHairsChanged );

    private static void OnShowReticleCrossHairsChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue )
            colorPickerBase.UpdateBySelectedColor();
    }

    public bool   ShowReticleCrossHairs
    {
        get => (bool)GetValue( ShowReticleCrossHairsProperty );
        set => SetValue( ShowReticleCrossHairsProperty, value );
    }

    #endregion
}
