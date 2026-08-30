namespace ColorPicker.Interfaces;

/// <summary>
/// A color picker that exposes a bindable <see cref="SelectedColor"/> and can be
/// linked to another picker via <see cref="AttachedColorPicker"/> so both edit the
/// same color.
/// </summary>
public interface IColorPicker : INotifyPropertyChanged
{
    /// <summary>The currently selected color. Bindable and two-way.</summary>
    Color SelectedColor { get; set; }

    /// <summary>
    /// Another picker to keep in sync with this one. Links are undirected and
    /// transitive: any number of pickers can be connected, in any order, to form a
    /// group that always shares a single color.
    /// </summary>
    IColorPicker AttachedColorPicker { get; set; }
}
