using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized description of how a table's rows are distributed across partitions.
/// </summary>
public sealed record TablePartitioning
{
    /// <summary>
    /// How rows are assigned to a partition.
    /// </summary>
    public required string Strategy { get; init; }

    /// <summary>
    /// The names of the columns whose values determine the partition a row belongs to, in order.
    /// </summary>
    /// <remarks>Names rather than columns, so that a partitioning key refers to the table's own columns.</remarks>
    public required IEnumerable<Identifier> Columns { get; init; }

    /// <summary>
    /// The names of the partitions the table is split into.
    /// </summary>
    public required IEnumerable<Identifier> Partitions { get; init; }
}
