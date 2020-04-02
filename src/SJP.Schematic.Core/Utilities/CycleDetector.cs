using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities
{
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
        public IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IReadOnlyCollection<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (tables.Count == 0)
                return Array.Empty<IReadOnlyCollection<Identifier>>();

            var graph = new AdjacencyGraph<Identifier, SEquatableEdge<Identifier>>();
            var tableNames = tables.Select(t => t.Name).Distinct().ToList();
            graph.AddVertexRange(tableNames);

            var foreignKeys = tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ChildTable != fk.ParentTable)
                .ToList();
            foreach (var foreignKey in foreignKeys)
                graph.AddEdge(new SEquatableEdge<Identifier>(foreignKey.ChildTable, foreignKey.ParentTable));

            return GetCyclePaths(graph);
        }

        private IReadOnlyCollection<IReadOnlyCollection<Identifier>> GetCyclePaths(IVertexListGraph<Identifier, SEquatableEdge<Identifier>> graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

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
            var startingNode = e.Target;
            var nextNode = e.Source;

            var knownNodes = new List<Identifier> { startingNode, nextNode };

            var edges = examinedEdges.Reverse().Skip(1); // skipping first edge because that's the back edge
            foreach (var edge in edges)
            {
                if (edge.Target != nextNode)
                    continue;

                if (!knownNodes.Contains(edge.Source))
                {
                    knownNodes.Add(edge.Source);
                    nextNode = edge.Source;
                }
                else
                {
                    knownNodes.Reverse();
                    if (!ContainsCycle(cycles, knownNodes))
                        cycles.Add(knownNodes);
                    return;
                }
            }
        }

        private static bool ContainsCycle(IEnumerable<IReadOnlyCollection<Identifier>> existingCycles, IReadOnlyCollection<Identifier> newCycle)
        {
            if (existingCycles == null)
                throw new ArgumentNullException(nameof(existingCycles));
            if (newCycle == null)
                throw new ArgumentNullException(nameof(newCycle));

            return existingCycles.Any(ec => CyclesEqual(ec, newCycle));
        }

        private static bool CyclesEqual(IReadOnlyCollection<Identifier> existingCycle, IReadOnlyCollection<Identifier> newCycle)
        {
            if (existingCycle == null)
                throw new ArgumentNullException(nameof(existingCycle));
            if (newCycle == null)
                throw new ArgumentNullException(nameof(newCycle));

            var orderedExisting = existingCycle.OrderBy(name => name).Distinct().ToList();
            var orderedNewCycle = newCycle.OrderBy(name => name).Distinct().ToList();

            return orderedExisting.SequenceEqual(orderedNewCycle);
        }
    }
}
