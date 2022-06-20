using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.Slider
{
    public class GreenHorisontalSlider : SliderBase
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromRgba(color.Red, newValue, color.Blue, color.Alpha);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.Green;
        }
    }
}
