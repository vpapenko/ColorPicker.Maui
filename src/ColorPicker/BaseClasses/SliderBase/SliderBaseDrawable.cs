namespace ColorPicker;

public class SliderBaseDrawable : ColorPickerBaseDrawable
{
    public SliderBaseDrawable( SliderBase slider ) : base( slider )
    {
    }

    //  Must override
    public override void DrawBackground( ICanvas canvas, RectF dirtyRect )
            => throw new NotImplementedException();

    //  May override - default draws a reticle
    public override void DrawContent( ICanvas canvas, RectF dirtyRect )
    {
    }
}
