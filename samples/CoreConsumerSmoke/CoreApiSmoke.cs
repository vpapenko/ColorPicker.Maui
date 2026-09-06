using ColorPicker.Core;
using ColorPicker.Core.Connection;
using ColorPicker.Core.Interaction;

namespace CoreConsumerSmoke;

public static class CoreApiSmoke
{
    public static RgbaColor ExercisePublicApi(HslaColor color)
    {
        var disc = new HueSaturationDisc();
        var triangle = new SaturationValueTriangle();
        var discInteraction = new ColorDiscInteraction();
        var triangleInteraction = new TriangleAreaInteraction();
        var graph = new ConnectionGraph<string>();

        discInteraction.SyncFromColor(color);
        triangleInteraction.SyncFromColor(color);
        graph.AddEdge("disc", "triangle");

        var discPoint = disc.ColorToPoint(color);
        var trianglePoint = triangle.ColorToPoint(color);
        var updated = disc.UpdateColor(discPoint, color);
        updated = triangle.UpdateColor(trianglePoint, updated);

        return graph.AreConnected("disc", "triangle")
            ? updated.ToRgba()
            : color.ToRgba();
    }
}
