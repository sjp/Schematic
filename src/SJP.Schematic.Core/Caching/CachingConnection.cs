using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// A connection that will cache ExecuteScalar and ExecuteReader results from the database.
    /// </summary>
    public class CachingConnection : DbConnection
    {
        public CachingConnection(DbConnection connection)
        {
            InnerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Cache = new CacheStore<int, DataTable>();
        }

        public CachingConnection(DbConnection connection, ICacheStore<int, DataTable> cacheStore)
        {
            InnerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Cache = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        }

        protected ICacheStore<int, DataTable> Cache { get; }

        protected DbConnection InnerConnection { get; }

        public override string ConnectionString
        {
            get => InnerConnection.ConnectionString;
            set => InnerConnection.ConnectionString = value;
        }

        public override string Database => InnerConnection.Database;

        public override string DataSource => InnerConnection.DataSource;

        public override string ServerVersion => InnerConnection.ServerVersion;

        public override ConnectionState State => InnerConnection.State;

        public override void ChangeDatabase(string databaseName) => InnerConnection.ChangeDatabase(databaseName);

        public override void Close() => InnerConnection.Close();

        public override void Open() => InnerConnection.Open();

        public override Task OpenAsync(CancellationToken cancellationToken) => InnerConnection.OpenAsync(cancellationToken);

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => InnerConnection.BeginTransaction(isolationLevel);

        protected override DbCommand CreateDbCommand()
        {
            var command = InnerConnection.CreateCommand();
            return new CachingCommand(command, Cache);
        }
    }
}