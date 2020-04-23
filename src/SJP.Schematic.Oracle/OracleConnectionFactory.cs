using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        public OracleConnectionFactory(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; }

        public IDbConnection CreateConnection() => new OracleConnection(ConnectionString);

        public IDbConnection OpenConnection()
        {
            var connection = new OracleConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new OracleConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public bool DisposeConnection { get; } = true;
    }
}
