using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Implements the statistics a database records for a table.
/// </summary>
/// <seealso cref="ITableStatistics" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class TableStatistics : ITableStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableStatistics"/> class.
    /// </summary>
    /// <param name="tableName">The name of the table the statistics describe.</param>
    /// <param name="rowCount">The number of rows in the table, if known.</param>
    /// <param name="isExact">Whether <paramref name="rowCount"/> is exact rather than an estimate.</param>
    /// <param name="dataSizeBytes">The space occupied by the table's rows, if known.</param>
    /// <param name="indexSizeBytes">The space occupied by the table's indexes, if known.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public TableStatistics(
        Identifier tableName,
        Option<long> rowCount,
        bool isExact,
        Option<long> dataSizeBytes,
        Option<long> indexSizeBytes
    )
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        RowCount = rowCount;
        IsExact = isExact;
        DataSizeBytes = dataSizeBytes;
        IndexSizeBytes = indexSizeBytes;
    }

    /// <summary>
    /// The name of the table these statistics describe.
    /// </summary>
    /// <value>A table name.</value>
    public Identifier TableName { get; }

    /// <summary>
    /// The number of rows in the table, when the database records one.
    /// </summary>
    /// <value>A row count, if available.</value>
    public Option<long> RowCount { get; }

    /// <summary>
    /// Whether <see cref="RowCount"/> is the exact number of rows in the table rather than an estimate.
    /// </summary>
    /// <value><see langword="true" /> if the row count is exact; otherwise <see langword="false" />.</value>
    public bool IsExact { get; }

    /// <summary>
    /// The space occupied by the table's rows, in bytes, when the database records it.
    /// </summary>
    /// <value>A size in bytes, if available.</value>
    public Option<long> DataSizeBytes { get; }

    /// <summary>
    /// The space occupied by the table's indexes, in bytes, when the database records it.
    /// </summary>
    /// <value>A size in bytes, if available.</value>
    public Option<long> IndexSizeBytes { get; }

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
            var rowCount = RowCount.Match(
                count => count.ToString(CultureInfo.InvariantCulture),
                static () => "unknown"
            );

            return "Statistics: " + TableName.LocalName + ", Rows: " + rowCount;
        }
    }
}
