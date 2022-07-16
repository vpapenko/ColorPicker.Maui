namespace ColorPicker;

public abstract partial class ColorPickerBase<T>
{
    /// <summary>
    /// SelectedColor bindable property
    /// </summary>
    public static readonly BindableProperty SelectedColorProperty
            = BindableProperty.Create(nameof(SelectedColor),
                                       typeof(Color),
                                       typeof(ColorPickerBase<T>),
                                       Color.FromHsla(0, 0.5, 0.5),
                                       propertyChanged: new BindableProperty.BindingPropertyChangedDelegate(HandleSelectedColor)
                                     );

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    static void HandleSelectedColor(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ColorPickerBase<T> colorPickerBase && oldValue != newValue)
            colorPickerBase.UpdateBySelectedColor();
    }
}
