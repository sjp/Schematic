namespace SJP.Schematic.Core;

/// <summary>
/// Describes what a table is, where that differs from an ordinary persistent table.
/// </summary>
public enum TableKind
{
    /// <summary>
    /// An ordinary persistent table.
    /// </summary>
    Regular,

    /// <summary>
    /// A temporary table, whose rows live no longer than a session or a transaction.
    /// </summary>
    Temporary,

    /// <summary>
    /// A partitioned table that holds no rows itself; its rows live in its partitions.
    /// </summary>
    PartitionParent,

    /// <summary>
    /// A partition of a partitioned table.
    /// </summary>
    Partition,

    /// <summary>
    /// The history table of a system-versioned table, holding rows superseded by later updates.
    /// </summary>
    History,

    /// <summary>
    /// A virtual table, whose contents are supplied by a module rather than by ordinary storage,
    /// e.g. a SQLite FTS5 or R*Tree table.
    /// </summary>
    Virtual,

    /// <summary>
    /// A table whose data is stored outside the database, e.g. an Oracle external table.
    /// </summary>
    External,

    /// <summary>
    /// A table stored in the structure of its primary key index, e.g. an Oracle index-organized
    /// table or a SQLite <c>WITHOUT ROWID</c> table.
    /// </summary>
    IndexOrganized,
}
