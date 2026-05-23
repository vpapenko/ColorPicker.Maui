namespace ColorPicker.Core;

/// <summary>
/// HSLA color in [0, 1] × [0, 1] × [0, 1] × [0, 1].
/// Pure value type, no framework dependencies.
/// </summary>
public readonly struct HslaColor : IEquatable<HslaColor>
{
    public double H { get; }
    public double S { get; }
    public double L { get; }
    public double A { get; }

    public HslaColor(double h, double s, double l, double a = 1.0)
    {
        H = h;
        S = s;
        L = l;
        A = a;
    }

    public HslaColor WithH(double h) => new(h, S, L, A);
    public HslaColor WithS(double s) => new(H, s, L, A);
    public HslaColor WithL(double l) => new(H, S, l, A);
    public HslaColor WithA(double a) => new(H, S, L, a);

    public RgbaColor ToRgba() => ColorConversions.HslToRgb(this);
    public HsvaColor ToHsva() => ColorConversions.HslToHsv(this);

    public static HslaColor FromRgba(RgbaColor rgba) => ColorConversions.RgbToHsl(rgba);
    public static HslaColor FromHsva(HsvaColor hsva) => ColorConversions.HsvToHsl(hsva);

    public bool Equals(HslaColor other) => H == other.H && S == other.S && L == other.L && A == other.A;
    public override bool Equals(object? obj) => obj is HslaColor other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(H, S, L, A);
    public static bool operator ==(HslaColor a, HslaColor b) => a.Equals(b);
    public static bool operator !=(HslaColor a, HslaColor b) => !a.Equals(b);

    public override string ToString() => $"hsla({H}, {S}, {L}, {A})";
}
