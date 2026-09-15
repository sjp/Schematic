using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// Discovers cyclical foreign key relationships within a database.
/// </summary>
public class CycleDetector
{
    /// <summary>
    /// For a set of tables, determines any cycles and retrieves any cycles detected.
    /// </summary>
    /// <param name="tables">The tables which may contain a cycle.</param>
    /// <returns>A set of cycles, each element contains the set of table names that form a cycle.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);
        if (tables.Count == 0)
            return [];

        var graph = new AdjacencyGraph<Identifier, SEquatableEdge<Identifier>>();
        var tableNames = tables.Select(static t => t.Name).Distinct().ToList();
        graph.AddVertexRange(tableNames);

        var foreignKeys = tables
            .SelectMany(static t => t.ParentKeys)
            .Where(static fk => fk.ChildTable != fk.ParentTable)
            .ToList();
        foreach (var foreignKey in foreignKeys)
            graph.AddEdge(new SEquatableEdge<Identifier>(foreignKey.ChildTable, foreignKey.ParentTable));

        return GetCyclePaths(graph);
    }

    private static IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IVertexListGraph<Identifier, SEquatableEdge<Identifier>> graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var predecessors = new Dictionary<Identifier, Identifier>();
        var cycles = new List<IReadOnlyCollection<Identifier>>();
        var cycleKeys = new HashSet<Identifier[]>(CycleKeyComparer.Instance);
        var dfs = new DepthFirstSearchAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);

        void onTreeEdge(SEquatableEdge<Identifier> e) => predecessors[e.Target] = e.Source;
        void onCyclingEdgeFound(SEquatableEdge<Identifier> e) => OnCyclingEdgeFound(predecessors, cycles, cycleKeys, e);

        try
        {
            dfs.TreeEdge += onTreeEdge;
            dfs.BackEdge += onCyclingEdgeFound;
            dfs.Compute();
            return cycles;
        }
        finally
        {
            dfs.TreeEdge -= onTreeEdge;
            dfs.BackEdge -= onCyclingEdgeFound;
        }
    }

    private static void OnCyclingEdgeFound(IReadOnlyDictionary<Identifier, Identifier> predecessors, ICollection<IReadOnlyCollection<Identifier>> cycles, HashSet<Identifier[]> cycleKeys, SEquatableEdge<Identifier> e)
    {
        var cycleNodes = GetCycleNodes(predecessors, e);
        if (cycleNodes == null || !cycleKeys.Add(GetCycleKey(cycleNodes)))
            return;

        cycles.Add(cycleNodes);
    }

    /// <summary>
    /// Walks the current depth-first search path backwards from the source of a back edge to its
    /// target, giving the vertices that form the cycle in the order they are traversed.
    /// </summary>
    /// <param name="predecessors">The tree edge predecessor of each visited vertex.</param>
    /// <param name="backEdge">A back edge, i.e. an edge pointing at an ancestor of its source.</param>
    /// <returns>The vertices forming the cycle, or <see langword="null" /> when the target is not an ancestor of the source.</returns>
    private static IReadOnlyCollection<Identifier>? GetCycleNodes(IReadOnlyDictionary<Identifier, Identifier> predecessors, SEquatableEdge<Identifier> backEdge)
    {
        var reversedPath = new List<Identifier> { backEdge.Source };
        var current = backEdge.Source;

        while (!current.Equals(backEdge.Target))
        {
            if (!predecessors.TryGetValue(current, out var predecessor))
                return null;

            reversedPath.Add(predecessor);
            current = predecessor;
        }

        reversedPath.Reverse();
        return reversedPath;
    }

    /// <summary>
    /// Creates a key identifying a cycle by the set of vertices it contains, so that cycles
    /// visiting the same vertices from a different starting point or in a different order share a key.
    /// </summary>
    /// <param name="cycle">The vertices forming a cycle.</param>
    /// <returns>The distinct vertices of the cycle in sorted order.</returns>
    private static Identifier[] GetCycleKey(IReadOnlyCollection<Identifier> cycle) => [.. cycle.Distinct().Order()];

    /// <summary>
    /// Compares cycle keys element by element, with a hash code that agrees with that comparison.
    /// </summary>
    private sealed class CycleKeyComparer : IEqualityComparer<Identifier[]>
    {
        public static CycleKeyComparer Instance { get; } = new CycleKeyComparer();

        public bool Equals(Identifier[]? x, Identifier[]? y) => x.AsSpan().SequenceEqual(y);

        public int GetHashCode(Identifier[] obj)
        {
            var hash = new HashCode();
            foreach (var identifier in obj)
                hash.Add(identifier);
            return hash.ToHashCode();
        }
    }
}