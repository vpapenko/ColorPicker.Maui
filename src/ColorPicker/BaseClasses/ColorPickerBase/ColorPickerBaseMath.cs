namespace ColorPicker;

using Microsoft.Maui.Graphics;

public partial class ColorPickerBase : IMathAbstractions
{
    public virtual PointF   ColorToPoint( Color color )                => throw new NotImplementedException();
    public virtual PointF   FitToActiveArea( PointF point )            => throw new NotImplementedException();
    public virtual bool     IsInActiveArea( PointF point )             => throw new NotImplementedException();
    public virtual Color    UpdateColor( PointF point, Color color )   => throw new NotImplementedException();
}
