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
        /// <summary>
        /// Creates an instance of a <see cref="DbConnection"/> from an <see cref="IDbConnection"/>.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public DbConnectionAdapter(IDbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// The existing connection that will be wrapped as a <see cref="DbConnection"/>.
        /// </summary>
        protected IDbConnection Connection { get; }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        public override string ConnectionString
        {
            get => Connection.ConnectionString;
            set => Connection.ConnectionString = value;
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened.
        /// </summary>
        public override string Database => Connection.Database;

        /// <summary>
        /// Gets the name of the database server to which to connect. Always an empty string.
        /// </summary>
        public override string DataSource => string.Empty;

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected. Always an empty string.
        /// </summary>
        public override string ServerVersion => string.Empty;

        /// <summary>
        /// Gets a value that describes the state of the connection.
        /// </summary>
        public override ConnectionState State => Connection.State;

        /// <summary>
        /// Changes the current database for an open connection.
        /// </summary>
        /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName) => Connection.ChangeDatabase(databaseName);

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// </summary>
        public override void Close() => Connection.Close();

        /// <summary>
        /// Opens a database connection with the settings specified by the <see cref="ConnectionString"/>.
        /// </summary>
        public override void Open() => Connection.Open();

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = Connection.BeginTransaction(isolationLevel);
            return transaction as DbTransaction ?? new DbTransactionAdapter(this, transaction);
        }

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="DbCommand"/> object.</returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = Connection.CreateCommand();
            return command as DbCommand ?? new DbCommandAdapter(this, command);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DbConnection"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">When true, releases managed resources, including the contained <see cref="IDbConnection"/> object.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed || !disposing)
                return;

            Connection.Dispose();
            _disposed = true;
        }

        private bool _disposed;
    }
}