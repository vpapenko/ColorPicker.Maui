namespace ColorPicker.Core;

/// <summary>
/// A 2D point on the unit square [0, 1] × [0, 1] used as the abstract
/// coordinate space for all color-picker math. Callers (rendering layer)
/// map between pixel coordinates and unit coordinates.
///
/// Values are not clamped on construction so that out-of-range inputs can
/// be detected by hit-testing and then projected back with
/// <c>IColorPickerArea.FitToActiveArea</c>.
/// </summary>
public readonly struct UnitPoint : IEquatable<UnitPoint>
{
    public float X { get; }
    public float Y { get; }

    public UnitPoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public UnitPoint WithX(float x) => new(x, Y);
    public UnitPoint WithY(float y) => new(X, y);

    public UnitPoint Translate(float dx, float dy) => new(X + dx, Y + dy);

    /// <summary>Shift this point so the unit-square center (0.5, 0.5) becomes the origin.</summary>
    public UnitPoint ToCentered() => new(X - 0.5f, Y - 0.5f);

    /// <summary>Shift a centered point (origin at center) back to unit-square coordinates.</summary>
    public UnitPoint FromCentered() => new(X + 0.5f, Y + 0.5f);

    public PolarPoint ToPolar() => PolarPoint.FromCartesian(X, Y);

    public bool Equals(UnitPoint other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is UnitPoint other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(UnitPoint a, UnitPoint b) => a.Equals(b);
    public static bool operator !=(UnitPoint a, UnitPoint b) => !a.Equals(b);

    public override string ToString() => $"({X}, {Y})";
}
