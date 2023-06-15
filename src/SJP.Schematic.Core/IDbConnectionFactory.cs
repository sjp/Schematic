﻿using System.Data.Common;
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
    /// <value><c>true</c> if connection instances should be disposed; otherwise, <c>false</c>.</value>
    bool DisposeConnection { get; }

    /// <summary>
    /// Gets a database command retry policy builder.
    /// </summary>
    /// <value>A retry policy builder.</value>
    PolicyBuilder RetryPolicy { get; }
}