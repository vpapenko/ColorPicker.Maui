using ColorPicker.Calculations;

namespace ColorPicker.Maui
{
    public abstract class ColorPickerBase<T> : GraphicsView
        where T: ColoPickerCalculationsBase, new()
    {
        private readonly ColoPickerCalculationsBase _coloPickerCalculations = null;
        private PointF _pickerLocation;

        public Color SelectedColor = Colors.Green;

        public ColorPickerBase()
        {
            _coloPickerCalculations = new T();
            Drawable = new ColorPickerBaseDrowable(DrawBackground, DrawPicker);
            StartInteraction += OnStartInteraction;
            DragInteraction += OnDragInteraction;
            EndInteraction += OnEndInteraction;
            UpdateBySelectedColor();
        }

        protected abstract void DrawBackground(ICanvas canvas, RectF dirtyRect);

        protected void DrawPicker(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeSize = 1;
            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_pickerLocation, 10);
            canvas.StrokeColor = Colors.Black;
            canvas.DrawCircle(_pickerLocation, 11);
            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_pickerLocation, 12);
        }

        protected override Size ArrangeOverride(Rect bounds)
        {
            UpdateBySelectedColor();
            return base.ArrangeOverride(bounds);
        }

        private void OnEndInteraction(object sender, TouchEventArgs e)
        {
            UpdateColor(e.Touches[0]);
        }

        private void OnDragInteraction(object sender, TouchEventArgs e)
        {
            UpdateColor(e.Touches[0]);
        }

        private void OnStartInteraction(object sender, TouchEventArgs e)
        {
            UpdateColor(e.Touches[0]);
        }

        private void UpdateColor(PointF pointF)
        {
            SelectedColor = _coloPickerCalculations.UpdateColor(ScalePoint(pointF), SelectedColor);
            _pickerLocation = UnscalePoint(_coloPickerCalculations.ColorToPoint(SelectedColor));
            this.Invalidate();
        }

        private void UpdateBySelectedColor()
        {
            _pickerLocation = UnscalePoint(_coloPickerCalculations.ColorToPoint(SelectedColor));
            this.Invalidate();
        }

        private PointF ScalePoint(PointF point)
        {
            return new PointF(point.X / (float)this.Width, point.Y / (float)this.Height);
        }

        private PointF UnscalePoint(PointF point)
        {
            return new PointF(point.X * (float)this.Width, point.Y * (float)this.Height);
        }
    }
}