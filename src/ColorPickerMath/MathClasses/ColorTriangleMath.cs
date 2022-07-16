﻿using ColorMine.ColorSpaces;

namespace ColorPickerMath;

public class ColorTriangleMath : ColorPickerMathBase
{
    const float triangleHeight = 0.75f;
    const float triangleSide = 0.8660254f;
    float lastHue = 0;

    public float Rotation { get; set; } = 0.523599f;

    public override PointF ColorToPoint( Color color )
    {
        HSLToHSV( color, out var _, out var saturation, out var value );
        saturation -= 0.5f;
        saturation *= triangleSide * value;
        var point = new PointF(value * triangleHeight, saturation);
        point.Y = -point.Y;
        return Rotate( point, Rotation ).AddY( 0.5f );
    }

    public override PointF FitToActiveArea( PointF point, Color color )
    {
        var result = Rotate( point.Clone().AddY(-0.5f), -Rotation );
        result.X = result.X.Clamp( 0, triangleHeight );
        var maxY = triangleSide * (result.X / triangleHeight) / 2;
        result.Y = result.Y.Clamp( -maxY, maxY );
        return Rotate( result, Rotation ).AddY( 0.5f );
    }

    public override bool IsInActiveArea( PointF point, Color color )
    {
        var p = Rotate(point.Clone().AddY(-0.5f), Rotation);
        if ( p.X > triangleHeight )
            return false;
        var MaxY = triangleSide * p.X;
        return Math.Abs( p.Y ) < MaxY;
    }

    public override Color UpdateColor( PointF point, Color color )
    {
        var p = FitToActiveArea(point, color);
        p = Rotate( p.AddY( -0.5f ), -Rotation );
        p.Y = -p.Y;
        var value = p.X / triangleHeight;
        var maxY = triangleSide * value;
        var y = p.Y + maxY / 2;
        var saturation = y / maxY;
        return Color.FromHsva( GetHue( color ), saturation, value, color.Alpha );
    }

    public static void HSLToHSV( Color color, out float h, out float s, out float v )
    {
        var hsl = new Hsl { H = color.GetHue(), S = color.GetSaturation(), L = color.GetLuminosity() };
        var hsv = hsl.To<Hsv>();
        h = (float)hsv.H;
        s = (float)hsv.S;
        v = (float)hsv.V;
    }

    PointF Rotate( PointF point, float angle )
    {
        point.AddX( -0.5f );
        var polar = point.ToPolarPoint();
        polar.AddAngle( angle );
        var p = polar.ToPointF();
        point.X = p.X;
        point.Y = p.Y;
        point.AddX( 0.5f );
        return point;
    }

    float GetHue( Color color )
    {
        HSLToHSV( color, out var _, out var saturation, out var _ );
        var hue = saturation > 0 ? color.GetHue() : lastHue;
        lastHue = saturation <= 0 ? lastHue : color.GetHue();
        return hue;
    }
}
