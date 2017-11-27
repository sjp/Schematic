using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbConnection"/> from an <see cref="IDbConnection"/>. This is used to provide an async wrapper over an <see cref="IDbConnection"/>, enabling Dapper support for any connection.
    /// </summary>
    public class DbConnectionAdapter : DbConnection
    {
        public DbConnectionAdapter(IDbConnection connection)
        {
            InnerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDbConnection InnerConnection { get; }

        public override string ConnectionString
        {
            get => InnerConnection.ConnectionString;
            set => InnerConnection.ConnectionString = value;
        }

        public override string Database => InnerConnection.Database;

        public override string DataSource => string.Empty;

        public override string ServerVersion => string.Empty;

        public override ConnectionState State => InnerConnection.State;

        public override void ChangeDatabase(string databaseName) => InnerConnection.ChangeDatabase(databaseName);

        public override void Close() => InnerConnection.Close();

        public override void Open() => InnerConnection.Open();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = InnerConnection.BeginTransaction(isolationLevel);
            return transaction as DbTransaction ?? new DbTransactionAdapter(this, transaction);
        }

        protected override DbCommand CreateDbCommand()
        {
            var command = InnerConnection.CreateCommand();
            return command as DbCommand ?? new DbCommandAdapter(this, command);
        }
    }
}