namespace ColorPicker.Core.Tests;

using System.Linq;
using ColorPicker.Core.Connection;

public class ConnectionGraphTests
{
    static ConnectionGraph<string> Graph() => new();

    [Fact]
    public void UnlinkedNode_Component_IsJustItself()
    {
        var g = Graph();
        Assert.Equal(new[] { "A" }, g.ConnectedComponent("A"));
    }

    [Fact]
    public void SingleEdge_ConnectsBothNodes()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        Assert.Equal(new[] { "A", "B" }, g.ConnectedComponent("A").OrderBy(x => x));
        Assert.Equal(new[] { "A", "B" }, g.ConnectedComponent("B").OrderBy(x => x));
    }

    [Fact]
    public void Chain_IsTransitivelyConnected()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("B", "C");
        Assert.Equal(new[] { "A", "B", "C" }, g.ConnectedComponent("A").OrderBy(x => x));
    }

    [Fact]
    public void Star_AllSatellitesShareOneComponent()
    {
        // Mirrors the sample: many pickers attached to one master.
        var g = Graph();
        g.AddEdge("S1", "M");
        g.AddEdge("S2", "M");
        g.AddEdge("S3", "M");
        Assert.Equal(new[] { "M", "S1", "S2", "S3" }, g.ConnectedComponent("S2").OrderBy(x => x));
    }

    [Fact]
    public void Cycle_IsSafe_AndReturnsEachNodeOnce()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("B", "C");
        g.AddEdge("C", "A"); // closes the loop
        var component = g.ConnectedComponent("A");
        Assert.Equal(new[] { "A", "B", "C" }, component.OrderBy(x => x));
        Assert.Equal(3, component.Count); // no duplicates despite the cycle
    }

    [Fact]
    public void DisjointComponents_StaySeparate()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("C", "D");
        Assert.Equal(new[] { "A", "B" }, g.ConnectedComponent("A").OrderBy(x => x));
        Assert.Equal(new[] { "C", "D" }, g.ConnectedComponent("C").OrderBy(x => x));
        Assert.False(g.AreConnected("A", "C"));
    }

    [Fact]
    public void AddEdge_IsIdempotent()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("A", "B");
        Assert.Equal(2, g.ConnectedComponent("A").Count);
    }

    [Fact]
    public void SelfEdge_IsIgnored()
    {
        var g = Graph();
        g.AddEdge("A", "A");
        Assert.Equal(new[] { "A" }, g.ConnectedComponent("A"));
    }

    [Fact]
    public void RemoveEdge_SplitsComponent()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("B", "C");
        g.RemoveEdge("B", "C");
        Assert.Equal(new[] { "A", "B" }, g.ConnectedComponent("A").OrderBy(x => x));
        Assert.Equal(new[] { "C" }, g.ConnectedComponent("C"));
    }

    [Fact]
    public void RemoveNode_DetachesItFromEveryNeighbor()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("B", "C");
        g.RemoveNode("B");
        Assert.Equal(new[] { "A" }, g.ConnectedComponent("A"));
        Assert.Equal(new[] { "C" }, g.ConnectedComponent("C"));
        Assert.False(g.AreConnected("A", "C"));
    }

    [Fact]
    public void AreConnected_SameNode_IsTrue()
    {
        var g = Graph();
        Assert.True(g.AreConnected("A", "A"));
    }

    [Fact]
    public void WouldFormCycle_TrueWhenAlreadyConnected()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.AddEdge("B", "C");
        Assert.True(g.WouldFormCycle("A", "C"));  // A..C already connected
        Assert.False(g.WouldFormCycle("A", "D")); // D is unrelated
    }

    [Fact]
    public void ReAddingRemovedEdge_Reconnects()
    {
        var g = Graph();
        g.AddEdge("A", "B");
        g.RemoveEdge("A", "B");
        Assert.False(g.AreConnected("A", "B"));
        g.AddEdge("A", "B");
        Assert.True(g.AreConnected("A", "B"));
    }
}
