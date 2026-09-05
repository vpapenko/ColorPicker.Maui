namespace ColorPicker.Rendering;

/// <summary>
/// Draws semantic color-picker elements. Implement this interface for a complete
/// renderer, or inherit a bundled renderer to replace selected elements.
/// </summary>
public interface IColorPickerRenderer
{
    /// <summary>Raised when renderer properties change and subscribed controls should repaint.</summary>
    event EventHandler? Invalidated;

    /// <summary>
    /// Draw one semantic element using the supplied immutable context. Implementations
    /// should ignore unknown context subclasses for forward compatibility.
    /// </summary>
    void Render(SKCanvas canvas, ColorPickerDrawingContext context);
}
