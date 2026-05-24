namespace ColorPicker.Core.Tests;

/// <summary>
/// Reference implementation: mirrors MAUI ColorTriangleArea pixel-space math
/// verbatim. Used to cross-validate the unit-square port in
/// SaturationValueTriangle. If MAUI changes, this file must change too —
/// it is the contract we are porting.
/// </summary>
static class MauiTriangleReference
{
    public const float TriangleHeight = 1.5000001F;
    public const float TriangleSide = 0.8660244F;
    public const float TriangleVerticalOffset = 0.5000001F;

    // Pixel-space encode (analogous to ColorTriangleArea.UpdateLocations).
    // Returns pixel point centered on (canvasRadius, canvasRadius).
    public static (float x, float y) EncodeSvPixel(
        double s, double v, double hue, float canvasRadius, float svRadius, bool rotateByHue)
    {
        double lumX = TriangleSide * (1 - 2 * s);
        double lumY = TriangleHeight;
        double r = Math.Sqrt(lumX * lumX + lumY * lumY) * v;
        double a = Math.Atan2(lumY, lumX);

        double x = r * Math.Cos(a);
        double y = r * Math.Sin(a);
        x = -x;
        y -= 1;
        x *= svRadius;
        y *= svRadius;

        double rotR = Math.Sqrt(x * x + y * y);
        double rotA = Math.Atan2(y, x) - (2.0 * Math.PI / 3.0);
        x = rotR * Math.Cos(rotA);
        y = rotR * Math.Sin(rotA);

        if (rotateByHue)
        {
            double hr = Math.Sqrt(x * x + y * y);
            double ha = Math.Atan2(y, x) - ((2.0 * Math.PI * hue) + (Math.PI / 2.0));
            x = hr * Math.Cos(ha);
            y = hr * Math.Sin(ha);
        }

        return ((float)(x + canvasRadius), (float)(y + canvasRadius));
    }

    // Pixel-space decode (analogous to ColorTriangleArea.WheelPointToColor's
    // SV-extraction portion). Returns (s, v).
    public static (double s, double v) DecodeSvPixel(
        float px, float py, double hue, float canvasRadius, float svRadius, bool rotateByHue)
    {
        double x = (px - canvasRadius) / svRadius;
        double y = (py - canvasRadius) / svRadius;

        if (rotateByHue)
        {
            double r = Math.Sqrt(x * x + y * y);
            double a = Math.Atan2(y, x) + ((2.0 * Math.PI * hue) + (Math.PI / 2.0));
            x = r * Math.Cos(a);
            y = r * Math.Sin(a);
        }

        double svX = x + TriangleSide;
        double svY = -y + TriangleVerticalOffset;

        const double x1 = TriangleSide;
        const double y1 = TriangleHeight;
        const double x2 = x1 * 2;
        const double y2 = 0.0;

        double vCurrent = ((svX * (y2 - y1)) - (svY * (x2 - x1)) + (x2 * y1) - (y2 * x1))
                          / Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));
        double v = (y1 - vCurrent) / y1;
        double sMax = x2 - (vCurrent / Math.Sin(Math.PI / 3.0));
        double sCurrent = svY / Math.Sin(Math.PI / 3.0);
        double s = sMax == 0 ? 0 : sCurrent / sMax;

        return (Math.Clamp(s, 0, 1), Math.Clamp(v, 0, 1));
    }
}
