using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// The columns already mapped for one table or view, used to read the copies of those columns
/// that keys, indexes and foreign keys carry as the same instances instead of new ones.
/// </summary>
/// <remarks>
/// A copy is matched by name alone, ordinally. When a copy matches, the object's column is used and
/// the rest of the copy is not read, so a document in which the two disagree reads back with the
/// object's column definition. A copy that matches no column, such as one naming a column the
/// object does not list, is mapped as a column of its own.
/// </remarks>
internal sealed class ColumnLookup
{
    /// <summary>
    /// A lookup containing no columns, so every copy is mapped as a column of its own.
    /// </summary>
    public static ColumnLookup Empty { get; } = new([]);

    public ColumnLookup(IReadOnlyList<IDatabaseColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        Columns = columns;
        _columnsByName = new Dictionary<string, IDatabaseColumn>(columns.Count, StringComparer.Ordinal);
        foreach (var column in columns)
            _columnsByName.TryAdd(column.Name.LocalName, column);
    }

    /// <summary>
    /// The object's columns, in their original order.
    /// </summary>
    public IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// Returns the object's column named by <paramref name="source"/>, or maps the copy when there is none.
    /// </summary>
    public IDatabaseColumn Resolve(Dto.DatabaseColumn source)
    {
        var name = source.ColumnName;
        if (_columnsByName.TryGetValue(name.LocalName, out var column)
            && string.Equals(column.Name.Schema, name.Schema, StringComparison.Ordinal)
            && string.Equals(column.Name.Database, name.Database, StringComparison.Ordinal)
            && string.Equals(column.Name.Server, name.Server, StringComparison.Ordinal))
        {
            return column;
        }

        return MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>().Map(source);
    }

    /// <summary>
    /// Resolves each copy in <paramref name="source"/>, keeping their order.
    /// </summary>
    public List<IDatabaseColumn> ResolveList(IEnumerable<Dto.DatabaseColumn> source)
    {
        var result = source is IReadOnlyCollection<Dto.DatabaseColumn> collection
            ? new List<IDatabaseColumn>(collection.Count)
            : [];

        foreach (var column in source)
            result.Add(Resolve(column));

        return result;
    }

    private readonly Dictionary<string, IDatabaseColumn> _columnsByName;
}
