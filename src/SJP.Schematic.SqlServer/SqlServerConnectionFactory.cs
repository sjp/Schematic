using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer
{
    /// <summary>
    /// A connection factory that provides SQL Server connections.
    /// </summary>
    /// <seealso cref="IDbConnectionFactory" />
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <c>null</c>, empty or whitespace.</exception>
        public SqlServerConnectionFactory(string connectionString)
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
        public IDbConnection CreateConnection() => new SqlConnection(ConnectionString);

        /// <summary>
        /// Creates and opens a database connection.
        /// </summary>
        /// <returns>An object representing a database connection.</returns>
        public IDbConnection OpenConnection()
        {
            var connection = CreateConnection();

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
            var connection = new SqlConnection(ConnectionString);

            if (connection.State != ConnectionState.Open)
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
