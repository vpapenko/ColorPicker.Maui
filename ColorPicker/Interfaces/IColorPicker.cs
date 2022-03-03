namespace ColorPicker.Interfaces;

public interface IColorPicker : INotifyPropertyChanged
{
    Color        SelectedColor          { get; set; }
    IColorPicker AttachedColorPicker    { get; set; }
}
