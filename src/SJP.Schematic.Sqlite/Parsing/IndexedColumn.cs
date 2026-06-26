using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// The parsed definition of an index column in a SQLite <c>CREATE TABLE</c> definition.
/// </summary>
public class IndexedColumn
{
    internal IndexedColumn(string? name, string expression, SqliteCollation collation, IndexColumnOrder columnOrder)
    {
        Name = name;
        Expression = expression ?? string.Empty;
        Collation = collation;
        ColumnOrder = columnOrder;
    }

    internal IndexedColumn(string identifier)
        : this(ValidateIdentifier(identifier), string.Empty, SqliteCollation.None, IndexColumnOrder.Ascending)
    {
    }

    /// <summary>
    /// The indexed column name.
    /// </summary>
    /// <value>The indexed column name.</value>
    public string? Name { get; }

    /// <summary>
    /// A SQL expression representing the indexing on a column.
    /// </summary>
    /// <value>A SQL expression, or an empty string when the column is a simple column reference.</value>
    public string Expression { get; }

    /// <summary>
    /// Indexing collation.
    /// </summary>
    /// <value>A collation.</value>
    public SqliteCollation Collation { get; }

    /// <summary>
    /// The column ordering for indexing.
    /// </summary>
    /// <value>A column order.</value>
    public IndexColumnOrder ColumnOrder { get; }

    private static string ValidateIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        return identifier;
    }
}
