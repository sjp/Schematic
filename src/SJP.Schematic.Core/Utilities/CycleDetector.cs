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
        var dfs = new DepthFirstSearchAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);

        void onTreeEdge(SEquatableEdge<Identifier> e) => predecessors[e.Target] = e.Source;
        void onCyclingEdgeFound(SEquatableEdge<Identifier> e) => OnCyclingEdgeFound(predecessors, cycles, e);

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

    private static void OnCyclingEdgeFound(IReadOnlyDictionary<Identifier, Identifier> predecessors, ICollection<IReadOnlyCollection<Identifier>> cycles, SEquatableEdge<Identifier> e)
    {
        var cycleNodes = GetCycleNodes(predecessors, e);
        if (cycleNodes == null || ContainsCycle(cycles, cycleNodes))
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

    private static bool ContainsCycle(IEnumerable<IReadOnlyCollection<Identifier>> existingCycles, IReadOnlyCollection<Identifier> newCycle)
    {
        ArgumentNullException.ThrowIfNull(existingCycles);
        ArgumentNullException.ThrowIfNull(newCycle);

        return existingCycles.Any(ec => CyclesEqual(ec, newCycle));
    }

    private static bool CyclesEqual(IReadOnlyCollection<Identifier> existingCycle, IReadOnlyCollection<Identifier> newCycle)
    {
        ArgumentNullException.ThrowIfNull(existingCycle);
        ArgumentNullException.ThrowIfNull(newCycle);

        var orderedExisting = existingCycle.Order().Distinct().ToList();
        var orderedNewCycle = newCycle.Order().Distinct().ToList();

        return orderedExisting.SequenceEqual(orderedNewCycle);
    }
}