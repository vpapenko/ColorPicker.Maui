namespace ColorPicker.Core;

/// <summary>
/// Pure conversions between RGB, HSL and HSV color spaces.
/// All channels are doubles in [0, 1]. Hue is also [0, 1] (i.e. 0..360° / 360).
///
/// Algorithms follow the standard formulas:
/// https://en.wikipedia.org/wiki/HSL_and_HSV#Color_conversion_formulae
/// </summary>
public static class ColorConversions
{
    public static RgbaColor HslToRgb(HslaColor hsla)
    {
        double h = WrapHue(hsla.H);
        double s = Clamp01(hsla.S);
        double l = Clamp01(hsla.L);

        if (s == 0.0)
            return new RgbaColor(l, l, l, hsla.A);

        double q = l < 0.5 ? l * (1 + s) : l + s - (l * s);
        double p = (2 * l) - q;

        double r = HueToRgb(p, q, h + (1.0 / 3.0));
        double g = HueToRgb(p, q, h);
        double b = HueToRgb(p, q, h - (1.0 / 3.0));

        return new RgbaColor(r, g, b, hsla.A);
    }

    public static HslaColor RgbToHsl(RgbaColor rgba)
    {
        double r = Clamp01(rgba.R);
        double g = Clamp01(rgba.G);
        double b = Clamp01(rgba.B);

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double l = (max + min) / 2.0;

        if (max == min)
            return new HslaColor(0, 0, l, rgba.A);

        double d = max - min;
        double s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        double h;
        if (max == r)
            h = ((g - b) / d) + (g < b ? 6.0 : 0.0);
        else if (max == g)
            h = ((b - r) / d) + 2.0;
        else
            h = ((r - g) / d) + 4.0;
        h /= 6.0;

        return new HslaColor(h, s, l, rgba.A);
    }

    public static RgbaColor HsvToRgb(HsvaColor hsva)
    {
        double h = WrapHue(hsva.H) * 6.0;
        double s = Clamp01(hsva.S);
        double v = Clamp01(hsva.V);

        int i = (int)Math.Floor(h) % 6;
        double f = h - Math.Floor(h);
        double p = v * (1 - s);
        double q = v * (1 - (f * s));
        double t = v * (1 - ((1 - f) * s));

        return i switch
        {
            0 => new RgbaColor(v, t, p, hsva.A),
            1 => new RgbaColor(q, v, p, hsva.A),
            2 => new RgbaColor(p, v, t, hsva.A),
            3 => new RgbaColor(p, q, v, hsva.A),
            4 => new RgbaColor(t, p, v, hsva.A),
            _ => new RgbaColor(v, p, q, hsva.A),
        };
    }

    public static HsvaColor RgbToHsv(RgbaColor rgba)
    {
        double r = Clamp01(rgba.R);
        double g = Clamp01(rgba.G);
        double b = Clamp01(rgba.B);

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double v = max;
        double d = max - min;
        double s = max == 0 ? 0 : d / max;

        double h;
        if (d == 0)
            h = 0;
        else if (max == r)
            h = ((g - b) / d) + (g < b ? 6.0 : 0.0);
        else if (max == g)
            h = ((b - r) / d) + 2.0;
        else
            h = ((r - g) / d) + 4.0;
        h /= 6.0;

        return new HsvaColor(h, s, v, rgba.A);
    }

    public static HsvaColor HslToHsv(HslaColor hsla)
    {
        double l = Clamp01(hsla.L);
        double s = Clamp01(hsla.S);
        double v = l + (s * Math.Min(l, 1 - l));
        double sv = v == 0 ? 0 : 2 * (1 - (l / v));
        return new HsvaColor(hsla.H, sv, v, hsla.A);
    }

    public static HslaColor HsvToHsl(HsvaColor hsva)
    {
        double v = Clamp01(hsva.V);
        double s = Clamp01(hsva.S);
        double l = v * (1 - (s / 2.0));
        double sl = (l == 0 || l == 1) ? 0 : (v - l) / Math.Min(l, 1 - l);
        return new HslaColor(hsva.H, sl, l, hsva.A);
    }

    static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + ((q - p) * 6 * t);
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + ((q - p) * ((2.0 / 3.0) - t) * 6);
        return p;
    }

    static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

    static double WrapHue(double h)
    {
        h %= 1.0;
        return h < 0 ? h + 1.0 : h;
    }
}
