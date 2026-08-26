using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
    /// Retrieves the executor associated with a connection, creating one when needed. Executors are
    /// shared so that the <c>FROM</c> suffix is discovered once per connection, rather than once per
    /// rule that needs it.
    /// </summary>
    /// <param name="connection">A database connection, qualified with a dialect.</param>
    /// <returns>An executor bound to <paramref name="connection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public static ExistsQueryExecutor GetForConnection(ISchematicConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return Executors.GetValue(connection, static c => new ExistsQueryExecutor(c));
    }

    private ExistsQueryExecutor(ISchematicConnection connection)
    {
        Connection = connection;

        _probeLimiter = ProbeConcurrencyLimiter.GetForConnection(connection);
    }

    private ISchematicConnection Connection { get; }

    private IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// Determines whether any row matches the given filter query.
    /// </summary>
    /// <param name="filterSql">A query whose existence of any resulting row is being tested, e.g. <c>select 1 from some_table</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if any row is returned by <paramref name="filterSql"/>; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterSql"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="filterSql"/> is empty or whitespace.</exception>
    public Task<bool> ExistsAsync(string filterSql, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterSql);

        return ExistsAsyncCore(filterSql, cancellationToken);
    }

    private async Task<bool> ExistsAsyncCore(string filterSql, CancellationToken cancellationToken)
    {
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        // resolving the suffix is serialised by its own lock and costs at most a handful of queries per
        // connection, so it stays outside the limiter rather than consuming permits meant for probes
        var suffix = await GetFromQuerySuffixAsync(cancellationToken);
        var query = suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;

        return await _probeLimiter.RunAsync(ct => DbConnection.ExecuteScalarAsync<bool>(query, ct), cancellationToken);
    }

    // Only one caller probes; the rest wait for its answer and then reuse the cached suffix.
    // A failed probe is not cached, so a cancelled lint run does not poison later ones.
    private async Task<string> GetFromQuerySuffixAsync(CancellationToken cancellationToken)
    {
        if (_fromQuerySuffix != null)
            return _fromQuerySuffix;

        await _suffixLock.WaitAsync(cancellationToken);
        try
        {
            return _fromQuerySuffix ??= await ProbeFromQuerySuffixAsync(cancellationToken);
        }
        finally
        {
            _suffixLock.Release();
        }
    }

    private async Task<string> ProbeFromQuerySuffixAsync(CancellationToken cancellationToken)
    {
        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryNoTable, cancellationToken);
            return string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Deliberately ignoring because we are testing functionality
        }

        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromSysDual, cancellationToken);
            return "SYS.DUAL";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Deliberately ignoring because we are testing functionality
        }

        _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromDual, cancellationToken);
        return "DUAL";
    }

    private const string TestQueryNoTable = "select 1 as dummy";
    private const string TestQueryFromDual = "select 1 as dummy from DUAL";
    private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

    private volatile string? _fromQuerySuffix;

    private readonly SemaphoreSlim _suffixLock = new(1, 1);

    private readonly ProbeConcurrencyLimiter _probeLimiter;

    private static readonly ConditionalWeakTable<ISchematicConnection, ExistsQueryExecutor> Executors = new();
}
