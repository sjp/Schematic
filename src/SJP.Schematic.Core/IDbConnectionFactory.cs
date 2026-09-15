using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database connection factory.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a database connection instance, but does not open the connection.
    /// </summary>
    /// <returns>An object representing a database connection</returns>
    DbConnection CreateConnection();

    /// <summary>
    /// Creates and opens a database connection.
    /// </summary>
    /// <returns>An object representing a database connection</returns>
    DbConnection OpenConnection();

    /// <summary>
    /// Creates and opens a database connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing an object representing a database connection when completed.</returns>
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether connections retrieved from this factory should be disposed.
    /// </summary>
    /// <value><see langword="true" /> if connection instances should be disposed; otherwise, <see langword="false" />.</value>
    bool DisposeConnection { get; }

    /// <summary>
    /// Gets a database command retry policy builder.
    /// </summary>
    /// <value>A retry policy builder.</value>
    /// <remarks>
    /// Queries run through this factory build their retry policy from this builder once, the first time the factory is
    /// used, and reuse that policy for every later query. A value that changes after that point has no effect.
    /// </remarks>
    PolicyBuilder RetryPolicy { get; }

    /// <summary>
    /// Gets the maximum number of queries that may run concurrently against this factory.
    /// </summary>
    /// <value>The maximum number of queries to run at once. Defaults to 16.</value>
    /// <remarks>
    /// Every query opened via this factory (including a streaming query, for as long as it is being enumerated)
    /// holds one slot for its duration. Implementations backed by a connection pool should return the pool's
    /// effective size so that fan-out over many objects cannot exhaust it and turn a slow response into an outright failure.
    /// </remarks>
    int MaxConcurrentQueries => 16;
}