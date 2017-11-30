using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// A command that will cache ExecuteScalar and ExecuteReader results from the database.
    /// </summary>
    public class CachingCommand : DbCommand
    {
        /// <summary>
        /// Creates an instance of <see cref="CachingCommand"/>.
        /// </summary>
        /// <param name="command">A <see cref="DbCommand"/> whose results will be cached.</param>
        /// <param name="cacheStore">A caching store.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> or <paramref name="cacheStore"/> is <c>null</c>.</exception>
        public CachingCommand(DbCommand command, ICacheStore<int, DataTable> cacheStore)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Cache = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        }

        /// <summary>
        /// The <see cref="DbCommand"/> instance that will be executed (and cached) against the database.
        /// </summary>
        protected DbCommand Command { get; }

        /// <summary>
        /// The cache in which the connection results will be stored.
        /// </summary>
        protected ICacheStore<int, DataTable> Cache { get; }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public override string CommandText
        {
            get => Command.CommandText;
            set => Command.CommandText = value;
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public override int CommandTimeout
        {
            get => Command.CommandTimeout;
            set => Command.CommandTimeout = value;
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="CommandText"/> property is interpreted.
        /// </summary>
        public override CommandType CommandType
        {
            get => Command.CommandType;
            set => Command.CommandType = value;
        }

        /// <summary>
        /// A value indicating whether the command object should be visible in a customized interface control.
        /// </summary>
        public override bool DesignTimeVisible
        {
            get => Command.DesignTimeVisible;
            set => Command.DesignTimeVisible = value;
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="DataRow"/> when used by the <c>Update()</c> method of a <see cref="DbDataAdapter"/>.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get => Command.UpdatedRowSource;
            set => Command.UpdatedRowSource = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DbConnection"/> used by this command.
        /// </summary>
        protected override DbConnection DbConnection
        {
            get => Command.Connection;
            set => Command.Connection = value;
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection => Command.Parameters;

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> used by this command.
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get => Command.Transaction;
            set => Command.Transaction = value;
        }

        /// <summary>
        /// Attempts to cancels the execution of a command.
        /// </summary>
        public override void Cancel() => Command.Cancel();

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public override int ExecuteNonQuery() => Command.ExecuteNonQuery();

        /// <summary>
        /// Executes a SQL statement asynchronously against a connection object.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Command.ExecuteNonQueryAsync(cancellationToken);

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. This command's results are only ever executed once.
        /// </summary>
        /// <returns>The first column of the first row in the result set.</returns>
        public override object ExecuteScalar()
        {
            var cacheKey = new DbCommandIdentity(this).Identity;
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                return cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
            }

            result = new DataTable();
            result.Columns.Add(new DataColumn());
            var reader = Command.ExecuteReader();
            object value;
            if (reader.Read())
            {
                value = reader.GetValue(0);
                var row = result.NewRow();
                row[0] = value;
                result.Rows.Add(row);
            }
            else
            {
                value = DBNull.Value;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        /// <summary>
        /// Asynchronously executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. This command's results are only ever executed once.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var cacheKey = new DbCommandIdentity(this).Identity;
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                return cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
            }

            result = new DataTable();
            result.Columns.Add(new DataColumn());
            var reader = await Command.ExecuteReaderAsync().ConfigureAwait(false);
            object value;
            if (reader.Read())
            {
                value = reader.GetValue(0);
                var row = result.NewRow();
                row[0] = value;
                result.Rows.Add(row);
            }
            else
            {
                value = DBNull.Value;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        /// <summary>
        /// Executes the command text against the connection. This command's results are only ever executed once.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
        /// <returns>A <see cref="DbDataReader"/> object.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var cacheKey = new DbCommandIdentity(this).Identity;
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = Command.ExecuteReader(behavior);
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        /// <summary>
        /// Asynchronously executes the command text against the connection. This command's results are only ever executed once.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var cacheKey = new DbCommandIdentity(this).Identity;
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = await Command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public override void Prepare() => Command.Prepare();

        /// <summary>
        /// Creates a new instance of a <see cref="DbParameter"/> object.
        /// </summary>
        /// <returns>A <see cref="DbParameter"/> object.</returns>
        protected override DbParameter CreateDbParameter() => Command.CreateParameter();

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DbCommand"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">When true, releases managed resources, including the contained <see cref="IDbCommand"/> object.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed || !disposing)
                return;

            Command.Dispose();
            _disposed = true;
        }

        private bool _disposed;
    }
}