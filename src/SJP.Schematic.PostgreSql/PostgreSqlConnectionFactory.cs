﻿using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Polly;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A connection factory that provides PostgreSQL connections.
    /// </summary>
    /// <seealso cref="IDbConnectionFactory" />
    public class PostgreSqlConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <c>null</c>, empty or whitespace.</exception>
        public PostgreSqlConnectionFactory(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        protected string ConnectionString { get; }

        /// <summary>
        /// Creates a database connection instance, but does not open the connection.
        /// </summary>
        /// <returns>An object representing a database connection.</returns>
        public IDbConnection CreateConnection() => new NpgsqlConnection(ConnectionString);

        /// <summary>
        /// Creates and opens a database connection.
        /// </summary>
        /// <returns>An object representing a database connection.</returns>
        public IDbConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        /// <summary>
        /// Creates and opens a database connection asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing an object representing a database connection when completed.</returns>
        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(ConnectionString);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            return connection;
        }

        /// <summary>
        /// Determines whether connections retrieved from this factory should be disposed.
        /// </summary>
        /// <value>Always <c>true</c>.</value>
        public bool DisposeConnection { get; } = true;

        /// <summary>
        /// Gets a database command retry policy builder.
        /// </summary>
        /// <value>A retry policy builder.</value>
        public PolicyBuilder RetryPolicy => Policy
            .Handle<PostgresException>(IsTransientError)
            .Or<TimeoutException>();

        private static bool IsTransientError(PostgresException pgex)
        {
            switch (pgex.SqlState)
            {
                case "40001": // serialzation_failure
                case "53000": // insufficient_resources
                case "53100": // disk_full
                case "53200": // out_of_memory
                case "53300": // too_many_connections
                case "53400": // configuration_limit_exceeded
                case "57P03": // cannot_connect_now
                case "58000": // system_error
                case "58030": // io_error
                case "55P03": // lock_not_available
                case "55006": // object_in_use
                case "55000": // object_not_in_prerequisite_state
                case "08000": // connection_exception
                case "08003": // connection_does_not_exist
                case "08006": // connection_failure
                case "08001": // sqlclient_unable_to_establish_sqlconnection
                case "08004": // sqlserver_rejected_establishment_of_sqlconnection
                case "08007": // transaction_resolution_unknown
                    return true;
                default:
                    return false;
            }
        }
    }
}
