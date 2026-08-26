using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database table and to the objects defined on it.
/// </summary>
/// <remarks>
/// The nested lookups are keyed by a name that is local to the table, e.g. a column name rather than a
/// qualified one. A <see langword="null"/> value means the object exists but carries no comment.
/// </remarks>
public class DatabaseTableComments
{
    /// <summary>
    /// The name of the table the comments are attached to.
    /// </summary>
    public required Identifier TableName { get; init; }

    /// <summary>
    /// The comment attached to the table, if any.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// The comment attached to the table's primary key, if any.
    /// </summary>
    public string? PrimaryKeyComment { get; init; }

    /// <summary>
    /// The comments attached to the table's columns, keyed by column name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> ColumnComments { get; init; }

    /// <summary>
    /// The comments attached to the table's check constraints, keyed by constraint name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> CheckComments { get; init; }

    /// <summary>
    /// The comments attached to the table's unique keys, keyed by key name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> UniqueKeyComments { get; init; }

    /// <summary>
    /// The comments attached to the table's foreign keys, keyed by key name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> ForeignKeyComments { get; init; }

    /// <summary>
    /// The comments attached to the table's indexes, keyed by index name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> IndexComments { get; init; }

    /// <summary>
    /// The comments attached to the table's triggers, keyed by trigger name.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> TriggerComments { get; init; }
}
