using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A connection factory decorator that counts how many connections were opened. Each query executed
/// via <c>ConnectionExtensions</c> (e.g. <c>ExecuteScalarAsync</c>) opens and disposes exactly one
/// connection, so this doubles as a round-trip counter for tests asserting on query-batching behaviour.
/// </summary>
public sealed class CountingDbConnectionFactory : IDbConnectionFactory
{
    public CountingDbConnectionFactory(IDbConnectionFactory innerFactory)
    {
        InnerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
    }

    private IDbConnectionFactory InnerFactory { get; }

    public int QueryCount => _queryCount;

    public DbConnection CreateConnection() => InnerFactory.CreateConnection();

    public DbConnection OpenConnection()
    {
        Interlocked.Increment(ref _queryCount);
        return InnerFactory.OpenConnection();
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _queryCount);
        return await InnerFactory.OpenConnectionAsync(cancellationToken);
    }

    public bool DisposeConnection => InnerFactory.DisposeConnection;

    public PolicyBuilder RetryPolicy => InnerFactory.RetryPolicy;

    private int _queryCount;
}
