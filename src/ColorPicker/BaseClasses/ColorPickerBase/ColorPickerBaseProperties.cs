namespace ColorPicker;

public partial class ColorPickerBase
{
    #region SelectedColor implementation
    /// <summary>
    /// SelectedColor bindable property
    /// </summary>
    public static readonly BindableProperty SelectedColorProperty 
                         = BindableProperty.Create( nameof(SelectedColor),
                                                    typeof(Color),
                                                    typeof(IColorPicker),
                                                    Color.FromHsla(0, 0.5, 0.5),
                                                    propertyChanged: OnSelectedColorPropertyChanged );
\
    public event EventHandler<ColorChangedEventArgs>? SelectedColorChanged;

    static void OnSelectedColorPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase colorPickerBase )
        {
            if ( oldValue != newValue && newValue is Color newColor )
            {
                colorPickerBase.UpdateSelectedColor();

                if ( colorPickerBase.AttachedTo is IColorPicker attachedPicker )
                    attachedPicker.SelectedColor = newColor;

                colorPickerBase.SelectedColorChanged?.Invoke( colorPickerBase, new ColorChangedEventArgs( (Color)oldValue, newColor ) );
            }
        }
    }

    public Color SelectedColor
    {
        get => (Color)GetValue( SelectedColorProperty );
        set => SetValue( SelectedColorProperty, value );
    }
    #endregion

    #region AttachedTo implementation
    /// <summary>
    /// AttachedTo bindable property
    /// </summary>
    public static readonly BindableProperty AttachedToProperty
                         = BindableProperty.Create( nameof(AttachedTo),
                                                    typeof(IColorPicker),
                                                    typeof(IColorPicker),
                                                    null,
                                                    propertyChanged: OnAttachedToPropertyChanged );

    static void OnAttachedToPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase colorPickerBase )
        {
            if ( oldValue is IColorPicker oldPicker )
                oldPicker.PropertyChanged -= colorPickerBase.AttachedToPicker;

            if ( newValue is IColorPicker newPicker )
            {
                newPicker.PropertyChanged += colorPickerBase.AttachedToPicker;
                newPicker.SelectedColor    = colorPickerBase.SelectedColor;
            }
        }
    }

    public IColorPicker AttachedTo
    {
        get => (IColorPicker)GetValue( AttachedToProperty );
        set => SetValue( AttachedToProperty, value );
    }

    void AttachedToPicker( object? sender, PropertyChangedEventArgs e )
    {
        if ( sender is IColorPicker picker && e.PropertyName == nameof( SelectedColor ) )
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
                                                    typeof(IColorPicker),
                                                    20.0,
                                                    propertyChanged: OnReticleRadiusPropertyChanged );

    static void OnReticleRadiusPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue is not null && bindable is ColorPickerBase colorPickerBase )
            colorPickerBase.UpdateReticle();
    }

    public double ReticleRadius
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
                                                    typeof(IColorPicker),
                                                    false,
                                                    propertyChanged: OnReticleCrosshairsPropertyChanged );

    static void OnReticleCrosshairsPropertyChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( newValue is not null && bindable is ColorPickerBase colorPickerBase )
            colorPickerBase.UpdateCrossHairs();
    }

    public bool ShowReticleCrossHairs
    {
        get => (bool)GetValue( ShowReticleCrossHairsProperty );
        set => SetValue( ShowReticleCrossHairsProperty, value );
    }
    #endregion
}
