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
        /// <summary>
        /// Creates an instance of <see cref="CachingConnection"/> with a default (in-memory) caching store.
        /// </summary>
        /// <param name="connection">A <see cref="DbConnection"/> whose results will be cached.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        public CachingConnection(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Cache = new CacheStore<int, DataTable>();
        }

        /// <summary>
        /// Creates an instance of <see cref="CachingConnection"/> with a given caching store.
        /// </summary>
        /// <param name="connection">A <see cref="DbConnection"/> whose results will be cached.</param>
        /// <param name="cacheStore">A caching store.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="cacheStore"/> is <c>null</c>.</exception>
        public CachingConnection(DbConnection connection, ICacheStore<int, DataTable> cacheStore)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Cache = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        }

        /// <summary>
        /// The cache in which the connection results will be stored.
        /// </summary>
        protected ICacheStore<int, DataTable> Cache { get; }

        /// <summary>
        /// The <see cref="DbConnection"/> instance whose results will be cached.
        /// </summary>
        protected DbConnection Connection { get; }

        /// <summary>
        /// Gets or sets the string used to open the connection.
        /// </summary>
        public override string ConnectionString
        {
            get => Connection.ConnectionString;
            set => Connection.ConnectionString = value;
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened
        /// </summary>
        public override string Database => Connection.Database;

        /// <summary>
        /// Gets the name of the database server to which to connect.
        /// </summary>
        public override string DataSource => Connection.DataSource;

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// </summary>
        public override string ServerVersion => Connection.ServerVersion;

        /// <summary>
        /// Gets an object that describes the state of the connection.
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
        /// Asynchronously opens a database connection with the settings specified by the <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task OpenAsync(CancellationToken cancellationToken) => Connection.OpenAsync(cancellationToken);

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => Connection.BeginTransaction(isolationLevel);

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. This command will perform caching on any query results retrieved.
        /// </summary>
        /// <returns>A <see cref="DbCommand"/> object whose query results will be cached.</returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = Connection.CreateCommand();
            return new CachingCommand(command, Cache);
        }
    }
}