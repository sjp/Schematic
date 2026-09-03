using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A table statistics provider that returns no statistics. Used where a database engine records
/// none, or where a consumer was given no provider.
/// </summary>
/// <seealso cref="ITableStatisticsProvider" />
public sealed class EmptyTableStatisticsProvider : ITableStatisticsProvider
{
    /// <summary>
    /// Gets the statistics recorded for a database table. This will always be 'none'.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A 'none' value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return OptionAsync<ITableStatistics>.None;
    }

    /// <summary>
    /// Gets the statistics recorded for all database tables. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of table statistics.</returns>
    public Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default) => Empty.Tasks.TableStatistics;
}
