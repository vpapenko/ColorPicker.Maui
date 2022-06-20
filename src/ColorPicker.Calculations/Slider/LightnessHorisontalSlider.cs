using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.Slider
{
    public class LightnessHorisontalSlider : SliderBase
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromHsla(color.GetHue(), color.GetSaturation(), newValue, color.Alpha);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.GetLuminosity();
        }
    }
}
