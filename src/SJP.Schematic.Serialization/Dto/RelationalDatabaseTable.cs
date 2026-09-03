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

    /// <summary>
    /// What the table is, where that differs from an ordinary persistent table.
    /// </summary>
    public Core.TableKind Kind { get; init; }

    /// <summary>
    /// How the table's rows are distributed across partitions. Null when the table is not partitioned.
    /// </summary>
    public TablePartitioning? Partitioning { get; init; }

    /// <summary>
    /// Where the table's superseded rows are retained. Null when the table is not system-versioned.
    /// </summary>
    public TableSystemVersioning? SystemVersioning { get; init; }

    /// <summary>
    /// Whether writes to the table are written to the database's transaction log.
    /// </summary>
    /// <remarks>
    /// Not required, and defaults to <see langword="true" />, so that a document written before
    /// tables carried storage metadata still reads back as an ordinary logged table.
    /// </remarks>
    public bool IsLogged { get; init; } = true;

    /// <summary>
    /// The default collation applied to the table's character data. Null when the database records
    /// no collation for the table as a whole.
    /// </summary>
    public Identifier? Collation { get; init; }
}
