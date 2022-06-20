using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.ColorTriangle
{
    public class RotatingColorTriangle : ColoPickerCalculationsBase
    {
        private readonly ColorTriangle _colorTriangle = new ColorTriangle();
        private float lastHue = 0;

        public override PointF ColorToPoint(Color color)
        {
            SetAngle(color);
            return _colorTriangle.ColorToPoint(color);
        }

        public override PointF FitToActiveAria(PointF point, Color color)
        {
            SetAngle(color);
            return _colorTriangle.FitToActiveAria(point, color);
        }

        public override bool IsInActiveAria(PointF point, Color color)
        {
            SetAngle(color);
            return _colorTriangle.IsInActiveAria(point, color);
        }

        public override Color UpdateColor(PointF point, Color color)
        {
            SetAngle(color);
            return _colorTriangle.UpdateColor(point, color);
        }

        private void SetAngle(Color color)
        {
            ColorTriangle.HSLToHSV(color, out float _, out float saturation, out float _);
            var hue = saturation > 0 ? color.GetHue() : lastHue;
            lastHue = saturation <= 0 ? lastHue : color.GetHue();
            _colorTriangle.Rotation = -2.094395f - hue * 2f * (float)Math.PI;
        }
    }
}
