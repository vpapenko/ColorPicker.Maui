namespace ColorPicker;

public abstract partial class ColorPickerBase<T>
{
    /// <summary>
    /// SelectedColor bindable property
    /// </summary>
    public static readonly BindableProperty SelectedColorProperty
            = BindableProperty.Create( nameof(SelectedColor),
                                       typeof(Color),
                                       typeof(ColorPickerBase<T>),
                                       Color.FromHsla(0, 0.5, 0.5),
                                       propertyChanged: OnSelectedColorChanged );
    public Color SelectedColor
    {
        get => (Color)GetValue( SelectedColorProperty );
        set => SetValue( SelectedColorProperty, value );
    }

    static void OnSelectedColorChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue )
            colorPickerBase.UpdateBySelectedColor();
    }

    /// <summary>
    /// AttachedTo bindable property
    /// </summary>
    public static readonly BindableProperty AttachedToProperty
            = BindableProperty.Create( nameof(AttachedTo),
                                       typeof(IColorPicker),
                                       typeof(ColorPickerBase<T>),
                                       null,
                                       propertyChanged: new BindableProperty.BindingPropertyChangedDelegate(OnAttachedToChanged)
                                     );
    public IColorPicker AttachedTo
    {
        get => (IColorPicker)GetValue( AttachedToProperty );
        set => SetValue( AttachedToProperty, value );
    }

    static void OnAttachedToChanged( BindableObject bindable, object oldValue, object newValue )
    {
        if ( bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue )
        {
            //  TODO: Attached to another colorpicker. 
        }
    }
}
