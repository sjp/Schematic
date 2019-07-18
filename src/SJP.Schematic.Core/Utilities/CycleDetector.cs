using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// Discovers cycles within a database
    /// </summary>
    public class CycleDetector
    {
        private IList<IEdge<Identifier>> _examinedEdges;
        private List<IReadOnlyCollection<Identifier>> _cycles;

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

            _examinedEdges = new List<IEdge<Identifier>>();
            _cycles = new List<IReadOnlyCollection<Identifier>>();
            var dfs = new DepthFirstSearchAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);

            try
            {
                dfs.ExamineEdge += OnExamineEdge;
                dfs.BackEdge += OnCyclingEdgeFound;
                dfs.Compute();
                return _cycles;
            }
            finally
            {
                dfs.ExamineEdge -= OnExamineEdge;
                dfs.BackEdge -= OnCyclingEdgeFound;
            }
        }

        private void OnExamineEdge(SEquatableEdge<Identifier> e) => _examinedEdges.Add(e);

        private void OnCyclingEdgeFound(SEquatableEdge<Identifier> e)
        {
            var startingNode = e.Target;
            var nextNode = e.Source;

            var knownNodes = new List<Identifier> { startingNode, nextNode };

            var edges = _examinedEdges.Reverse().Skip(1); // skipping first edge because that's the back edge
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
                    _cycles.Add(knownNodes);
                    return;
                }
            }
        }
    }
}
