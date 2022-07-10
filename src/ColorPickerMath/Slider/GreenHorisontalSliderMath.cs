using Microsoft.Maui.Graphics;

namespace ColorPickerMath.Slider
{
    public class GreenHorisontalSliderMath : SliderBaseMath
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
