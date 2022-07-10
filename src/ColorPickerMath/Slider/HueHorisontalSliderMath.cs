using Microsoft.Maui.Graphics;

namespace ColorPickerMath.Slider
{
    public class HueHorisontalSliderMath : SliderBaseMath
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromHsla(newValue, color.GetSaturation(), color.GetLuminosity(), color.Alpha);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.GetHue();
        }
    }
}
