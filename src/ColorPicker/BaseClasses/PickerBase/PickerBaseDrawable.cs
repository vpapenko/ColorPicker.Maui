namespace ColorPicker;

public class PickerBaseDrawable : ColorPickerBaseDrawable
{
    public PickerBaseDrawable( PickerBase picker ) : base( picker ) { }

    //  Defers to subclass
    public override void DrawBackground( ICanvas canvas, RectF dirtyRect ) { }

    //  Draws a reticle by default
    public override void DrawContent( ICanvas canvas, RectF dirtyRect )
    {
        DrawReticle( canvas, dirtyRect );
    }
}
