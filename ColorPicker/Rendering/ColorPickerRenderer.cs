namespace ColorPicker.Rendering;

/// <summary>
/// Bindable base class for renderers. Property changes automatically invalidate
/// every control currently using the renderer.
/// </summary>
public abstract class ColorPickerRenderer : BindableObject, IColorPickerRenderer
{
    readonly WeakEventManager _eventManager = new();

    public event EventHandler? Invalidated
    {
        add => _eventManager.AddEventHandler(value);
        remove => _eventManager.RemoveEventHandler(value);
    }

    public abstract void Render(SKCanvas canvas, ColorPickerDrawingContext context);

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        _eventManager.HandleEvent(this, EventArgs.Empty, nameof(Invalidated));
    }
}
