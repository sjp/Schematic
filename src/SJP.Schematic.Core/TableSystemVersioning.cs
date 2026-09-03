using System;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A system versioning implementation, describing where a table's superseded rows are retained.
/// </summary>
/// <seealso cref="ITableSystemVersioning" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class TableSystemVersioning : ITableSystemVersioning
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableSystemVersioning"/> class.
    /// </summary>
    /// <param name="historyTable">The table holding rows superseded by later updates.</param>
    /// <param name="periodStartColumn">The column recording when a row's version became current.</param>
    /// <param name="periodEndColumn">The column recording when a row's version stopped being current.</param>
    /// <exception cref="ArgumentNullException"><paramref name="historyTable"/>, <paramref name="periodStartColumn"/> or <paramref name="periodEndColumn"/> is <see langword="null" />.</exception>
    public TableSystemVersioning(Identifier historyTable, Identifier periodStartColumn, Identifier periodEndColumn)
    {
        HistoryTable = historyTable ?? throw new ArgumentNullException(nameof(historyTable));
        PeriodStartColumn = periodStartColumn ?? throw new ArgumentNullException(nameof(periodStartColumn));
        PeriodEndColumn = periodEndColumn ?? throw new ArgumentNullException(nameof(periodEndColumn));
    }

    /// <summary>
    /// The table holding rows superseded by later updates.
    /// </summary>
    /// <value>A history table name.</value>
    public Identifier HistoryTable { get; }

    /// <summary>
    /// The column recording when a row's version became current.
    /// </summary>
    /// <value>A period start column name.</value>
    public Identifier PeriodStartColumn { get; }

    /// <summary>
    /// The column recording when a row's version stopped being current.
    /// </summary>
    /// <value>A period end column name.</value>
    public Identifier PeriodEndColumn { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("System versioning: history table ");

            if (!HistoryTable.Schema.IsNullOrWhiteSpace())
                builder.Append(HistoryTable.Schema).Append('.');

            builder.Append(HistoryTable.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
