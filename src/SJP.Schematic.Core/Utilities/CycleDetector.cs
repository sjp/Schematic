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
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IEnumerable<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);
        if (!tables.Any())
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

    private IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IVertexListGraph<Identifier, SEquatableEdge<Identifier>> graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var examinedEdges = new List<IEdge<Identifier>>();
        var cycles = new List<IReadOnlyCollection<Identifier>>();
        var dfs = new DepthFirstSearchAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);

        void onExamineEdge(SEquatableEdge<Identifier> e) => examinedEdges.Add(e);
        void onCyclingEdgeFound(SEquatableEdge<Identifier> e) => OnCyclingEdgeFound(examinedEdges, cycles, e);

        try
        {
            dfs.ExamineEdge += onExamineEdge;
            dfs.BackEdge += onCyclingEdgeFound;
            dfs.Compute();
            return cycles;
        }
        finally
        {
            dfs.ExamineEdge -= onExamineEdge;
            dfs.BackEdge -= onCyclingEdgeFound;
        }
    }

    private static void OnCyclingEdgeFound(IEnumerable<IEdge<Identifier>> examinedEdges, ICollection<IReadOnlyCollection<Identifier>> cycles, SEquatableEdge<Identifier> e)
    {
        var cycleNodes = new HashSet<Identifier>();
        cycleNodes.Add(e.Source);
        cycleNodes.Add(e.Target);

        var examinedEdgesList = examinedEdges.ToList();
        var startIndex = examinedEdgesList.FindIndex(edge => edge.Source.Equals(e.Target));

        for (var i = startIndex; i < examinedEdgesList.Count; i++)
        {
            var edge = examinedEdgesList[i];
            cycleNodes.Add(edge.Source);
            cycleNodes.Add(edge.Target);
        }

        if (ContainsCycle(cycles, cycleNodes))
            return;

        cycles.Add(cycleNodes);
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