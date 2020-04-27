using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        public SqlServerConnectionFactory(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; }

        public IDbConnection CreateConnection() => new SqlConnection(ConnectionString);

        public IDbConnection OpenConnection()
        {
            var connection = CreateConnection();
            connection.Open();
            return connection;
        }

        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public bool DisposeConnection { get; } = true;
    }
}
