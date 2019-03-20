using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class GraphAttribute : IEquatable<GraphAttribute>
    {
        private GraphAttribute(string attrName, string attrValue)
        {
            if (attrName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrName));
            if (attrValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrValue));

            _attr = attrName + "=" + attrValue;
            _hashCode = _attr.GetHashCode();
        }

        public override string ToString() => _attr;

        public override int GetHashCode() => _hashCode;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as GraphAttribute);
        }

        public bool Equals(GraphAttribute other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _attr == other.ToString();
        }

        public static GraphAttribute RankDirection(RankDirection rankDir)
        {
            if (!rankDir.IsValid())
                throw new ArgumentException($"The { nameof(RankDirection) } provided must be a valid enum.", nameof(rankDir));

            var rankDirStr = rankDir.AsString();
            return new GraphAttribute("rankdir", rankDirStr);
        }

        public static GraphAttribute BackgroundColor(RgbColor color)
        {
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            return new GraphAttribute("bgcolor", "\"" + color + "\"");
        }

        public static GraphAttribute Ratio(GraphRatio ratio)
        {
            if (!ratio.IsValid())
                throw new ArgumentException($"The { nameof(GraphRatio) } provided must be a valid enum.", nameof(ratio));

            var ratioStr = ratio.AsString().ToLowerInvariant();
            return new GraphAttribute("ratio", ratioStr);
        }

        private readonly string _attr;
        private readonly int _hashCode;
    }
}
