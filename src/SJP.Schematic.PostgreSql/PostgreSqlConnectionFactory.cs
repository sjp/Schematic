using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlConnectionFactory : IDbConnectionFactory
    {
        public PostgreSqlConnectionFactory(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; }

        public IDbConnection CreateConnection() => new NpgsqlConnection(ConnectionString);

        public IDbConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public bool DisposeConnection { get; } = true;
    }
}
