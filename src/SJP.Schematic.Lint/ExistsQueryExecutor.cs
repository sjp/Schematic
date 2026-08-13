using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint;

/// <summary>
/// Executes cheap <c>EXISTS</c>-style probes, avoiding full scans/counts. Used by rules that only
/// need to know whether any row matches a filter (e.g. whether a table has any rows at all).
/// </summary>
internal sealed class ExistsQueryExecutor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExistsQueryExecutor"/> class.
    /// </summary>
    /// <param name="connection">A database connection, qualified with a dialect.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public ExistsQueryExecutor(ISchematicConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        _fromQuerySuffixAsync = new AsyncLazy<string>(GetFromQuerySuffixAsync);
    }

    private ISchematicConnection Connection { get; }

    private IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// Determines whether any row matches the given filter query.
    /// </summary>
    /// <param name="filterSql">A query whose existence of any resulting row is being tested, e.g. <c>select 1 from some_table</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if any row is returned by <paramref name="filterSql"/>; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="filterSql"/> is <see langword="null" />, empty or whitespace.</exception>
    public Task<bool> ExistsAsync(string filterSql, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterSql);

        return ExistsAsyncCore(filterSql, cancellationToken);
    }

    private async Task<bool> ExistsAsyncCore(string filterSql, CancellationToken cancellationToken)
    {
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        var suffix = await _fromQuerySuffixAsync;
        var query = suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;

        return await DbConnection.ExecuteScalarAsync<bool>(query, cancellationToken);
    }

    private async Task<string> GetFromQuerySuffixAsync()
    {
        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryNoTable, CancellationToken.None);
            return string.Empty;
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromSysDual, CancellationToken.None);
            return "SYS.DUAL";
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromDual, CancellationToken.None);
        return "DUAL";
    }

    private const string TestQueryNoTable = "select 1 as dummy";
    private const string TestQueryFromDual = "select 1 as dummy from DUAL";
    private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

    private readonly AsyncLazy<string> _fromQuerySuffixAsync;
}
