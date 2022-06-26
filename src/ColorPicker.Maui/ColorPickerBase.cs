using ColorPicker.Calculations;

namespace ColorPicker.Maui
{
    public abstract class ColorPickerBase<T> : GraphicsView
        where T: ColoPickerCalculationsBase, new()
    {
        private readonly T _coloPickerCalculations = null;
        private PointF _pickerLocation;
        protected Action<double,double> _setAspectRatio;

        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
           nameof(SelectedColor),
           typeof(Color),
           typeof(ColorPickerBase<T>),
           Color.FromHsla(0, 0.5, 0.5),
           propertyChanged: new BindableProperty.BindingPropertyChangedDelegate(HandleSelectedColorSet));

        static void HandleSelectedColorSet(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                ((ColorPickerBase<T>)bindable).UpdateBySelectedColor();
            }
        }

        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public double PickerRadius { get; set; } = 20;

        public ColorPickerBase()
        {
            _setAspectRatio = SetAspectRatioToPickerHeigth;
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
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_pickerLocation, PickerRadius);
            canvas.StrokeColor = Colors.Black;
            canvas.DrawCircle(_pickerLocation, PickerRadius - 2);
            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(_pickerLocation, PickerRadius - 4);
        }

        protected void SetAspectRatioToPickerHeigth(double widthConstraint, double heightConstraint)
        {
            this.HeightRequest = PickerRadius * 2 + 2;
        }

        protected void SetAspectRatioSquare(double widthConstraint, double heightConstraint)
        {
            var minConstraint = Math.Min(widthConstraint, heightConstraint);
            this.WidthRequest = minConstraint;
            this.HeightRequest = minConstraint;
        }

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            _setAspectRatio(widthConstraint,heightConstraint);
            return base.MeasureOverride(widthConstraint, widthConstraint);
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