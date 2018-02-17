using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// Internal. Not to be consumed externally. A generic vertex/edge graph class.
    /// </summary>
    /// <typeparam name="TVertex">The type of the graph vertex.</typeparam>
    /// <typeparam name="TEdge">The type of the graph edge.</typeparam>
    public class Multigraph<TVertex, TEdge> : Graph<TVertex>
    {
        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="vertex">A graph vertex.</param>
        /// <returns>A string representation of the vertex.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="vertex"/> is <c>null</c>.</exception>
        protected virtual string ToString(TVertex vertex)
        {
            if (ReferenceEquals(vertex, null))
                throw new ArgumentNullException(nameof(vertex));

            return vertex.ToString();
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        public virtual IEnumerable<TEdge> Edges => _successorMap.Values.SelectMany(s => s.Values).SelectMany(e => e).Distinct();

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="from">A vertex to get the edges from.</param>
        /// <param name="to">A vertex to get the edges going to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="from"/> or <paramref name="to"/> is <c>null</c>.</exception>
        public virtual IEnumerable<TEdge> GetEdges(TVertex from, TVertex to)
        {
            if (ReferenceEquals(from, null))
                throw new ArgumentNullException(nameof(from));
            if (ReferenceEquals(to, null))
                throw new ArgumentNullException(nameof(to));

            return _successorMap.TryGetValue(from, out var successorSet) && successorSet.TryGetValue(to, out var edgeList)
                ? edgeList
                : Enumerable.Empty<TEdge>();
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="vertex">A vertex to add to the graph.</param>
        /// <exception cref="ArgumentNullException"><paramref name="vertex"/> is <c>null</c>.</exception>
        public virtual void AddVertex(TVertex vertex)
        {
            if (ReferenceEquals(vertex, null))
                throw new ArgumentNullException(nameof(vertex));

            _vertices.Add(vertex);
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="vertices">A collection of vertices to add to the graph.</param>
        /// <exception cref="ArgumentNullException"><paramref name="vertices"/> is <c>null</c>.</exception>
        public virtual void AddVertices(IEnumerable<TVertex> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            _vertices.UnionWith(vertices);
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="from">A vertex to get the edges from.</param>
        /// <param name="to">A vertex to get the edges going to.</param>
        /// <param name="edge">An edge that goes from <paramref name="from"/> to <paramref name="to"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="from"/>, <paramref name="to"/> or <paramref name="edge"/> is <c>null</c>.</exception>
        public virtual void AddEdge(TVertex from, TVertex to, TEdge edge)
        {
            if (ReferenceEquals(from, null))
                throw new ArgumentNullException(nameof(from));
            if (ReferenceEquals(to, null))
                throw new ArgumentNullException(nameof(to));
            if (ReferenceEquals(edge, null))
                throw new ArgumentNullException(nameof(edge));

            AddEdges(from, to, new[] { edge });
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="from">A vertex to get the edges from.</param>
        /// <param name="to">A vertex to get the edges going to.</param>
        /// <param name="edges">A collection of edges that go from <paramref name="from"/> to <paramref name="to"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="from"/>, <paramref name="to"/> or <paramref name="edges"/> is <c>null</c>.</exception>
        public virtual void AddEdges(TVertex from, TVertex to, IEnumerable<TEdge> edges)
        {
            if (ReferenceEquals(from, null))
                throw new ArgumentNullException(nameof(from));
            if (ReferenceEquals(to, null))
                throw new ArgumentNullException(nameof(to));
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            if (!_vertices.Contains(from))
                throw new InvalidOperationException($"The edge cannot be added because the graph does not contain vertex '{ from }'.");

            if (!_vertices.Contains(to))
                throw new InvalidOperationException($"The edge cannot be added because the graph does not contain vertex '{ to }'.");

            if (!_successorMap.TryGetValue(from, out var successorSet))
            {
                successorSet = new Dictionary<TVertex, List<TEdge>>();
                _successorMap.Add(from, successorSet);
            }

            if (!successorSet.TryGetValue(to, out var edgeList))
            {
                edgeList = new List<TEdge>();
                successorSet.Add(to, edgeList);
            }

            edgeList.AddRange(edges);
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        public virtual IReadOnlyList<TVertex> TopologicalSort() => TopologicalSort(null, null);

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="canBreakEdge">A function that determines whether it is permitted to break the edge in order to sort the graph.</param>
        /// <exception cref="ArgumentNullException"><paramref name="canBreakEdge"/> is <c>null</c>.</exception>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge)
        {
            if (canBreakEdge == null)
                throw new ArgumentNullException(nameof(canBreakEdge));

            return TopologicalSort(canBreakEdge, null);
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="formatCycle">A function which determines how to display a cycle.</param>
        /// <exception cref="ArgumentNullException"><paramref name="formatCycle"/> is <c>null</c>.</exception>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            if (formatCycle == null)
                throw new ArgumentNullException(nameof(formatCycle));

            return TopologicalSort(null, formatCycle);
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="canBreakEdge">A function that determines whether it is permitted to break the edge in order to sort the graph.</param>
        /// <param name="formatCycle">A function which determines how to display a cycle.</param>
        public virtual IReadOnlyList<TVertex> TopologicalSort(
            Func<TVertex, TVertex, IEnumerable<TEdge>, bool> canBreakEdge,
            Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var sortedQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbour in GetOutgoingNeighbours(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbour))
                    {
                        predecessorCounts[outgoingNeighbour]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbour] = 1;
                    }
                }
            }

            foreach (var vertex in _vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    sortedQueue.Add(vertex);
                }
            }

            var index = 0;
            while (sortedQueue.Count < _vertices.Count)
            {
                while (index < sortedQueue.Count)
                {
                    var currentRoot = sortedQueue[index];

                    foreach (var successor in GetOutgoingNeighbours(currentRoot).Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
                    {
                        // Decrement counts for edges from sorted vertices and append any vertices that no longer have predecessors
                        predecessorCounts[successor]--;
                        if (predecessorCounts[successor] == 0)
                        {
                            sortedQueue.Add(successor);
                            predecessorCounts.Remove(successor);
                        }
                    }
                    index++;
                }

                // Cycle breaking
                if (sortedQueue.Count < _vertices.Count)
                {
                    var broken = false;

                    var candidateVertices = predecessorCounts.Keys.ToList();
                    var candidateIndex = 0;

                    // Iterate over the unsorted vertices
                    while ((candidateIndex < candidateVertices.Count)
                            && !broken
                            && (canBreakEdge != null))
                    {
                        var candidateVertex = candidateVertices[candidateIndex];

                        // Find vertices in the unsorted portion of the graph that have edges to the candidate
                        var incomingNeighbours = GetIncomingNeighbours(candidateVertex)
                            .Where(neighbour => predecessorCounts.ContainsKey(neighbour)).ToList();

                        foreach (var incomingNeighbour in incomingNeighbours)
                        {
                            // Check to see if the edge can be broken
                            if (canBreakEdge(incomingNeighbour, candidateVertex, _successorMap[incomingNeighbour][candidateVertex]))
                            {
                                predecessorCounts[candidateVertex]--;
                                if (predecessorCounts[candidateVertex] == 0)
                                {
                                    sortedQueue.Add(candidateVertex);
                                    predecessorCounts.Remove(candidateVertex);
                                    broken = true;
                                    break;
                                }
                            }
                        }
                        candidateIndex++;
                    }
                    if (!broken)
                    {
                        // Failed to break the cycle
                        var currentCycleVertex = _vertices.First(v => predecessorCounts.ContainsKey(v));
                        var cycle = new List<TVertex> { currentCycleVertex };
                        var finished = false;
                        while (!finished)
                        {
                            // Find a cycle
                            foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex)
                                .Where(neighbour => predecessorCounts.ContainsKey(neighbour)))
                            {
                                if (predecessorCounts[predecessor] != 0)
                                {
                                    predecessorCounts[currentCycleVertex] = -1;

                                    currentCycleVertex = predecessor;
                                    cycle.Add(currentCycleVertex);
                                    finished = predecessorCounts[predecessor] == -1;
                                    break;
                                }
                            }
                        }
                        cycle.Reverse();

                        // Throw an exception
                        if (formatCycle == null)
                        {
                            throw new InvalidOperationException($"Unable to add relationship because a circular dependency was detected: '{ cycle.Select(ToString).Join(" -> ") }'.");
                        }
                        // Build the cycle message data
                        currentCycleVertex = cycle[0];
                        var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                        foreach (var vertex in cycle.Skip(1))
                        {
                            cycleData.Add(Tuple.Create(currentCycleVertex, vertex, GetEdges(currentCycleVertex, vertex)));
                            currentCycleVertex = vertex;
                        }
                        throw new InvalidOperationException($"Unable to add relationship because a circular dependency was detected: '{ formatCycle(cycleData) }'.");
                    }
                }
            }
            return sortedQueue;
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort()
            => BatchingTopologicalSort(null);

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="formatCycle">A function which determines how to display a cycle.</param>
        public virtual IReadOnlyList<List<TVertex>> BatchingTopologicalSort(
            Func<IEnumerable<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>, string> formatCycle)
        {
            var currentRootsQueue = new List<TVertex>();
            var predecessorCounts = new Dictionary<TVertex, int>();

            foreach (var vertex in _vertices)
            {
                foreach (var outgoingNeighbour in GetOutgoingNeighbours(vertex))
                {
                    if (predecessorCounts.ContainsKey(outgoingNeighbour))
                    {
                        predecessorCounts[outgoingNeighbour]++;
                    }
                    else
                    {
                        predecessorCounts[outgoingNeighbour] = 1;
                    }
                }
            }

            foreach (var vertex in _vertices)
            {
                if (!predecessorCounts.ContainsKey(vertex))
                {
                    currentRootsQueue.Add(vertex);
                }
            }

            var result = new List<List<TVertex>>();
            var nextRootsQueue = new List<TVertex>();
            var currentRootIndex = 0;

            while (currentRootIndex < currentRootsQueue.Count)
            {
                var currentRoot = currentRootsQueue[currentRootIndex];
                currentRootIndex++;

                // Remove edges from current root and add any exposed vertices to the next batch
                foreach (var successor in GetOutgoingNeighbours(currentRoot))
                {
                    predecessorCounts[successor]--;
                    if (predecessorCounts[successor] == 0)
                    {
                        nextRootsQueue.Add(successor);
                    }
                }

                // Roll lists over for next batch
                if (currentRootIndex == currentRootsQueue.Count)
                {
                    result.Add(currentRootsQueue);

                    currentRootsQueue = nextRootsQueue;
                    currentRootIndex = 0;

                    if (currentRootsQueue.Count != 0)
                    {
                        nextRootsQueue = new List<TVertex>();
                    }
                }
            }

            if (result.Sum(b => b.Count) != _vertices.Count)
            {
                // TODO: Support cycle-breaking?

                var currentCycleVertex = _vertices.First(v => predecessorCounts.TryGetValue(v, out var predecessorNumber) && predecessorNumber != 0);
                var cyclicWalk = new List<TVertex> { currentCycleVertex };
                var finished = false;
                while (!finished)
                {
                    foreach (var predecessor in GetIncomingNeighbours(currentCycleVertex))
                    {
                        if (!predecessorCounts.TryGetValue(predecessor, out var predecessorCount))
                            continue;

                        if (predecessorCount != 0)
                        {
                            predecessorCounts[currentCycleVertex] = -1;

                            currentCycleVertex = predecessor;
                            cyclicWalk.Add(currentCycleVertex);
                            finished = predecessorCounts[predecessor] == -1;
                            break;
                        }
                    }
                }
                cyclicWalk.Reverse();

                var cycle = new List<TVertex>();
                var startingVertex = cyclicWalk[0];
                cycle.Add(startingVertex);
                foreach (var vertex in cyclicWalk.Skip(1))
                {
                    if (!vertex.Equals(startingVertex))
                    {
                        cycle.Add(vertex);
                    }
                    else
                    {
                        break;
                    }
                }
                cycle.Add(startingVertex);

                // Throw an exception
                if (formatCycle == null)
                {
                    throw new InvalidOperationException($"Unable to add relationship because a circular dependency was detected: '{ cycle.Select(ToString).Join(" -> ") }'.");
                }
                // Build the cycle message data
                currentCycleVertex = cycle[0];
                var cycleData = new List<Tuple<TVertex, TVertex, IEnumerable<TEdge>>>();

                foreach (var vertex in cycle.Skip(1))
                {
                    cycleData.Add(Tuple.Create(currentCycleVertex, vertex, GetEdges(currentCycleVertex, vertex)));
                    currentCycleVertex = vertex;
                }
                throw new InvalidOperationException($"Unable to add relationship because a circular dependency was detected: '{ formatCycle(cycleData) }'.");
            }

            return result;
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        public override IEnumerable<TVertex> Vertices => _vertices;

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="from">The vertex for which we are retrieving outgoing neighbours for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="from"/> is <c>null</c>.</exception>
        public override IEnumerable<TVertex> GetOutgoingNeighbours(TVertex from)
        {
            if (ReferenceEquals(from, null))
                throw new ArgumentNullException(nameof(from));

            return _successorMap.TryGetValue(from, out var successorSet)
                ? successorSet.Keys
                : Enumerable.Empty<TVertex>();
        }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="to">The vertex for which we are retrieving incoming neighbours for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="to"/> is <c>null</c>.</exception>
        public override IEnumerable<TVertex> GetIncomingNeighbours(TVertex to)
        {
            if (ReferenceEquals(to, null))
                throw new ArgumentNullException(nameof(to));

            return _successorMap.Where(kvp => kvp.Value.ContainsKey(to)).Select(kvp => kvp.Key);
        }

        private readonly HashSet<TVertex> _vertices = new HashSet<TVertex>();
        private readonly Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>> _successorMap = new Dictionary<TVertex, Dictionary<TVertex, List<TEdge>>>();
    }
}
