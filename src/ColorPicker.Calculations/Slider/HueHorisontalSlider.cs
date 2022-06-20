using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.Slider
{
    public class HueHorisontalSlider : SliderBase
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
