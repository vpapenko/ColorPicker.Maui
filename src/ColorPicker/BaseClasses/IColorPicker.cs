namespace ColorPicker;

public interface IColorPicker : INotifyPropertyChanged
{
    double          ReticleRadius   { get; set; }
    Color           SelectedColor   { get; set; }
    IColorPicker    AttachedTo      { get; set; }
}