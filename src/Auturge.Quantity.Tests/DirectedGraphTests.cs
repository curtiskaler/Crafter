namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="DirectedGraph{T}"/>, the adjacency-list graph the unit-conversion engine walks
/// to chain conversions: vertex/edge insertion, breadth-first shortest-path search, and the
/// disconnected / self / missing-vertex edge cases.
/// </summary>
[TestFixture]
public class DirectedGraphTests
{
    private sealed class Graph : DirectedGraph<string>;

    [Test]
    public void AddEdge_MakesBothEndpointsMutualNeighbours()
    {
        var graph = new Graph();

        graph.AddEdge("a", "b");

        Assert.Multiple(() =>
        {
            Assert.That(graph.GetNeighbors("a"), Does.Contain("b"));
            Assert.That(graph.GetNeighbors("b"), Does.Contain("a"));
        });
    }

    [Test]
    public void AddVertex_IsIdempotent()
    {
        var graph = new Graph();

        graph.AddVertex("solo");
        graph.AddVertex("solo");

        Assert.That(graph.GetNeighbors("solo"), Is.Empty);
    }

    [Test]
    public void FindShortestPathBFS_ReturnsTheChainOfVerticesFromStartToTarget()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");
        graph.AddEdge("b", "c");
        graph.AddEdge("c", "d");
        graph.AddEdge("a", "d"); // a shortcut — BFS must prefer it

        IEnumerable<string> path = graph.FindShortedPathBFS("a", "d");

        Assert.That(path, Is.EqualTo(new[] { "a", "d" }));
    }

    [Test]
    public void FindShortestPathBFS_WalksAMultiHopChainWhenThereIsNoShortcut()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");
        graph.AddEdge("b", "c");
        graph.AddEdge("c", "d");

        Assert.That(graph.FindShortedPathBFS("a", "d"), Is.EqualTo(new[] { "a", "b", "c", "d" }));
    }

    [Test]
    public void TryFindBFS_ReturnsFalseForDisconnectedVertices()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");
        graph.AddEdge("x", "y");

        bool found = graph.TryFindBFS("a", "y", out IEnumerable<string> path);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(path, Is.Empty);
        });
    }

    [Test]
    public void FindShortestPathBFS_WhenStartEqualsTarget_ReturnsSingletonPath()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");

        Assert.That(graph.FindShortedPathBFS("a", "a"), Is.EqualTo(new[] { "a" }));
    }

    [Test]
    public void FindShortestPathBFS_WhenAVertexIsUnknown_ReturnsEmpty()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");

        Assert.That(graph.FindShortedPathBFS("a", "missing"), Is.Empty);
    }

    [Test]
    public void BFS_VisitsEveryReachableVertex()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");
        graph.AddEdge("b", "c");
        graph.AddEdge("z", "y"); // unreachable from "a"

        HashSet<string> visited = graph.BFS("a");

        Assert.Multiple(() =>
        {
            Assert.That(visited, Is.EquivalentTo(new[] { "a", "b", "c" }));
            Assert.That(graph.BFS("unknown"), Is.Empty);
        });
    }

    [Test]
    public void DFS_VisitsEveryReachableVertex()
    {
        var graph = new Graph();
        graph.AddEdge("a", "b");
        graph.AddEdge("b", "c");

        Assert.That(graph.DFS("a"), Is.EquivalentTo(new[] { "a", "b", "c" }));
    }
}
