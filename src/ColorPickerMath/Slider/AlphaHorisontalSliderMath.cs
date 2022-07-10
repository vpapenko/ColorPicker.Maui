using ColorPickerMath.Slider;
using Microsoft.Maui.Graphics;

namespace ColorPickerMath.Slider
{
    public class AlphaHorisontalSliderMath : SliderBaseMath
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
