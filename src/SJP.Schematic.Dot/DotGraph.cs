using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dot
{
    internal sealed class DotGraph
    {
        public DotGraph(
            DotIdentifier graphName,
            IEnumerable<GraphAttribute> graphAttrs,
            IEnumerable<NodeAttribute> nodeAttrs,
            IEnumerable<EdgeAttribute> edgeAttrs,
            IEnumerable<DotNode> nodes,
            IEnumerable<DotEdge> edges)
        {
            GraphName = graphName ?? throw new ArgumentNullException(nameof(graphName));
            GraphAttributes = graphAttrs ?? throw new ArgumentNullException(nameof(graphAttrs));
            NodeAttributes = nodeAttrs ?? throw new ArgumentNullException(nameof(nodeAttrs));
            EdgeAttributes = edgeAttrs ?? throw new ArgumentNullException(nameof(edgeAttrs));
            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            Edges = edges ?? throw new ArgumentNullException(nameof(edges));

            _dotBuilder = new Lazy<string>(BuildDot);
        }

        public DotIdentifier GraphName { get; }

        public IEnumerable<GraphAttribute> GraphAttributes { get; }

        public IEnumerable<NodeAttribute> NodeAttributes { get; }

        public IEnumerable<EdgeAttribute> EdgeAttributes { get; }

        public IEnumerable<DotNode> Nodes { get; }

        public IEnumerable<DotEdge> Edges { get; }

        public override string ToString() => _dotBuilder.Value;

        private string BuildDot()
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("// Schematic version ").AppendLine(_fileVersion);
            builder.Append("digraph ").Append(GraphName).AppendLine(" {");

            const uint level = 1U;
            if (GraphAttributes.Any())
            {
                var graphIndent = GetIndentForLevel(level);
                builder.Append(graphIndent).AppendLine("graph [");

                var graphAttrIndent = GetIndentForLevel(level + 1);
                foreach (var graphAttr in GraphAttributes)
                    builder.Append(graphAttrIndent).AppendLine(graphAttr.ToString());

                builder.Append(graphIndent).AppendLine("]");
            }

            if (NodeAttributes.Any())
            {
                var nodeIndent = GetIndentForLevel(level);
                builder.Append(nodeIndent).AppendLine("node [");

                var nodeAttrIndent = GetIndentForLevel(level + 1);
                foreach (var nodeAttr in NodeAttributes)
                    builder.Append(nodeAttrIndent).AppendLine(nodeAttr.ToString());

                builder.Append(nodeIndent).AppendLine("]");
            }

            if (EdgeAttributes.Any())
            {
                var edgeIndent = GetIndentForLevel(level);
                builder.Append(edgeIndent).AppendLine("edge [");

                var edgeAttrIndent = GetIndentForLevel(level + 1);
                foreach (var edgeAttr in EdgeAttributes)
                    builder.Append(edgeAttrIndent).AppendLine(edgeAttr.ToString());

                builder.Append(edgeIndent).AppendLine("]");
            }

            if (Nodes.Any())
            {
                var nodeIndent = GetIndentForLevel(level);
                var orderedNodes = Nodes.OrderBy(n => n.Identifier.ToString());

                foreach (var node in orderedNodes)
                {
                    var nodeStr = node.ToString();

                    using var reader = new StringReader(nodeStr);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                        builder.Append(nodeIndent).AppendLine(line);
                }
            }

            if (Edges.Any())
            {
                var edgeIndent = GetIndentForLevel(level);
                foreach (var edge in Edges)
                {
                    var edgeStr = edge.ToString();

                    using var reader = new StringReader(edgeStr);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                        builder.Append(edgeIndent).AppendLine(line);
                }
            }

            builder.AppendLine("}");
            return builder.GetStringAndRelease();
        }

        private static string GetIndentForLevel(uint level) => new string(' ', (int)(level * 2));

        private readonly Lazy<string> _dotBuilder;
        private static readonly string _fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? string.Empty;
    }
}
