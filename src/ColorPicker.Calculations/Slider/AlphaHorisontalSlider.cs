using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations.Slider
{
    public class AlphaHorisontalSlider : SliderBase
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromRgba(color.Red, color.Green, color.Blue, newValue);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.Alpha;
        }
    }
}
