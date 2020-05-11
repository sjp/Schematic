using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A connection factory that provides Oracle connections.
    /// </summary>
    /// <seealso cref="IDbConnectionFactory" />
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <c>null</c>, empty or whitespace.</exception>
        public OracleConnectionFactory(string connectionString)
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
        public IDbConnection CreateConnection() => new OracleConnection(ConnectionString);

        /// <summary>
        /// Creates and opens a database connection.
        /// </summary>
        /// <returns>An object representing a database connection.</returns>
        public IDbConnection OpenConnection()
        {
            var connection = new OracleConnection(ConnectionString);
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
            var connection = new OracleConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// Determines whether connections retrieved from this factory should be disposed.
        /// </summary>
        /// <value>Always <c>true</c>.</value>
        public bool DisposeConnection { get; } = true;
    }
}
