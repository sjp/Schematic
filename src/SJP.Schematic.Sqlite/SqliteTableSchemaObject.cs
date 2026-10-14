namespace SJP.Schematic.Sqlite;

/// <summary>
/// A row of a schema's <c>sqlite_master</c> table that belongs to a table: the table itself, one of its indexes, or one of its triggers.
/// </summary>
public sealed record SqliteTableSchemaObject
{
    /// <summary>
    /// The type of object. One of <c>table</c>, <c>index</c> or <c>trigger</c>.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The name of the object.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The SQL text that created the object. This is <see langword="null" /> for an index that SQLite creates to enforce a <c>PRIMARY KEY</c> or <c>UNIQUE</c> constraint.
    /// </summary>
    public string? Sql { get; init; }
}
