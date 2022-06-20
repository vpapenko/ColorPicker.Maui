using Microsoft.Maui.Graphics;

namespace ColorPicker.Calculations
{
    public abstract class ColoPickerCalculationsBase
    {
        public abstract bool IsInActiveAria(PointF point, Color color);
        public abstract PointF FitToActiveAria(PointF point, Color color);
        public abstract Color UpdateColor(PointF point, Color color);
        public abstract PointF ColorToPoint(Color color);

        protected PointF ShiftToCenter(PointF point)
        {
            point.X -= 0.5F;
            point.Y -= 0.5F;
            return point;
        }

        protected PointF ShiftFromCenter(PointF point)
        {
            point.X += 0.5F;
            point.Y += 0.5F;
            return point;
        }
    }
}
