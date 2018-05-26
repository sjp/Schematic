using EnumsNET;
using SJP.Schematic.Core.Extensions;
using System;
using System.Security;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal sealed class NodeAttribute : IEquatable<NodeAttribute>
    {
        private NodeAttribute(string attrName, string attrValue)
        {
            if (attrName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrName));
            if (attrValue.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(attrValue));

            _attr = attrName + "=" + attrValue;
        }

        public override string ToString() => _attr;

        public override int GetHashCode() => _attr.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as NodeAttribute);
        }

        public bool Equals(NodeAttribute other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _attr == other.ToString();
        }

        public static NodeAttribute URL(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var uriStr = uri.ToString();
            var encodedUri = SecurityElement.Escape(uriStr);

            return new NodeAttribute("URL", "\"" + encodedUri + "\"");
        }

        public static NodeAttribute Tooltip(string tooltip)
        {
            if (tooltip.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tooltip));

            var escapedTooltip = "\"" + tooltip.Replace("\"", "\\\"") + "\"";
            return new NodeAttribute("tooltip", escapedTooltip);
        }

        public static NodeAttribute FontFace(FontFace fontFace)
        {
            if (!fontFace.IsValid())
                throw new ArgumentException($"The { nameof(FontFace) } provided must be a valid enum.", nameof(fontFace));

            var fontFaceStr = "\"" + fontFace.AsString() + "\"";
            return new NodeAttribute("fontname", fontFaceStr);
        }

        public static NodeAttribute EmptyNodeShape() => new NodeAttribute("shape", "none");

        private readonly string _attr;
    }
}
