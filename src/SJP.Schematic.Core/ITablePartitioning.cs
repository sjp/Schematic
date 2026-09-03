using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes how a table's rows are distributed across partitions.
/// </summary>
public interface ITablePartitioning
{
    /// <summary>
    /// How rows are assigned to a partition, e.g. <c>RANGE</c>, <c>LIST</c>, <c>HASH</c>, or the
    /// name of a SQL Server partition scheme.
    /// </summary>
    /// <value>A partitioning strategy.</value>
    string Strategy { get; }

    /// <summary>
    /// The ordered list of columns whose values determine the partition a row belongs to.
    /// </summary>
    /// <value>The partitioning key columns. Empty when the database does not report them.</value>
    IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// The partitions the table is split into.
    /// </summary>
    /// <value>
    /// Partition names. These are table names where the database exposes partitions as tables in
    /// their own right, e.g. PostgreSQL; otherwise they are names local to the table. Empty when
    /// the database does not report them.
    /// </value>
    IReadOnlyCollection<Identifier> Partitions { get; }
}
