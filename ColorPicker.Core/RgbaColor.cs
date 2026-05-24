namespace ColorPicker.Core;

/// <summary>
/// RGBA color with each channel in [0, 1].
/// </summary>
public readonly struct RgbaColor : IEquatable<RgbaColor>
{
    public double R { get; }
    public double G { get; }
    public double B { get; }
    public double A { get; }

    public RgbaColor(double r, double g, double b, double a = 1.0)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public HslaColor ToHsla() => ColorConversions.RgbToHsl(this);
    public HsvaColor ToHsva() => ColorConversions.RgbToHsv(this);

    public RgbaColor WithR(double r) => new(r, G, B, A);
    public RgbaColor WithG(double g) => new(R, g, B, A);
    public RgbaColor WithB(double b) => new(R, G, b, A);
    public RgbaColor WithA(double a) => new(R, G, B, a);

    public bool Equals(RgbaColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object? obj) => obj is RgbaColor other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(RgbaColor a, RgbaColor b) => a.Equals(b);
    public static bool operator !=(RgbaColor a, RgbaColor b) => !a.Equals(b);

    public override string ToString() => $"rgba({R}, {G}, {B}, {A})";
}
