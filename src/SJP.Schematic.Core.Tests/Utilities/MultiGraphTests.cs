using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

#pragma warning disable S125 // Sections of code should not be commented out

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    public static class MultigraphTests
    {
        private class Vertex
        {
            public int Id { get; set; }

            public override string ToString() => Id.ToString();
        }

        private class Edge
        {
            public int Id { get; set; }

            public override string ToString() => Id.ToString();
        }

        [Test]
        public static void GetIncomingNeighbours_GivenNullVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.GetIncomingNeighbours(null));
        }

        [Test]
        public static void GetOutgoingNeighbours_GivenNullVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.GetOutgoingNeighbours(null));
        }

        [Test]
        public static void AddEdges_GivenNullFromVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var to = new Vertex();
            var edges = Array.Empty<Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdges(null, to, edges));
        }

        [Test]
        public static void AddEdges_GivenNullToVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var from = new Vertex();
            var edges = Array.Empty<Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdges(from, null, edges));
        }

        [Test]
        public static void AddEdges_GivenNullEdges_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var from = new Vertex();
            var to = new Vertex();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdges(from, to, null));
        }

        [Test]
        public static void AddEdge_GivenNullFromVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var to = new Vertex();
            var edge = new Edge();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdge(null, to, edge));
        }

        [Test]
        public static void AddEdge_GivenNullToVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var from = new Vertex();
            var edge = new Edge();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdge(from, null, edge));
        }

        [Test]
        public static void AddEdge_GivenNullEdge_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var from = new Vertex();
            var to = new Vertex();

            Assert.Throws<ArgumentNullException>(() => graph.AddEdge(from, to, null));
        }

        [Test]
        public static void AddVertices_GivenNullVertices_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.AddVertices(null));
        }

        [Test]
        public static void AddVertex_GivenNullVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();

            Assert.Throws<ArgumentNullException>(() => graph.AddVertex(null));
        }

        [Test]
        public static void GetEdges_GivenNullFromVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var to = new Vertex();

            Assert.Throws<ArgumentNullException>(() => graph.GetEdges(null, to));
        }

        [Test]
        public static void GetEdges_GivenNullToVertex_ThrowsArgumentNullException()
        {
            var graph = new Multigraph<Vertex, Edge>();
            var from = new Vertex();

            Assert.Throws<ArgumentNullException>(() => graph.GetEdges(from, null));
        }

        [Test]
        public static void AddVertex_adds_a_vertex()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };

            var graph = new Multigraph<Vertex, Edge>();

            graph.AddVertex(vertexOne);
            graph.AddVertex(vertexTwo);

            Assert.AreEqual(2, graph.Vertices.Count());
            Assert.AreEqual(2, graph.Vertices.Intersect(new[] { vertexOne, vertexTwo }).Count());
        }

        [Test]
        public static void AddVertices_add_verticies()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();

            graph.AddVertices(new[] { vertexOne, vertexTwo });
            graph.AddVertices(new[] { vertexTwo, vertexThree });

            Assert.AreEqual(3, graph.Vertices.Count());
            Assert.AreEqual(3, graph.Vertices.Intersect(new[] { vertexOne, vertexTwo, vertexThree }).Count());
        }

        [Test]
        public static void AddEdge_adds_an_edge()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo });
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            graph.AddEdge(vertexOne, vertexTwo, edgeTwo);

            Assert.AreEqual(2, graph.Edges.Count());
            Assert.AreEqual(2, graph.Edges.Intersect(new[] { edgeOne, edgeTwo }).Count());

            Assert.AreEqual(0, graph.GetEdges(vertexTwo, vertexOne).Count());
            Assert.AreEqual(2, graph.GetEdges(vertexOne, vertexTwo).Count());
            Assert.AreEqual(2, graph.GetEdges(vertexOne, vertexTwo).Intersect(new[] { edgeOne, edgeTwo }).Count());
        }

        [Test]
        public static void AddEdge_throws_on_verticies_not_in_the_graph()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };

            var edgeOne = new Edge
            {
                Id = 1
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            Assert.Throws<InvalidOperationException>(() => graph.AddEdge(vertexOne, vertexTwo, edgeOne));
        }

        [Test]
        public static void AddEdges_adds_multiple_edges()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo });
            graph.AddEdges(vertexOne, vertexTwo, new[] { edgeOne });
            graph.AddEdges(vertexOne, vertexTwo, new[] { edgeTwo, edgeThree });

            Assert.AreEqual(0, graph.GetEdges(vertexTwo, vertexOne).Count());
            Assert.AreEqual(3, graph.GetEdges(vertexOne, vertexTwo).Count());
            Assert.AreEqual(3, graph.GetEdges(vertexOne, vertexTwo).Intersect(new[] { edgeOne, edgeTwo, edgeThree }).Count());
        }

        [Test]
        public static void AddEdges_throws_on_verticies_not_in_the_graph()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };

            var edgeOne = new Edge
            {
                Id = 1
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            Assert.Throws<InvalidOperationException>(() => graph.AddEdges(vertexOne, vertexTwo, new[] { edgeOne }));
        }

        [Test]
        public static void AddEdge_updates_incomming_and_outgoing_neighbours()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            graph.AddEdge(vertexOne, vertexThree, edgeTwo);
            graph.AddEdge(vertexTwo, vertexThree, edgeThree);

            Assert.AreEqual(2, graph.GetOutgoingNeighbours(vertexOne).Count());
            Assert.AreEqual(2, graph.GetOutgoingNeighbours(vertexOne).Intersect(new[] { vertexTwo, vertexThree }).Count());

            Assert.AreEqual(2, graph.GetIncomingNeighbours(vertexThree).Count());
            Assert.AreEqual(2, graph.GetIncomingNeighbours(vertexThree).Intersect(new[] { vertexOne, vertexTwo }).Count());
        }

        [Test]
        public static void TopologicalSort_on_graph_with_no_edges_returns_all_verticies()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            var result = graph.TopologicalSort();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(3, result.Intersect(new[] { vertexOne, vertexTwo, vertexThree }).Count());
            });
        }

        [Test]
        public static void TopologicalSort_on_simple_graph_returns_all_verticies_in_order()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 2-> {1}
            graph.AddEdge(vertexTwo, vertexOne, edgeOne);
            // 1 -> {3}
            graph.AddEdge(vertexOne, vertexThree, edgeTwo);

            Assert.AreEqual(
                new[] { vertexTwo, vertexOne, vertexThree },
                graph.TopologicalSort().ToArray());
        }

        [Test]
        public static void TopologicalSort_on_tree_graph_returns_all_verticies_in_order()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 1 -> {2, 3}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            graph.AddEdge(vertexOne, vertexThree, edgeTwo);
            // 3 -> {2}
            graph.AddEdge(vertexThree, vertexTwo, edgeThree);

            Assert.AreEqual(
                new[] { vertexOne, vertexThree, vertexTwo },
                graph.TopologicalSort().ToArray());
        }

        [Test]
        public static void TopologicalSort_on_self_ref_can_break_cycle()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };

            var edgeOne = new Edge
            {
                Id = 1
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertex(vertexOne);

            // 1 -> {1}
            graph.AddEdge(vertexOne, vertexOne, edgeOne);

            Assert.AreEqual(
                new[] { vertexOne },
                graph.TopologicalSort(
                    (from, to, edges) =>
                        (from == vertexOne)
                        && (to == vertexOne)
                        && (edges.Intersect(new[] { edgeOne }).Count() == 1)).ToArray());
        }

        [Test]
        public static void TopologicalSort_can_break_simple_cycle()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 1 -> {2}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            // 2 -> {3}
            graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
            // 3 -> {1}
            graph.AddEdge(vertexThree, vertexOne, edgeThree);

            Assert.AreEqual(
                new[] { vertexOne, vertexTwo, vertexThree },
                graph.TopologicalSort(
                    (from, to, edges) =>
                        (from == vertexThree)
                        && (to == vertexOne)
                        && (edges.Single() == edgeThree)).ToArray());
        }

        [Test]
        public static void TopologicalSort_can_break_two_cycles()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };
            var vertexFour = new Vertex
            {
                Id = 4
            };
            var vertexFive = new Vertex
            {
                Id = 5
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };
            var edgeFour = new Edge
            {
                Id = 4
            };
            var edgeFive = new Edge
            {
                Id = 5
            };
            var edgeSix = new Edge
            {
                Id = 6
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree, vertexFour, vertexFive });

            // 1 -> {2, 4}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            graph.AddEdge(vertexOne, vertexFour, edgeTwo);
            // 2 -> {3}
            graph.AddEdge(vertexTwo, vertexThree, edgeThree);
            // 3 -> {1}
            graph.AddEdge(vertexThree, vertexOne, edgeFour);
            // 4 -> {5}
            graph.AddEdge(vertexFour, vertexFive, edgeFive);
            // 5 -> {1}
            graph.AddEdge(vertexFive, vertexOne, edgeSix);

            Assert.AreEqual(
                new[] { vertexTwo, vertexThree, vertexOne, vertexFour, vertexFive },
                graph.TopologicalSort(
                    (_, __, edges) =>
                    {
                        var edge = edges.Single();
                        return (edge == edgeOne) || (edge == edgeSix);
                    }).ToArray());
        }

        [Test]
        public static void TopologicalSort_throws_with_default_message_when_cycle_cannot_be_broken()
        {
            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 1 -> {2}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            // 2 -> {3}
            graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
            // 3 -> {1}
            graph.AddEdge(vertexThree, vertexOne, edgeThree);

            Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort());
        }

        [Test]
        public static void TopologicalSort_throws_with_formatted_message_when_cycle_cannot_be_broken()
        {
            const string message = "Formatted cycle";

            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 1 -> {2}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            // 2 -> {3}
            graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
            // 3 -> {1}
            graph.AddEdge(vertexThree, vertexOne, edgeThree);

            Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

            string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
            {
                cycleData = data.ToDictionary(entry => entry.Item1);
                return message;
            }

            Assert.Multiple(() =>
            {
                Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort(formatter));

                Assert.AreEqual(3, cycleData.Count);

                Assert.AreEqual(vertexTwo, cycleData[vertexOne].Item2);
                Assert.AreEqual(new[] { edgeOne }, cycleData[vertexOne].Item3);

                Assert.AreEqual(vertexThree, cycleData[vertexTwo].Item2);
                Assert.AreEqual(new[] { edgeTwo }, cycleData[vertexTwo].Item3);

                Assert.AreEqual(vertexOne, cycleData[vertexThree].Item2);
                Assert.AreEqual(new[] { edgeThree }, cycleData[vertexThree].Item3);
            });
        }

        [Test]
        public static void BatchingTopologicalSort_throws_with_formatted_message_when_cycle_cannot_be_broken()
        {
            const string message = "Formatted cycle";

            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree });

            // 1 -> {2}
            graph.AddEdge(vertexOne, vertexTwo, edgeOne);
            // 2 -> {3}
            graph.AddEdge(vertexTwo, vertexThree, edgeTwo);
            // 3 -> {1}
            graph.AddEdge(vertexThree, vertexOne, edgeThree);

            Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

            string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
            {
                cycleData = data.ToDictionary(entry => entry.Item1);
                return message;
            }

        Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort(formatter));

            Assert.AreEqual(3, cycleData.Count);

            Assert.AreEqual(vertexTwo, cycleData[vertexOne].Item2);
            Assert.AreEqual(new[] { edgeOne }, cycleData[vertexOne].Item3);

            Assert.AreEqual(vertexThree, cycleData[vertexTwo].Item2);
            Assert.AreEqual(new[] { edgeTwo }, cycleData[vertexTwo].Item3);

            Assert.AreEqual(vertexOne, cycleData[vertexThree].Item2);
            Assert.AreEqual(new[] { edgeThree }, cycleData[vertexThree].Item3);
        }

        [Test]
        public static void BatchingTopologicalSort_throws_with_formatted_message_with_no_tail_when_cycle_cannot_be_broken()
        {
            const string message = "Formatted cycle";

            var vertexOne = new Vertex
            {
                Id = 1
            };
            var vertexTwo = new Vertex
            {
                Id = 2
            };
            var vertexThree = new Vertex
            {
                Id = 3
            };
            var vertexFour = new Vertex
            {
                Id = 4
            };

            var edgeOne = new Edge
            {
                Id = 1
            };
            var edgeTwo = new Edge
            {
                Id = 2
            };
            var edgeThree = new Edge
            {
                Id = 3
            };
            var edgeFour = new Edge
            {
                Id = 4
            };

            var graph = new Multigraph<Vertex, Edge>();
            graph.AddVertices(new[] { vertexOne, vertexTwo, vertexThree, vertexFour });

            // 2 -> {1}
            graph.AddEdge(vertexTwo, vertexOne, edgeOne);
            // 3 -> {2}
            graph.AddEdge(vertexThree, vertexTwo, edgeTwo);
            // 4 -> {3}
            graph.AddEdge(vertexFour, vertexThree, edgeThree);
            // 3 -> {4}
            graph.AddEdge(vertexThree, vertexFour, edgeFour);

            Dictionary<Vertex, Tuple<Vertex, Vertex, IEnumerable<Edge>>> cycleData = null;

            string formatter(IEnumerable<Tuple<Vertex, Vertex, IEnumerable<Edge>>> data)
            {
                cycleData = data.ToDictionary(entry => entry.Item1);
                return message;
            }

            Assert.Throws<InvalidOperationException>(() => graph.BatchingTopologicalSort(formatter));

            Assert.AreEqual(2, cycleData.Count);

            Assert.AreEqual(vertexFour, cycleData[vertexThree].Item2);
            Assert.AreEqual(new[] { edgeFour }, cycleData[vertexThree].Item3);

            Assert.AreEqual(vertexThree, cycleData[vertexFour].Item2);
            Assert.AreEqual(new[] { edgeThree }, cycleData[vertexFour].Item3);
        }
    }
}
#pragma warning restore S125 // Sections of code should not be commented out
