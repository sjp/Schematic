﻿using System;
using System.ComponentModel;
using System.Security;
using EnumsNET;

namespace SJP.Schematic.Dot;

internal sealed class NodeAttribute : IEquatable<NodeAttribute>
{
    private NodeAttribute(string attrName, string attrValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attrName);
        ArgumentException.ThrowIfNullOrWhiteSpace(attrValue);

        _attr = attrName + "=" + attrValue;
        _hashCode = _attr.GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString() => _attr;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => _hashCode;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;

        return obj is NodeAttribute attr && Equals(attr);
    }

    public bool Equals(NodeAttribute? other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(_attr, other.ToString(), StringComparison.Ordinal);
    }

    public static NodeAttribute URL(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var uriStr = uri.ToString();
        var encodedUri = SecurityElement.Escape(uriStr);

        return new NodeAttribute("URL", "\"" + encodedUri + "\"");
    }

    public static NodeAttribute Tooltip(string tooltip)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tooltip);

        var escapedTooltip = "\"" + tooltip.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
        return new NodeAttribute("tooltip", escapedTooltip);
    }

    public static NodeAttribute FontFace(FontFace fontFace)
    {
        if (!fontFace.IsValid())
            throw new ArgumentException($"The {nameof(FontFace)} provided must be a valid enum.", nameof(fontFace));

        var fontFaceStr = "\"" + fontFace.AsString() + "\"";
        return new NodeAttribute("fontname", fontFaceStr);
    }

    public static NodeAttribute EmptyNodeShape() => new("shape", "none");

    private readonly string _attr;
    private readonly int _hashCode;
}