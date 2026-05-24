using ColorPicker.Core;
using ColorPicker.Core.Interaction;

namespace ColorPicker.Core.Tests;

/// <summary>
/// Same bug-class guard as <see cref="TriangleAreaInteractionTests"/> but
/// for ColorDisc. Dragging the L-ring must not budge H or S; dragging the
/// HS disc must not budge L.
/// </summary>
public class ColorDiscInteractionTests
{
    static UnitPoint LRingPoint(double l01, bool rightSide = true)
    {
        // From LuminosityRing.cs: L = 0 at top, 0.5 at bottom; the encoded
        // angle is (1 - 2*L) * pi/2 for the right half, mirrored on the left.
        double sinA = 1.0 - 2.0 * l01;
        sinA = Math.Max(-1.0, Math.Min(1.0, sinA));
        double angle = Math.Asin(sinA); // [-pi/2, pi/2] → right half
        if (!rightSide) angle = Math.PI - angle;
        return new PolarPoint(0.5f, (float)angle).ToCartesian().FromCentered();
    }

    [Fact]
    public void LDrag_DoesNotChange_H_or_S()
    {
        var c = new ColorDiscInteraction();
        c.SyncFromColor(new HslaColor(0.30, 0.60, 0.50, 1));
        double h0 = c.Color.H;
        double s0 = c.Color.S;

        for (int i = 1; i <= 100; i++)
        {
            double l = 0.10 + (i % 80) * 0.01;
            c.UpdateFromL(LRingPoint(l));
        }

        Assert.InRange(c.Color.H - h0, -1e-9, 1e-9);
        Assert.InRange(c.Color.S - s0, -1e-9, 1e-9);
    }

    [Fact]
    public void HsDrag_DoesNotChange_L()
    {
        var c = new ColorDiscInteraction();
        c.SyncFromColor(new HslaColor(0.30, 0.60, 0.42, 1));
        double l0 = c.Color.L;

        for (int i = 0; i < 100; i++)
        {
            // Walk inside the HS disc (radius < 0.5 around center).
            var u = new UnitPoint(0.5f + 0.001f * i, 0.5f + 0.0007f * i);
            c.UpdateFromHs(c.FitToHs(u));
        }

        Assert.InRange(c.Color.L - l0, -1e-9, 1e-9);
    }

    /// <summary>
    /// Sanity test of the cross-decode (naive) pattern previously used by
    /// ColorDisc. We don't assert specific drift magnitudes here — the
    /// previous tests already lock down that the FIXED controller is
    /// drift-free; this just exercises the alternate code path.
    /// </summary>
    [Fact]
    public void NaiveCrossDecode_Path_Compiles_And_Runs()
    {
        var hsDisc = new HueSaturationDisc();
        var lRing  = new LuminosityRing();

        var color = new HslaColor(0.30, 0.60, 0.42, 1);
        var hsLoc = hsDisc.ColorToPoint(color);
        var lLoc  = lRing.ColorToPoint(color);

        for (int i = 1; i <= 50; i++)
        {
            var u = new UnitPoint(0.5f + 0.0005f * i, 0.5f + 0.0003f * i);
            hsLoc = hsDisc.FitToActiveArea(u, color);

            color = hsDisc.UpdateColor(hsLoc, color);
            color = lRing.UpdateColor(lLoc, color);

            lLoc = lRing.ColorToPoint(color, lLoc);
        }

        Assert.True(color.L >= 0 && color.L <= 1);
    }
}
