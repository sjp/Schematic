using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// A table partitioning implementation, describing how a table's rows are distributed across
/// partitions.
/// </summary>
/// <seealso cref="ITablePartitioning" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class TablePartitioning : ITablePartitioning
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TablePartitioning"/> class.
    /// </summary>
    /// <param name="strategy">How rows are assigned to a partition.</param>
    /// <param name="columns">The ordered partitioning key columns. May be empty when the database does not report them.</param>
    /// <param name="partitions">The partitions the table is split into. May be empty when the database does not report them.</param>
    /// <exception cref="ArgumentNullException"><paramref name="strategy"/> is <see langword="null" />, or <paramref name="columns"/> or <paramref name="partitions"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="strategy"/> is empty or whitespace.</exception>
    public TablePartitioning(string strategy, IReadOnlyList<IDatabaseColumn> columns, IReadOnlyCollection<Identifier> partitions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(strategy);

        Strategy = strategy;
        Columns = columns.ToDefensiveCopy(nameof(columns));
        Partitions = partitions.ToDefensiveCopy(nameof(partitions));
    }

    /// <summary>
    /// How rows are assigned to a partition.
    /// </summary>
    /// <value>A partitioning strategy.</value>
    public string Strategy { get; }

    /// <summary>
    /// The ordered list of columns whose values determine the partition a row belongs to.
    /// </summary>
    /// <value>The partitioning key columns.</value>
    public IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// The partitions the table is split into.
    /// </summary>
    /// <value>Partition names.</value>
    public IReadOnlyCollection<Identifier> Partitions { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay => "Partitioning: " + Strategy;
}
