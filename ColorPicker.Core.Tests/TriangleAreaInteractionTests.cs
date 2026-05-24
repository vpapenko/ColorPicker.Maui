using ColorPicker.Core;
using ColorPicker.Core.Interaction;

namespace ColorPicker.Core.Tests;

/// <summary>
/// Deterministic guards for the bug pattern fixed in PR #36.
///
/// The bug class: per-touch-event "UpdateColors" methods that decode BOTH
/// the SV indicator and the H indicator from cached unit-space coordinates,
/// regardless of which one the user actually touched. On a rotating triangle
/// this re-decodes (S, L) at the new hue from the OLD unit-space point —
/// which encodes a different (S, L) under the rotated geometry. The visible
/// symptom in PR #36 was S/L drift while dragging hue.
/// </summary>
public class TriangleAreaInteractionTests
{
    static UnitPoint HueRingPoint(double hue01)
    {
        double angle = Math.PI - (2.0 * Math.PI * hue01);
        var cart = new PolarPoint(0.5f, (float)angle).ToCartesian();
        return cart.FromCentered();
    }

    [Fact]
    public void HueDrag_DoesNotDrift_S_or_L_on_RotatedTriangle()
    {
        // Reproduces the PR #36 bug class: dragging the hue ring on the
        // rotating triangle should not budge S or L.
        var c = new TriangleAreaInteraction(rotateByHue: true);
        c.SyncFromColor(new HslaColor(0.10, 0.70, 0.45, 1));

        double s0 = c.Color.S;
        double l0 = c.Color.L;

        // 360 small hue steps (one per "touch event") covering a full revolution.
        for (int i = 1; i <= 360; i++)
        {
            double h = (0.10 + i / 360.0) % 1.0;
            c.UpdateFromH(HueRingPoint(h));
        }

        Assert.InRange(c.Color.S - s0, -1e-9, 1e-9);
        Assert.InRange(c.Color.L - l0, -1e-9, 1e-9);
    }

    [Fact]
    public void HueDrag_DoesNotDrift_on_FixedTriangle()
    {
        var c = new TriangleAreaInteraction(rotateByHue: false);
        c.SyncFromColor(new HslaColor(0.10, 0.70, 0.45, 1));

        double s0 = c.Color.S;
        double l0 = c.Color.L;

        for (int i = 1; i <= 360; i++)
        {
            double h = (0.10 + i / 360.0) % 1.0;
            c.UpdateFromH(HueRingPoint(h));
        }

        Assert.InRange(c.Color.S - s0, -1e-9, 1e-9);
        Assert.InRange(c.Color.L - l0, -1e-9, 1e-9);
    }

    [Fact]
    public void HueDrag_UpdatesH()
    {
        var c = new TriangleAreaInteraction(rotateByHue: true);
        c.SyncFromColor(new HslaColor(0.10, 0.70, 0.45, 1));

        c.UpdateFromH(HueRingPoint(0.40));

        Assert.InRange(c.Color.H - 0.40, -1e-7, 1e-7);
    }

    [Fact]
    public void SvDrag_DoesNotChange_H()
    {
        var c = new TriangleAreaInteraction(rotateByHue: true);
        c.SyncFromColor(new HslaColor(0.32, 0.50, 0.50, 1));
        double h0 = c.Color.H;

        // Walk a path of unit points inside the SV triangle.
        for (int i = 0; i < 50; i++)
        {
            var u = new UnitPoint(0.5f + 0.0001f * i, 0.4f + 0.001f * i);
            c.UpdateFromSv(c.FitToSv(u));
        }

        Assert.InRange(c.Color.H - h0, -1e-9, 1e-9);
    }

    [Fact]
    public void Grayscale_PreservesLastHue_AcrossSyncRoundtrip()
    {
        var c = new TriangleAreaInteraction(rotateByHue: true);
        c.SyncFromColor(new HslaColor(0.42, 0.5, 0.5, 1));
        Assert.Equal(0.42, c.LastHue, 9);

        // Go to grayscale — LastHue must be preserved.
        c.SyncFromColor(new HslaColor(0.0, 0.0, 0.5, 1));
        Assert.True(c.ZeroSL);
        Assert.Equal(0.42, c.LastHue, 9);
    }

    [Fact]
    public void RotatingTriangle_HueDrag_ReencodesSvLocationButPreservesColor()
    {
        // After a hue drag the rotating-triangle SV indicator should move
        // to the position corresponding to the (unchanged) S, L at the new hue.
        var c = new TriangleAreaInteraction(rotateByHue: true);
        c.SyncFromColor(new HslaColor(0.0, 0.5, 0.5, 1));

        var before = c.LocationSv;
        c.UpdateFromH(HueRingPoint(0.33));

        Assert.NotEqual(before, c.LocationSv); // indicator moved within rotated triangle
        Assert.InRange(c.Color.S - 0.5, -1e-9, 1e-9);
        Assert.InRange(c.Color.L - 0.5, -1e-9, 1e-9);
    }

    /// <summary>
    /// Documents the bug class: a NAIVE implementation that re-decodes BOTH
    /// SV and H from cached unit points per touch event drifts S/L on
    /// the rotating triangle. The new controller's per-region split prevents
    /// this — this test simulates the buggy pattern to show it does drift,
    /// so the regression guards above are not vacuous.
    /// </summary>
    [Fact]
    public void NaiveCrossDecode_Drifts_OnRotatedTriangle()
    {
        var triangle = new SaturationValueTriangle(rotateByHue: true);
        var ring = new HueRing();

        var color = new HslaColor(0.10, 0.70, 0.45, 1);
        var svLoc = triangle.ColorToPoint(color);
        var hLoc  = ring.ColorToPoint(color);

        double s0 = color.S, l0 = color.L;

        for (int i = 1; i <= 360; i++)
        {
            // Simulate a hue touch event.
            double newH = (0.10 + i / 360.0) % 1.0;
            hLoc = HueRingPoint(newH);

            // Buggy pattern: decode BOTH every event.
            color = triangle.UpdateColor(svLoc, color);
            color = ring.UpdateColor(hLoc, color);
        }

        // The naive pattern drifts even at this scale (0.01+) — many orders
        // of magnitude more than the 1e-9 tolerance of the fixed-controller
        // tests above, demonstrating those guards aren't vacuous.
        double drift = Math.Abs(color.S - s0) + Math.Abs(color.L - l0);
        Assert.True(drift > 1e-4,
            $"Expected meaningful drift from the naive cross-decode pattern; got {drift}.");
    }
}
