namespace ColorPicker.Rendering;

/// <summary>Immutable color and position data for a rendered gradient.</summary>
public sealed class ColorGradient
{
    readonly SKColor[] _colors;
    readonly float[]? _positions;

    public ColorGradient(IEnumerable<SKColor> colors, IEnumerable<float>? positions = null)
    {
        ArgumentNullException.ThrowIfNull(colors);
        _colors = colors.ToArray();
        _positions = positions?.ToArray();

        if (_colors.Length == 0)
            throw new ArgumentException("A gradient requires at least one color.", nameof(colors));
        if (_positions is not null && _positions.Length != _colors.Length)
            throw new ArgumentException("Gradient positions must match the number of colors.", nameof(positions));

        Colors = Array.AsReadOnly(_colors);
        Positions = _positions is null ? null : Array.AsReadOnly(_positions);
    }

    /// <summary>Gradient colors in start-to-end order.</summary>
    public IReadOnlyList<SKColor> Colors { get; }

    /// <summary>Optional normalized positions corresponding to <see cref="Colors"/>.</summary>
    public IReadOnlyList<float>? Positions { get; }

    internal SKColor[] ColorArray => _colors;
    internal float[]? PositionArray => _positions;
}
