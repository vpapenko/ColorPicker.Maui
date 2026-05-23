namespace ColorPicker.Core;

/// <summary>
/// A 1-D track on the unit square, used by all linear sliders. The track
/// occupies the full extent of the chosen axis; the cross-axis is fixed at
/// 0.5. Values are normalized [0, 1] where 0 is the start of the track and
/// 1 is the end.
/// </summary>
public readonly struct LinearTrack
{
    public bool Vertical { get; }

    public LinearTrack(bool vertical) { Vertical = vertical; }

    public double ValueAt(UnitPoint p)
    {
        double v = Vertical ? p.Y : p.X;
        return v < 0 ? 0 : v > 1 ? 1 : v;
    }

    public UnitPoint PointFor(double value)
    {
        float v = value < 0 ? 0f : value > 1 ? 1f : (float)value;
        return Vertical ? new UnitPoint(0.5f, v) : new UnitPoint(v, 0.5f);
    }
}
