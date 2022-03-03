namespace ColorPickerTest;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();

    void OnPaintSurface( object sender, SKPaintSurfaceEventArgs e )
    {
        if ( sender is not SKCanvasView canvasView )
        {
            throw new InvalidOperationException( "Not a SKCanvasView" );
        }

        var scale       = 21F;

        var canvas = e.Surface.Canvas;
        SKPath path     = new();

        path.MoveTo( -1 * scale, -1 * scale );
        path.LineTo(  0 * scale, -1 * scale );
        path.LineTo(  0 * scale,  0 * scale );
        path.LineTo(  1 * scale,  0 * scale );
        path.LineTo(  1 * scale,  1 * scale );
        path.LineTo(  0 * scale,  1 * scale );
        path.LineTo(  0 * scale,  0 * scale );
        path.LineTo( -1 * scale,  0 * scale );
        path.LineTo( -1 * scale, -1 * scale );

        var matrix = SKMatrix.CreateScale( 2 * scale, 2 * scale );

        SKPaint paint = new()
        {
            PathEffect  = SKPathEffect.Create2DPath( matrix, path ),
            Color       = Colors.LightSkyBlue.ToSKColor(),
            IsAntialias = true
        };

        var patternRect = new SKRect( 0, 0, canvasView.CanvasSize.Width, canvasView.CanvasSize.Height );

        canvas.Save();
        canvas.DrawRect( patternRect, paint );
        canvas.Restore();
    }
}
