namespace ColorPicker.Core.Connection;

using System.Collections.Generic;

/// <summary>
/// An undirected graph of connected nodes, used to model color pickers that are
/// linked together so they share a single color.
///
/// Each link between two pickers is an undirected edge. The set of pickers that
/// should update together is the <see cref="ConnectedComponent"/> a picker
/// belongs to. Because component lookup uses a visited set, the graph is safe to
/// query even when links form a cycle (A-B-C-A) — every node is returned once.
///
/// The type is deliberately framework-agnostic (no MAUI/Skia dependency) so the
/// connection logic can be unit-tested in isolation. Node identity uses
/// <see cref="EqualityComparer{T}.Default"/>, which for reference types (the MAUI
/// controls) is reference equality.
/// </summary>
/// <typeparam name="T">The node type (a color picker in the MAUI layer).</typeparam>
public sealed class ConnectionGraph<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _adjacency = new();

    /// <summary>Adds an undirected edge between <paramref name="a"/> and
    /// <paramref name="b"/>. Idempotent; self-edges are ignored.</summary>
    public void AddEdge(T a, T b)
    {
        if (EqualityComparer<T>.Default.Equals(a, b))
            return;

        GetOrAddNeighbors(a).Add(b);
        GetOrAddNeighbors(b).Add(a);
    }

    /// <summary>Removes the undirected edge between <paramref name="a"/> and
    /// <paramref name="b"/> if present. Nodes with no remaining edges are dropped.</summary>
    public void RemoveEdge(T a, T b)
    {
        if (_adjacency.TryGetValue(a, out var na))
        {
            na.Remove(b);
            if (na.Count == 0)
                _adjacency.Remove(a);
        }

        if (_adjacency.TryGetValue(b, out var nb))
        {
            nb.Remove(a);
            if (nb.Count == 0)
                _adjacency.Remove(b);
        }
    }

    /// <summary>Removes <paramref name="node"/> and every edge touching it.</summary>
    public void RemoveNode(T node)
    {
        if (!_adjacency.TryGetValue(node, out var neighbors))
            return;

        foreach (var neighbor in neighbors)
        {
            if (_adjacency.TryGetValue(neighbor, out var back))
            {
                back.Remove(node);
                if (back.Count == 0)
                    _adjacency.Remove(neighbor);
            }
        }

        _adjacency.Remove(node);
    }

    /// <summary>True when <paramref name="a"/> and <paramref name="b"/> are in the
    /// same connected component (directly or transitively linked).</summary>
    public bool AreConnected(T a, T b)
    {
        if (EqualityComparer<T>.Default.Equals(a, b))
            return true;

        foreach (var node in EnumerateComponent(a))
        {
            if (EqualityComparer<T>.Default.Equals(node, b))
                return true;
        }

        return false;
    }

    /// <summary>True when adding an edge between <paramref name="a"/> and
    /// <paramref name="b"/> would close a cycle (they are already connected).
    /// Provided for callers that prefer to reject or drop cycle-forming links;
    /// the graph itself tolerates cycles.</summary>
    public bool WouldFormCycle(T a, T b) => AreConnected(a, b);

    /// <summary>Returns every node reachable from <paramref name="node"/>,
    /// including <paramref name="node"/> itself. An unlinked node yields just
    /// itself. Cycle-safe: each node appears exactly once.</summary>
    public IReadOnlyList<T> ConnectedComponent(T node)
    {
        var result = new List<T>();
        foreach (var n in EnumerateComponent(node))
            result.Add(n);
        return result;
    }

    private IEnumerable<T> EnumerateComponent(T start)
    {
        var visited = new HashSet<T>();
        var queue = new Queue<T>();
        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            if (!_adjacency.TryGetValue(current, out var neighbors))
                continue;

            foreach (var neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                    queue.Enqueue(neighbor);
            }
        }
    }

    private HashSet<T> GetOrAddNeighbors(T node)
    {
        if (!_adjacency.TryGetValue(node, out var set))
        {
            set = new HashSet<T>();
            _adjacency[node] = set;
        }

        return set;
    }
}
