using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized database table.
/// </summary>
public sealed record RelationalDatabaseTable
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public required Identifier TableName { get; init; }

    /// <summary>
    /// The primary key of the table, if it has one.
    /// </summary>
    public DatabaseKey? PrimaryKey { get; init; }

    /// <summary>
    /// The columns of the table, in ordinal order.
    /// </summary>
    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    /// <summary>
    /// The check constraints defined on the table.
    /// </summary>
    public required IEnumerable<DatabaseCheckConstraint> Checks { get; init; }

    /// <summary>
    /// The indexes defined on the table.
    /// </summary>
    public required IEnumerable<DatabaseIndex> Indexes { get; init; }

    /// <summary>
    /// The unique keys defined on the table.
    /// </summary>
    public required IEnumerable<DatabaseKey> UniqueKeys { get; init; }

    /// <summary>
    /// The foreign keys defined on the table, i.e. the relationships in which this table is the child.
    /// </summary>
    public required IEnumerable<DatabaseRelationalKey> ParentKeys { get; init; }

    /// <summary>
    /// The foreign keys pointing at this table, i.e. the relationships in which this table is the parent.
    /// </summary>
    public required IEnumerable<DatabaseRelationalKey> ChildKeys { get; init; }

    /// <summary>
    /// The triggers defined on the table.
    /// </summary>
    public required IEnumerable<DatabaseTrigger> Triggers { get; init; }
}
