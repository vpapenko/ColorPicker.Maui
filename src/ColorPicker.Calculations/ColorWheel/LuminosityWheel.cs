using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.ColorWheel
{
    public class LuminosityWheel : ColoPickerCalculationsBase
    {
        private float _internalRadius = 0.25F;

        public LuminosityWheel()
        {
        }

        public LuminosityWheel(float internalRadius)
        {
            InternalRadius = internalRadius;
        }

        private float MiddleRadius => (0.5F - _internalRadius) / 2 + _internalRadius;
        public float Rotation { get; set; }
        public bool IsLeftSide { get; protected set; } = true;

        public float InternalRadius
        {
            get
            {
                return _internalRadius;
            }
            set
            {
                value = value < 0 ? 0 : value;
                value = value > 0.5F ? 0.5F : value;
                _internalRadius = value;
            }
        }

        public override PointF ColorToPoint(Color color)
        {
            var side = IsLeftSide ? 1 : -1;
            var r = MiddleRadius;
            var a = color.GetLuminosity() * (float)Math.PI + side * (float)Math.PI / 2 - Rotation;
            var polar = new PolarPoint(r, a);
            var point = polar.ToAbstractPoint();
            point.Y = -1 * side * point.Y;
            point = ShiftFromCenter(point);
            return point;
        }

        public override PointF FitToActiveAria(PointF point, Color color)
        {
            var polar = ToPolarPoint(point);
            point = polar.ToAbstractPoint();
            point = ShiftFromCenter(point);
            return point;
        }

        public override bool IsInActiveAria(PointF point, Color color)
        {
            point = ShiftToCenter(point);
            var polar = point.ToPolarPoint();
            return polar.Radius <= 0.5 && polar.Radius >= _internalRadius;
        }

        public override Color UpdateColor(PointF point, Color color)
        {
            UpdateSide(point);
            var polar = ToPolarPoint(point);
            polar.Angle += Rotation + (float)Math.PI / 2F;
            polar = polar.ToAbstractPoint().ToPolarPoint();
            var l = Math.Abs(polar.Angle) / Math.PI;
            return Color.FromHsla(color.GetHue(), color.GetSaturation(), l, color.Alpha);
        }

        private void UpdateSide(PointF point)
        {
            point = ShiftToCenter(point);
            IsLeftSide = point.X < 0;
        }

        private PolarPoint ToPolarPoint(PointF point)
        {
            point = ShiftToCenter(point);
            return new PolarPoint(point)
            {
                Radius = MiddleRadius
            };
        }
    }
}
