using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.Slider
{
    public class SaturationHorisontalSlider : SliderBase
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromHsla(color.GetHue(), newValue, color.GetLuminosity(), color.Alpha);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.GetSaturation();
        }
    }
}