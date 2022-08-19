namespace ColorPicker;

public class ColorCircleDrawable : PickerBaseDrawable
{
    public ColorCircleDrawable( ColorCircle picker ) : base( picker ) { }

    /// <summary>
    /// Draws the background using a sweep gradient
    /// </summary>
    public override void DrawBackground( ICanvas canvas, RectF dirtyRect )
    {
        DrawSweepGradientInternal( canvas, dirtyRect );
        DrawGrayGradient( canvas, dirtyRect );
    }

    /// <summary>
    /// Draws the reticle
    /// </summary>
    public override void DrawContent( ICanvas canvas, RectF dirtyRect )
    {
        base.DrawReticle( canvas, dirtyRect );
    }

    /// <summary>
    /// Poor man's sweep gradient
    /// </summary>
    void DrawSweepGradientInternal( ICanvas canvas, RectF dirtyRect )
    {
        var countOfSectors = 512;
        var colors = new Color[ countOfSectors ];

        for ( var i = 0; i < countOfSectors; i++ )
            colors[ i ] = Color.FromHsv( (float)i / countOfSectors, 1f, 1f );

        DrawSweepGradient( canvas, dirtyRect, CreateSweepGradient( colors ) );
    }

    /// <summary>
    /// Support creation of sweep gradient array
    /// </summary>
    /// <returns></returns>
    public SweepInfo[] CreateSweepGradient( Color[] colors )
    {
        var infoList = new List<SweepInfo>();
        var sectorCount = colors.Length;

        for ( var i = 0; i < sectorCount; i++ )
        {
            infoList.Add( new SweepInfo
            {
                SweepColor = colors[ i ],
                Angle1 = ( MathF.PI * 2 * i / sectorCount ) - ( MathF.PI / sectorCount ) + MathF.PI,
                Angle2 = ( MathF.PI * 2 * i / sectorCount ) + ( MathF.PI / sectorCount ) + ( MathF.PI / ( sectorCount / 10 ) ) + MathF.PI
            } );

        }

        return infoList.ToArray();
    }

    /// <summary>
    /// Draw sweep gradients
    /// </summary>
    public void DrawSweepGradient( ICanvas canvas, RectF dirtyRect, SweepInfo[] infoArray )
    {
        var sectorCount = infoArray.Length;

        canvas.SaveState();

        for ( var i = 0; i < sectorCount; i++ )
            DrawSector( canvas, dirtyRect, infoArray[ i ] );

        canvas.RestoreState();
    }

    void DrawSector( ICanvas canvas, RectF dirtyRect, SweepInfo info )
    {
        var path    = new PathF( dirtyRect.Center );

        path.AddArc( new PointF( 0, 0 ),
                     new PointF( dirtyRect.Width, dirtyRect.Height ),
                     info.Angle1 / ( 2 * MathF.PI ) * 360,
                     info.Angle2 / ( 2 * MathF.PI ) * 360, false );

        canvas.FillColor = info.SweepColor;
        canvas.FillPath( path );
    }

    void DrawGrayGradient( ICanvas canvas, RectF dirtyRect )
    {
        canvas.SaveState();

        var radialGradientPaint = new RadialGradientPaint
        {
            StartColor  = Colors.Gray,
            EndColor    = Colors.Transparent
        };

        canvas.SetFillPaint( radialGradientPaint, dirtyRect );
        canvas.FillRoundedRectangle( dirtyRect, dirtyRect.Center.X );

        canvas.RestoreState();
    }
}

public struct SweepInfo
{
    internal Color SweepColor { get; set; }
    internal float Angle1 { get; set; }
    internal float Angle2 { get; set; }
}

