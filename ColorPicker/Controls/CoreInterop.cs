using ColorPicker.Core;

namespace ColorPicker.Controls;

internal static class CoreInterop
{
    public static HslaColor ToHsla(this Color c)
        => new(c.GetHue(), c.GetSaturation(), c.GetLuminosity(), c.Alpha);

    public static Color ToMauiColor(this HslaColor h)
        => Color.FromHsla(h.H, h.S, h.L, h.A);

    public static RgbaColor ToRgba(this Color c)
        => new(c.Red, c.Green, c.Blue, c.Alpha);

    public static Color ToMauiColor(this RgbaColor r)
        => Color.FromRgba(r.R, r.G, r.B, r.A);
}
