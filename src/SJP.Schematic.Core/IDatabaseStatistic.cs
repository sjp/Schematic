using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// Represents a database statistic.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseStatistic : IDatabaseEntity
{
    /// <summary>
    /// A collection of database columns that are covered by the statistic.
    /// </summary>
    /// <value>Columns covered by the statistic.</value>
    IReadOnlyCollection<IDatabaseColumn> Columns { get; }
}
