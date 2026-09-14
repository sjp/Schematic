using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests.Integration;

/// <summary>
/// A statistics provider returning a fixed set of statistics, standing in for the statistics a
/// database records for its tables.
/// </summary>
internal sealed class FakeTableStatisticsProvider : ITableStatisticsProvider
{
    public FakeTableStatisticsProvider(params ITableStatistics[] statistics)
    {
        _statistics = statistics;
    }

    public OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var statistics = _statistics.FirstOrDefault(s => s.TableName == tableName);
        return statistics != null
            ? OptionAsync<ITableStatistics>.Some(statistics)
            : OptionAsync<ITableStatistics>.None;
    }

    public Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<ITableStatistics>>(_statistics);

    private readonly IReadOnlyCollection<ITableStatistics> _statistics;
}
