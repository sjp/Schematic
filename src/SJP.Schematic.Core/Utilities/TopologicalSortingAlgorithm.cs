using System;
using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities
{

    /// <summary>
    /// Applies topological sorting in the same manner as QuickGraph's built-in algorithm. However, this implementation enables cycles and potentially incorrect results.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    /// <remarks>Not intended to be used directly.</remarks>
    public sealed class TopologicalSortingAlgorithm<TVertex, TEdge> : AlgorithmBase<IVertexListGraph<TVertex, TEdge>> where TEdge : IEdge<TVertex>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopologicalSortingAlgorithm{TVertex, TEdge}"/> class.
        /// </summary>
        /// <param name="graph">The graph.</param>
        public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph)
            : this(graph, new List<TVertex>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopologicalSortingAlgorithm{TVertex, TEdge}"/> class.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="vertices">The vertices for the graph.</param>
        /// <exception cref="ArgumentNullException"><paramref name="vertices"/> is <c>null</c>.</exception>
        public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph, IList<TVertex> vertices)
            : base(graph)
        {
            SortedVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        }

        /// <summary>
        /// A set of sorted vertices in the graph.
        /// </summary>
        /// <value>The sorted vertices.
        /// </value>
        public IList<TVertex> SortedVertices { get; }

        private void FinishVertex(TVertex v) => SortedVertices.Insert(0, v);

        /// <summary>
        /// Algorithm compute step.
        /// </summary>
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
