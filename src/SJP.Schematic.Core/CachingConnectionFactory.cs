using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public class CachingConnectionFactory : IDbConnectionFactory
    {
        private readonly object _lock = new object();
        private readonly IDbConnectionFactory _connectionFactory;
        private IDbConnection? _connection;

        public CachingConnectionFactory(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public IDbConnection CreateConnection()
        {
            if (_connection != null)
                return _connection;

            lock (_lock)
            {
                if (_connection != null)
                    return _connection;

                _connection = _connectionFactory.CreateConnection();
                return _connection;
            }
        }

        public IDbConnection OpenConnection()
        {
            var connection = CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = CreateConnection();

            if (connection.State != ConnectionState.Open)
            {
                if (connection is DbConnection dbConnection)
                    await dbConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                else
                    connection.Open();
            }

            return connection;
        }

        public bool DisposeConnection { get; }
    }
}
