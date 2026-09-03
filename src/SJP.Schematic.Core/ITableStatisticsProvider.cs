using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a provider that retrieves the statistics a database records for its tables.
/// </summary>
/// <remarks>
/// Statistics are dynamic data, so they are not part of <see cref="IRelationalDatabase"/> and are
/// not held in a snapshot. A provider is obtained separately, from
/// <see cref="IRelationalDatabaseProvider.GetTableStatisticsProviderAsync(CancellationToken)"/>,
/// and consumers treat it as optional.
/// </remarks>
public interface ITableStatisticsProvider
{
    /// <summary>
    /// Gets the statistics recorded for a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Statistics for the table in the 'some' state if the table is known; otherwise 'none'.</returns>
    OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the statistics recorded for all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of table statistics.</returns>
    Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default);
}
