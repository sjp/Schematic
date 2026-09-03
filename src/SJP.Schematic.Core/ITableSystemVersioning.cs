namespace SJP.Schematic.Core;

/// <summary>
/// Describes the system versioning applied to a table, i.e. the database retaining superseded
/// rows so that the table can be queried as it was at a point in time.
/// </summary>
public interface ITableSystemVersioning
{
    /// <summary>
    /// The table holding rows superseded by later updates.
    /// </summary>
    /// <value>A history table name.</value>
    Identifier HistoryTable { get; }

    /// <summary>
    /// The column recording when a row's version became current.
    /// </summary>
    /// <value>A period start column name.</value>
    Identifier PeriodStartColumn { get; }

    /// <summary>
    /// The column recording when a row's version stopped being current.
    /// </summary>
    /// <value>A period end column name.</value>
    Identifier PeriodEndColumn { get; }
}
