using Microsoft.Maui.Graphics;

namespace ColorPickerMath.Slider
{
    public class RedHorisontalSliderMath : SliderBaseMath
    {
        public override Color UpdateColor(PointF point, Color color)
        {
            var newValue = GetSliderValue(point, color);
            return Color.FromRgba(newValue, color.Green, color.Blue, color.Alpha);
        }

        protected override float GetSliderValue(Color color)
        {
            return color.Red;
        }
    }
}
