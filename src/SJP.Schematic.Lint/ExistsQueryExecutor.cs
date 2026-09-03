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
    /// shared so that the rules running against one connection also share its probe concurrency limit.
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

    private Task<bool> ExistsAsyncCore(string filterSql, CancellationToken cancellationToken)
    {
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        // engines such as Oracle reject a select without a from clause, and say so through their dialect
        var query = Connection.Dialect.Capabilities.FromLessSelectSuffix
            .Match(suffix => sql + " from " + suffix, sql);

        return _probeLimiter.RunAsync(ct => DbConnection.ExecuteScalarAsync<bool>(query, ct), cancellationToken);
    }

    private readonly ProbeConcurrencyLimiter _probeLimiter;

    private static readonly ConditionalWeakTable<ISchematicConnection, ExistsQueryExecutor> Executors = new();
}
