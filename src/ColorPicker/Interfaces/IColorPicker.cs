namespace ColorPicker;

public interface IColorPicker : INotifyPropertyChanged
{
    Color           SelectedColor   { get; set; }
    IColorPicker    AttachedTo      { get; set; }
}