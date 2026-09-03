using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.Lint;

/// <summary>
/// Serves statistics that were already retrieved for the report, so that the rules using them run
/// no queries of their own and cannot fail on a database that refuses to report statistics.
/// </summary>
/// <seealso cref="ITableStatisticsProvider" />
internal sealed class PreloadedTableStatisticsProvider : ITableStatisticsProvider
{
    public PreloadedTableStatisticsProvider(IReadOnlyDictionary<Identifier, ITableStatistics> statistics)
    {
        _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }

    public OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return _statistics.TryGetValue(tableName, out var statistics)
            ? OptionAsync<ITableStatistics>.Some(statistics)
            : OptionAsync<ITableStatistics>.None;
    }

    public Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<ITableStatistics>>(_statistics.Values.ToList());

    private readonly IReadOnlyDictionary<Identifier, ITableStatistics> _statistics;
}
