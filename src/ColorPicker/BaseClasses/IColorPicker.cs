namespace ColorPicker;

public interface IColorPicker
{
    double          ReticleRadius   { get; set; }
    Color           SelectedColor   { get; set; }
    IColorPicker    AttachedTo      { get; set; }
}