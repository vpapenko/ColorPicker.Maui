﻿namespace ColorPicker;

public class ColorCircle : ColorPickerBase<ColorCircleMath>
{
    public ColorCircle()
    {
        SetAspectRatio = SetAspectRatioSquare;
    }

    protected override void DrawBackground( ICanvas canvas, RectF dirtyRect )
    {
        DrawSweepGradient( canvas, dirtyRect );
        DrawGrayGradient( canvas, dirtyRect );
    }

    static void DrawGrayGradient( ICanvas canvas, RectF dirtyRect )
    {
        var radialGradientPaint = new RadialGradientPaint
        {
            StartColor  = Colors.Gray,
            EndColor    = Colors.Transparent
        };
        canvas.SetFillPaint( radialGradientPaint, dirtyRect );
        canvas.FillRoundedRectangle( dirtyRect, dirtyRect.Center.X );
    }

    void DrawSweepGradient( ICanvas canvas, RectF dirtyRect )
    {
        var countOfSectors = 512;

        for ( var i = 0; i <= countOfSectors; i++ )
        {
            var angle1  = (MathF.PI * 2 * i / countOfSectors) - (MathF.PI / countOfSectors) + MathF.PI;
            var angle2  = (MathF.PI * 2 * i / countOfSectors) + (MathF.PI / countOfSectors) + (MathF.PI / (countOfSectors / 10)) + MathF.PI;

            var color   = Color.FromHsv((float)i / countOfSectors, 1f, 1f);

            DrawSector( angle1, angle2, color, canvas, dirtyRect );
        }
    }

    void DrawSector( float angle1, float angle2, Color color, ICanvas canvas, RectF dirtyRect )
    {
        var center  = dirtyRect.Center;
        var path    = new PathF(center);

        path.AddArc( new PointF( 0, 0 ),
                     new PointF( dirtyRect.Width, dirtyRect.Height ), angle1 / ( 2 * MathF.PI ) * 360, angle2 / ( 2 * MathF.PI ) * 360, false );

        canvas.FillColor = color;
        canvas.FillPath( path );
    }
}