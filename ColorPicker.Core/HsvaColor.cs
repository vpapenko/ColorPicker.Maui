namespace ColorPicker.Core;

/// <summary>
/// HSVA color in [0, 1] × [0, 1] × [0, 1] × [0, 1].
/// Used by the triangle picker, which expresses color in HSV space.
/// </summary>
public readonly struct HsvaColor : IEquatable<HsvaColor>
{
    public double H { get; }
    public double S { get; }
    public double V { get; }
    public double A { get; }

    public HsvaColor(double h, double s, double v, double a = 1.0)
    {
        H = h;
        S = s;
        V = v;
        A = a;
    }

    public HsvaColor WithH(double h) => new(h, S, V, A);
    public HsvaColor WithS(double s) => new(H, s, V, A);
    public HsvaColor WithV(double v) => new(H, S, v, A);
    public HsvaColor WithA(double a) => new(H, S, V, a);

    public RgbaColor ToRgba() => ColorConversions.HsvToRgb(this);
    public HslaColor ToHsla() => ColorConversions.HsvToHsl(this);

    public bool Equals(HsvaColor other) => H == other.H && S == other.S && V == other.V && A == other.A;
    public override bool Equals(object? obj) => obj is HsvaColor other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(H, S, V, A);
    public static bool operator ==(HsvaColor a, HsvaColor b) => a.Equals(b);
    public static bool operator !=(HsvaColor a, HsvaColor b) => !a.Equals(b);

    public override string ToString() => $"hsva({H}, {S}, {V}, {A})";
}
