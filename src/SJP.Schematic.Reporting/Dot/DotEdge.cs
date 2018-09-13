using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class DotEdge
    {
        public DotEdge(DotIdentifier sourceNode, DotIdentifier sourcePort, DotIdentifier targetNode, DotIdentifier targetPort, IEnumerable<EdgeAttribute> edgeAttrs)
            : this(sourceNode, targetNode, edgeAttrs)
        {
            SourcePort = sourcePort ?? throw new ArgumentNullException(nameof(sourcePort));
            TargetPort = targetPort ?? throw new ArgumentNullException(nameof(targetPort));
        }

        public DotEdge(DotIdentifier sourceNode, DotIdentifier targetNode, IEnumerable<EdgeAttribute> edgeAttrs)
        {
            SourceNode = sourceNode ?? throw new ArgumentNullException(nameof(sourceNode));
            TargetNode = targetNode ?? throw new ArgumentNullException(nameof(targetNode));
            EdgeAttributes = edgeAttrs ?? throw new ArgumentNullException(nameof(edgeAttrs));

            _dotBuilder = new Lazy<string>(BuildDot);
        }

        private DotIdentifier SourceNode { get; }

        private DotIdentifier SourcePort { get; }

        private DotIdentifier TargetNode { get; }

        private DotIdentifier TargetPort { get; }

        private IEnumerable<EdgeAttribute> EdgeAttributes { get; }

        private string BuildDot()
        {
            var builder = StringBuilderCache.Acquire();

            var sourceIdentifier = SourcePort != null
                ? SourceNode + ":" + SourcePort
                : SourceNode.ToString();

            var targetIdentifier = TargetPort != null
                ? TargetNode + ":" + TargetPort
                : TargetNode.ToString();

            builder.Append(sourceIdentifier)
                .Append(" -> ")
                .Append(targetIdentifier);

            if (EdgeAttributes.Any())
            {
                const string indent = "  ";

                builder.AppendLine(" [");

                foreach (var edgeAttr in EdgeAttributes)
                    builder.Append(indent).AppendLine(edgeAttr.ToString());

                builder.Append("]");
            }

            return builder.GetStringAndRelease();
        }

        public override string ToString() => _dotBuilder.Value;

        private readonly Lazy<string> _dotBuilder;
    }
}
