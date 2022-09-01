using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// A convenience class that uses foreign key relationships to determine the order in which data should be inserted/deleted.
/// </summary>
public class TableRelationshipOrderer
{
    /// <summary>
    /// Retrieves the deletion order for a collection of tables.
    /// </summary>
    /// <param name="tables">The tables.</param>
    /// <returns>An ordered set of tables, where the tables at the head of the collection should be deleted from before tables at the tail of the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IReadOnlyCollection<Identifier> GetDeletionOrder(IEnumerable<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var graph = new AdjacencyGraph<Identifier, SEquatableEdge<Identifier>>();
        var tableNames = tables.Select(static t => t.Name).Distinct().ToList();
        graph.AddVertexRange(tableNames);

        var foreignKeys = tables
            .SelectMany(static t => t.ParentKeys)
            .Where(static fk => fk.ChildTable != fk.ParentTable)
            .ToList();
        foreach (var foreignKey in foreignKeys)
            graph.AddEdge(new SEquatableEdge<Identifier>(foreignKey.ChildTable, foreignKey.ParentTable));

        var topologicalSorter = new TopologicalSortingAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);
        topologicalSorter.Compute();

        return topologicalSorter.SortedVertices.Distinct().ToList();
    }

    /// <summary>
    /// Retrieves the insertion order for a collection of tables.
    /// </summary>
    /// <param name="tables">The tables.</param>
    /// <returns>An ordered set of tables, where the tables at the head of the collection should be inserted into before tables at the tail of the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IReadOnlyCollection<Identifier> GetInsertionOrder(IEnumerable<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return GetDeletionOrder(tables).Reverse().ToList();
    }
}