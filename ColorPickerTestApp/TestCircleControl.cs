namespace ColorPickerTestApp;

using ColorPicker.BaseClasses;
using ColorPicker.Behaviors;
using System.Diagnostics;

public class TestCircleControl : SkiaSharpPickerBase
{
    public TestCircleControl()
    {
        Debug.WriteLine( "[TestCircleControl] Constructor called" );
        Debug.WriteLine( $"[TestCircleControl] Children.Count={Children.Count}" );
    }

    public override float GetPickerRadiusPixels() => GetPickerRadiusPixels( GetCanvasSize() );
    public override float GetPickerRadiusPixels( SKSize canvasSize ) => GetSize( canvasSize ) * PickerRadiusScale;

    protected override void OnSelectedColorChanging( Color color )
    {
        Debug.WriteLine( $"[TestCircleControl] OnSelectedColorChanging color={color}" );
        InvalidateSurface();
    }

    protected override SizeRequest GetMeasure( double widthConstraint, double heightConstraint )
    {
        if ( double.IsPositiveInfinity( widthConstraint ) &&
             double.IsPositiveInfinity( heightConstraint ) )
        {
            widthConstraint  = 300;
            heightConstraint = 300;
        }

        var size = Math.Min( widthConstraint, heightConstraint );
        Debug.WriteLine( $"[TestCircleControl] GetMeasure w={widthConstraint} h={heightConstraint} -> size={size}" );
        return new SizeRequest( new Size( size, size ) );
    }

    protected override float GetSize( SKSize canvasSize )
    {
        var s = Math.Min( canvasSize.Width, canvasSize.Height );
        Debug.WriteLine( $"[TestCircleControl] GetSize canvasSize={canvasSize} -> {s}" );
        return s;
    }
    protected override float GetSize() => GetSize( GetCanvasSize() );

    protected override void OnPaintSurface( SKCanvas canvas, int width, int height )
    {
        Debug.WriteLine( $"[TestCircleControl] OnPaintSurface width={width} height={height}" );

        canvas.Clear( SKColors.DarkGray );

        var centerX = width / 2f;
        var centerY = height / 2f;
        var radius  = Math.Min( centerX, centerY ) - 20;

        Debug.WriteLine( $"[TestCircleControl] Drawing circle at ({centerX},{centerY}) radius={radius}" );

        // Red filled circle
        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Fill,
            Color       = SKColors.Red
        };
        canvas.DrawCircle( centerX, centerY, radius, fillPaint );

        // White border
        using var strokePaint = new SKPaint
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.White,
            StrokeWidth = 4
        };
        canvas.DrawCircle( centerX, centerY, radius, strokePaint );

        // Diagonal cross lines for visibility
        using var linePaint = new SKPaint
        {
            IsAntialias = true,
            Style       = SKPaintStyle.Stroke,
            Color       = SKColors.Yellow,
            StrokeWidth = 3
        };
        canvas.DrawLine( centerX - radius, centerY - radius, centerX + radius, centerY + radius, linePaint );
        canvas.DrawLine( centerX - radius, centerY + radius, centerX + radius, centerY - radius, linePaint );

        // Label
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color       = SKColors.White,
            TextSize    = 32,
            TextAlign   = SKTextAlign.Center
        };
        canvas.DrawText( "SkiaSharp OK", centerX, centerY + 10, textPaint );
    }

    protected override void OnTouchActionPressed( ColorPickerTouchActionEventArgs args ) { }
    protected override void OnTouchActionMoved( ColorPickerTouchActionEventArgs args ) { }
    protected override void OnTouchActionReleased( ColorPickerTouchActionEventArgs args ) { }
    protected override void OnTouchActionCancelled( ColorPickerTouchActionEventArgs args ) { }
}
