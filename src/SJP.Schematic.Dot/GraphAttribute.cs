﻿using System;
using System.ComponentModel;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot;

internal sealed class GraphAttribute : IEquatable<GraphAttribute>
{
    private GraphAttribute(string attrName, string attrValue)
    {
        if (attrName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(attrName));
        if (attrValue.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(attrValue));

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

        return obj is GraphAttribute attr && Equals(attr);
    }

    public bool Equals(GraphAttribute? other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(_attr, other.ToString(), StringComparison.Ordinal);
    }

    public static GraphAttribute RankDirection(RankDirection rankDir)
    {
        if (!rankDir.IsValid())
            throw new ArgumentException($"The {nameof(RankDirection)} provided must be a valid enum.", nameof(rankDir));

        var rankDirStr = rankDir.AsString();
        return new GraphAttribute("rankdir", rankDirStr);
    }

    public static GraphAttribute BackgroundColor(RgbColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        return new GraphAttribute("bgcolor", "\"" + color + "\"");
    }

    public static GraphAttribute Ratio(GraphRatio ratio)
    {
        if (!ratio.IsValid())
            throw new ArgumentException($"The {nameof(GraphRatio)} provided must be a valid enum.", nameof(ratio));

        var ratioStr = ratio.AsString().ToLowerInvariant();
        return new GraphAttribute("ratio", ratioStr);
    }

    private readonly string _attr;
    private readonly int _hashCode;
}