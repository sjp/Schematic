using System;
using System.Collections.Generic;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// Compares object names reduced to a schema and a local name. Report links are resolved on those
/// two parts alone, because a name that a database reports for one purpose (a column's type, a
/// view's dependency) carries fewer parts than the name the same object is listed under, and
/// comparing the parts that are missing would stop every such name from ever resolving.
/// </summary>
internal sealed class SchemaLocalNameComparer : IEqualityComparer<(string? Schema, string LocalName)>
{
    private readonly StringComparer _comparer;

    private SchemaLocalNameComparer(StringComparer comparer) => _comparer = comparer;

    /// <summary>Compares both parts case-sensitively.</summary>
    public static SchemaLocalNameComparer Ordinal { get; } = new(StringComparer.Ordinal);

    /// <summary>Compares both parts ignoring case.</summary>
    public static SchemaLocalNameComparer OrdinalIgnoreCase { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Equals((string? Schema, string LocalName) x, (string? Schema, string LocalName) y)
    {
        return _comparer.Equals(x.Schema, y.Schema)
            && _comparer.Equals(x.LocalName, y.LocalName);
    }

    public int GetHashCode((string? Schema, string LocalName) obj)
    {
        var schemaHash = obj.Schema is null ? 0 : _comparer.GetHashCode(obj.Schema);
        return HashCode.Combine(schemaHash, _comparer.GetHashCode(obj.LocalName));
    }
}
