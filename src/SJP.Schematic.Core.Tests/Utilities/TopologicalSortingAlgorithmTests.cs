using System.Collections.Generic;
using NUnit.Framework;
using QuikGraph;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class TopologicalSortingAlgorithmTests
{
    private static AdjacencyGraph<string, SEquatableEdge<string>> CreateGraph()
    {
        var graph = new AdjacencyGraph<string, SEquatableEdge<string>>();
        graph.AddVertexRange(["a", "b", "c"]);

        // a -> b -> c
        graph.AddEdge(new SEquatableEdge<string>("a", "b"));
        graph.AddEdge(new SEquatableEdge<string>("b", "c"));

        return graph;
    }

    [Test]
    public static void Ctor_GivenNullVertices_ThrowsArgumentNullException()
    {
        var graph = CreateGraph();

        Assert.That(
            () => new TopologicalSortingAlgorithm<string, SEquatableEdge<string>>(graph, null),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Compute_GivenGraphWithDependencies_ReturnsDependentsBeforeDependencies()
    {
        var sorter = new TopologicalSortingAlgorithm<string, SEquatableEdge<string>>(CreateGraph());
        sorter.Compute();

        Assert.That(sorter.SortedVertices, Is.EqualTo(new[] { "a", "b", "c" }).AsCollection);
    }

    [Test]
    public static void Compute_WhenInvokedRepeatedly_ReturnsSameResultWithoutDuplicates()
    {
        var sorter = new TopologicalSortingAlgorithm<string, SEquatableEdge<string>>(CreateGraph());

        sorter.Compute();
        var firstResult = new List<string>(sorter.SortedVertices);

        sorter.Compute();
        var secondResult = sorter.SortedVertices;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondResult, Has.Exactly(3).Items);
            Assert.That(secondResult, Is.Unique);
            Assert.That(secondResult, Is.EqualTo(firstResult).AsCollection);
        }
    }

    [Test]
    public static void Compute_GivenVerticesInCtor_DoesNotMutateGivenCollection()
    {
        var vertices = new List<string>();
        var sorter = new TopologicalSortingAlgorithm<string, SEquatableEdge<string>>(CreateGraph(), vertices);

        sorter.Compute();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vertices, Is.Empty);
            Assert.That(sorter.SortedVertices, Has.Exactly(3).Items);
        }
    }
}
