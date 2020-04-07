using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities
{
    public class TableRelationshipOrderer
    {
        public IReadOnlyCollection<Identifier> GetDeletionOrder(IReadOnlyCollection<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var graph = new AdjacencyGraph<Identifier, SEquatableEdge<Identifier>>();
            var tableNames = tables.Select(t => t.Name).Distinct().ToList();
            graph.AddVertexRange(tableNames);

            var foreignKeys = tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ChildTable != fk.ParentTable)
                .ToList();
            foreach (var foreignKey in foreignKeys)
                graph.AddEdge(new SEquatableEdge<Identifier>(foreignKey.ChildTable, foreignKey.ParentTable));

            var topologicalSorter = new TopologicalSortingAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);
            topologicalSorter.Compute();

            return topologicalSorter.SortedVertices.Distinct().ToList();
        }

        public IReadOnlyCollection<Identifier> GetInsertionOrder(IReadOnlyCollection<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return GetDeletionOrder(tables).Reverse().ToList();
        }
    }

    /// <summary>
    /// Applies topological sorting in the same manner as QuickGraph's built-in algorithm. However, this implementation enables cycles and potentially incorrect results.
    /// </summary>
    public sealed class TopologicalSortingAlgorithm<TVertex, TEdge> : AlgorithmBase<IVertexListGraph<TVertex, TEdge>> where TEdge : IEdge<TVertex>
    {
        public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph)
            : this(graph, new List<TVertex>())
        {
        }

        public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph, IList<TVertex> vertices)
            : base(graph)
        {
            SortedVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        }

        public IList<TVertex> SortedVertices { get; }

        private void FinishVertex(TVertex v) => SortedVertices.Insert(0, v);

        protected override void InternalCompute()
        {
            DepthFirstSearchAlgorithm<TVertex, TEdge>? dfs = null;
            try
            {
                dfs = new DepthFirstSearchAlgorithm<TVertex, TEdge>(
                    this,
                    VisitedGraph,
                    new Dictionary<TVertex, GraphColor>(VisitedGraph.VertexCount)
                );
                dfs.FinishVertex += FinishVertex;

                dfs.Compute();
            }
            finally
            {
                if (dfs != null)
                    dfs.FinishVertex -= FinishVertex;
            }
        }
    }
}
