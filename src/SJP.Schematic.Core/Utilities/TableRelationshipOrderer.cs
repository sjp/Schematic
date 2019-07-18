using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities
{
    public class TableRelationshipOrderer
    {
        public TableRelationshipOrderer(IReadOnlyCollection<IRelationalDatabaseTable> tables)
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

            _deletionOrderer = new Lazy<IReadOnlyCollection<Identifier>>(() => BuildDeletionOrder(graph));
            _insertionSorter = new Lazy<IReadOnlyCollection<Identifier>>(() => _deletionOrderer.Value.Reverse().ToList());
        }

        public IReadOnlyCollection<Identifier> DeletionOrder => _deletionOrderer.Value;

        public IReadOnlyCollection<Identifier> InsertionOrder => _insertionSorter.Value;

        private static IReadOnlyCollection<Identifier> BuildDeletionOrder(IVertexListGraph<Identifier, SEquatableEdge<Identifier>> graph)
        {
            var topologicalSorter = new TopologicalSortingAlgorithm<Identifier, SEquatableEdge<Identifier>>(graph);
            topologicalSorter.Compute();

            return topologicalSorter.SortedVertices.Distinct().ToList();
        }

        private readonly Lazy<IReadOnlyCollection<Identifier>> _deletionOrderer;
        private readonly Lazy<IReadOnlyCollection<Identifier>> _insertionSorter;
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
            SortedVertices = vertices;
        }

        public IList<TVertex> SortedVertices { get; }

        private void FinishVertex(TVertex v) => SortedVertices.Insert(0, v);

        protected override void InternalCompute()
        {
            DepthFirstSearchAlgorithm<TVertex, TEdge> dfs = null;
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
