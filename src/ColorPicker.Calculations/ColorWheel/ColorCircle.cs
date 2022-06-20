using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.ColorWheel
{
    public class ColorCircle : ColoPickerCalculationsBase
    {
        public float Rotation { get; set; }

        public override PointF ColorToPoint(Color color)
        {
            var r = color.GetSaturation() / 2f;
            var a = color.GetHue() * 2f * (float)Math.PI - Rotation;
            var polar = new PolarPoint(r, a);
            var point = polar.ToAbstractPoint();
            point.X = -point.X;
            point = ShiftFromCenter(point);
            return point;
        }

        public override PointF FitToActiveAria(PointF point, Color color)
        {
            point = ShiftToCenter(point);
            var polar = point.ToPolarPoint();
            polar.Radius = polar.Radius > 0.5F ? 0.5F : polar.Radius;
            point = polar.ToAbstractPoint();
            point = ShiftFromCenter(point);
            return point;
        }

        public override bool IsInActiveAria(PointF point, Color color)
        {
            point = ShiftToCenter(point);
            var polar = point.ToPolarPoint();
            return polar.Radius <= 1;
        }

        public override Color UpdateColor(PointF point, Color color)
        {
            point = FitToActiveAria(point, color);
            point = ShiftToCenter(point);
            point.Y = -point.Y;
            var polar = point.ToPolarPoint();
            polar.Angle += Rotation;
            polar = polar.ToAbstractPoint().ToPolarPoint();
            var h = (polar.Angle + Math.PI) / (Math.PI * 2);
            var s = polar.Radius * 2;
            return Color.FromHsla(h, s, color.GetLuminosity(), color.Alpha);
        }
    }
}
