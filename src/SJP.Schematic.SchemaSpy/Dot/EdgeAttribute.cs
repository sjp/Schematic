using EnumsNET;
using SJP.Schematic.Core.Extensions;
using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal sealed class EdgeAttribute : IEquatable<EdgeAttribute>
    {
        public EdgeAttribute(string attrName, string attrValue)
        {
            if (attrName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrName));
            if (attrValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrValue));

            _attr = attrName.ToLowerInvariant() + "=" + attrValue.ToLowerInvariant();
        }

        public override string ToString() => _attr;

        public override int GetHashCode() => _attr.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as EdgeAttribute);
        }

        public bool Equals(EdgeAttribute other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _attr == other.ToString();
        }

        public static EdgeAttribute ArrowHead(ArrowStyleName arrowStyle)
        {
            if (!arrowStyle.IsValid())
                throw new ArgumentException($"The { nameof(ArrowStyleName) } provided must be a valid enum.", nameof(arrowStyle));

            var arrowStyleStr = arrowStyle.AsString().ToLowerInvariant();
            return new EdgeAttribute("arrowhead", arrowStyleStr);
        }

        public static EdgeAttribute ArrowTail(ArrowStyleName arrowStyle)
        {
            if (!arrowStyle.IsValid())
                throw new ArgumentException($"The { nameof(ArrowStyleName) } provided must be a valid enum.", nameof(arrowStyle));

            var arrowStyleStr = arrowStyle.AsString().ToLowerInvariant();
            return new EdgeAttribute("arrowtail", arrowStyleStr);
        }

        public static EdgeAttribute Direction(EdgeDirection direction)
        {
            if (!direction.IsValid())
                throw new ArgumentException($"The { nameof(EdgeDirection) } provided must be a valid enum.", nameof(direction));

            var directionStr = direction.AsString().ToLowerInvariant();
            return new EdgeAttribute("dir", directionStr);
        }

        private readonly string _attr;
    }
}
