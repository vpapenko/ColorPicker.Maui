namespace ColorPicker.Core;

/// <summary>
/// Polar coordinates with the angle normalized to [-π, π] on construction.
/// </summary>
public readonly struct PolarPoint : IEquatable<PolarPoint>
{
    public float Radius { get; }

    /// <summary>Angle in radians, normalized to [-π, π].</summary>
    public float Angle { get; }

    public PolarPoint(float radius, float angle)
    {
        Radius = radius;
        Angle = Normalize(angle);
    }

    /// <summary>Build a polar point from cartesian coordinates (origin at 0,0).</summary>
    public static PolarPoint FromCartesian(float x, float y)
    {
        var r = (float)Math.Sqrt((x * x) + (y * y));
        var a = (float)Math.Atan2(y, x);
        return new PolarPoint(r, a);
    }

    public PolarPoint WithRadius(float radius) => new(radius, Angle);
    public PolarPoint WithAngle(float angle) => new(Radius, angle);
    public PolarPoint AddAngle(float delta) => new(Radius, Angle + delta);
    public PolarPoint AddRadius(float delta) => new(Radius + delta, Angle);

    /// <summary>Convert back to cartesian. Origin is (0,0); caller is responsible for any centering.</summary>
    public UnitPoint ToCartesian()
    {
        var x = (float)(Radius * Math.Cos(Angle));
        var y = (float)(Radius * Math.Sin(Angle));
        return new UnitPoint(x, y);
    }

    static float Normalize(float angle) =>
        (float)Math.Atan2(Math.Sin(angle), Math.Cos(angle));

    public bool Equals(PolarPoint other) => Radius == other.Radius && Angle == other.Angle;
    public override bool Equals(object? obj) => obj is PolarPoint other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Radius, Angle);
    public static bool operator ==(PolarPoint a, PolarPoint b) => a.Equals(b);
    public static bool operator !=(PolarPoint a, PolarPoint b) => !a.Equals(b);

    public override string ToString() => $"R: {Radius}; A: {Angle}";
}
