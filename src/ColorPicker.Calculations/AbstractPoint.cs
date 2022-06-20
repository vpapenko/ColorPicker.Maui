using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations
{
    public static class PointFExtensions
    {
        public static PointF AddX(this PointF point, float x)
        {
            point.X += x;
            return point;
        }

        public static PointF AddY(this PointF point, float y)
        {
            point.Y += y;
            return point;
        }

        public static PolarPoint ToPolarPoint(this PointF point)
        {
            return new PolarPoint(point);
        }

        public static PointF Clone(this PointF point)
        {
            return new PointF(point.X, point.Y);
        }
    }
}
