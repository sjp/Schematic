using System;
using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// Applies topological sorting in the same manner as QuickGraph's built-in algorithm. However, this implementation enables cycles and potentially incorrect results.
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
/// <typeparam name="TEdge">The edge type.</typeparam>
/// <remarks>Not intended to be used directly.</remarks>
public sealed class TopologicalSortingAlgorithm<TVertex, TEdge> : AlgorithmBase<IVertexListGraph<TVertex, TEdge>>
    where TVertex : notnull
    where TEdge : IEdge<TVertex>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TopologicalSortingAlgorithm{TVertex, TEdge}"/> class.
    /// </summary>
    /// <param name="graph">The graph.</param>
    public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph)
        : this(graph, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TopologicalSortingAlgorithm{TVertex, TEdge}"/> class.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="vertices">The vertices for the graph.</param>
    /// <exception cref="ArgumentNullException"><paramref name="vertices"/> is <see langword="null" />.</exception>
    public TopologicalSortingAlgorithm(IVertexListGraph<TVertex, TEdge> graph, IList<TVertex> vertices)
        : base(graph)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        _sortedVertices = [.. vertices];
    }

    /// <summary>
    /// A set of sorted vertices in the graph.
    /// </summary>
    /// <value>The sorted vertices.
    /// </value>
    /// <remarks>Reset at the start of every computation, so the results of a computation never accumulate on top of a previous one.</remarks>
    public IList<TVertex> SortedVertices => _sortedVertices;

    private void FinishVertex(TVertex v) => _sortedVertices.Add(v);

    /// <summary>
    /// Algorithm compute step.
    /// </summary>
    protected override void InternalCompute()
    {
        _sortedVertices.Clear();

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

            // vertices are appended in the order in which they finish, i.e. dependencies first
            _sortedVertices.Reverse();
        }
        finally
        {
            if (dfs != null)
                dfs.FinishVertex -= FinishVertex;
        }
    }

    private readonly List<TVertex> _sortedVertices;
}