namespace ColorPicker;

public class PickerBase : ColorPickerBase
{
    #region PickerRadiusScale implementation
    /// <summary>
    /// PickerRadiusScale bindable property
    /// </summary>
    public static readonly BindableProperty PickerRadiusScaleProperty
            = BindableProperty.Create( nameof(PickerRadiusScale),
                                       typeof(float),
                                       typeof(IColorPicker),
                                       0.05f,
                                propertyChanged: (bindable, oldValue, newValue) =>
                                {
                                    if ( newValue is not null && bindable is ColorPickerBase colorPickerBase )
                                        colorPickerBase.Invalidate();
                                } );

    public float PickerRadiusScale
    {
        get => (float)GetValue( PickerRadiusScaleProperty );
        set => SetValue( PickerRadiusScaleProperty, value );
    }
    #endregion












}
