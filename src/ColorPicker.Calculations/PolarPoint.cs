using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations
{
    public class PolarPoint
    {
        public PolarPoint(float radius, float angle)
        {
            Radius = radius;
            Angle = angle;
        }

        public PolarPoint(PointF point)
        {
            float radius = (float)Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
            float angle = (float)Math.Atan2(point.Y, point.X);
            Radius = radius;
            Angle = angle;
        }

        public float Radius { get; set; }

        float _angel;
        public float Angle
        {
            get
            {
                return _angel;
            }
            set
            {
                _angel = (float)Math.Atan2(Math.Sin(value), Math.Cos(value));
            }
        }

        public PolarPoint AddAngle(float angel)
        {
            Angle += angel;
            return this;
        }

        public PolarPoint AddRadius(float radius)
        {
            Radius += radius;
            return this;
        }

        public PointF ToPoint()
        {
            float x = Radius * (float)Math.Cos(Angle);
            float y = Radius * (float)Math.Sin(Angle);
            return new PointF(x, y);
        }

        public override string ToString()
        {
            return string.Format("R: {0}; A: {1}", Radius, Angle);
        }
    }
}
