using System;
using System.Collections.Generic;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// Internal. Not to be consumed externally. A generic graph class.
    /// </summary>
    /// <typeparam name="TVertex">The type of the graph vertex.</typeparam>
    public abstract class Graph<TVertex>
    {
        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        public abstract IEnumerable<TVertex> Vertices { get; }

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="from">A vertex which may contain outgoing neighbours</param>
        /// <returns>A collection of outgoing neighbours.</returns>
        public abstract IEnumerable<TVertex> GetOutgoingNeighbours(TVertex from);

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="to">A vertex which may contain incoming neighbours</param>
        /// <returns>A collection of incoming neighbours.</returns>
        public abstract IEnumerable<TVertex> GetIncomingNeighbours(TVertex to);

        /// <summary>
        /// Internal. Not to be consumed externally.
        /// </summary>
        /// <param name="roots">A list of roots for the graph.</param>
        /// <returns>A set of vertices that cannot be reached from the roots.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="roots"/> is <c>null</c>.</exception>
        public virtual ISet<TVertex> GetUnreachableVertices(IReadOnlyList<TVertex> roots)
        {
            if (roots == null)
                throw new ArgumentNullException(nameof(roots));

            var unreachableVertices = new HashSet<TVertex>(Vertices);
            unreachableVertices.ExceptWith(roots);
            var visitingQueue = new List<TVertex>(roots);

            var currentVertexIndex = 0;
            while (currentVertexIndex < visitingQueue.Count)
            {
                var currentVertex = visitingQueue[currentVertexIndex];
                currentVertexIndex++;
                foreach (var neighbour in GetOutgoingNeighbours(currentVertex))
                {
                    if (unreachableVertices.Remove(neighbour))
                    {
                        visitingQueue.Add(neighbour);
                    }
                }
            }

            return unreachableVertices;
        }

        /// <summary>
        /// Internal. Determines whether the vertex is null or not.
        /// </summary>
        /// <param name="vertex">A vertex.</param>
        /// <returns><c>true</c> if <paramref name="vertex"/> is <c>null</c>; otherwise <c>false</c>. Always <c>false</c> when <see cref="TVertex"/> is a value type.</returns>
        protected static bool IsVertexNull(TVertex vertex) => !_isVertexValueType && EqualityComparer<TVertex>.Default.Equals(vertex, default(TVertex));

        /// <summary>
        /// Internal. Determines whether <see cref="TVertex"/> is a value type.
        /// </summary>
        protected readonly static bool _isVertexValueType = typeof(TVertex).IsValueType;
    }
}
