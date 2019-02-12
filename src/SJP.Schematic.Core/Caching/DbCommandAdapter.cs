using System;
using System.Data;
using System.Data.Common;

namespace SJP.Schematic.Core.Caching
{
    /// <summary>
    /// Creates a <see cref="DbCommand"/> from an <see cref="IDbCommand"/>. Only used for implementing <see cref="DbConnectionAdapter"/>.
    /// </summary>
    public class DbCommandAdapter : DbCommand
    {
        /// <summary>
        /// Creates an instance of <see cref="DbCommandAdapter"/> to wrap an <see cref="IDbCommand"/> as a <see cref="DbCommand"/>.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="command">A database command.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="command"/> is <c>null</c>.</exception>
        public DbCommandAdapter(DbConnection connection, IDbCommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));

            DbParameterCollection = new DbParameterCollectionAdapter(command.Parameters);
        }

        /// <summary>
        /// Gets or sets the <see cref="DbConnection"/> used by this command.
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                var connection = Command.Connection;
                if (connection == null)
                    return null;

                return connection as DbConnection ?? new DbConnectionAdapter(connection);
            }
            set => Command.Connection = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> used by this command.
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                var transaction = Command.Transaction;
                if (transaction == null)
                    return null;

                return transaction as DbTransaction ?? new DbTransactionAdapter(DbConnection, transaction);
            }
            set => Command.Transaction = value;
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection { get; }

        /// <summary>
        /// The <see cref="IDbCommand"/> instance that is being wrapped as a <see cref="DbCommand"/>.
        /// </summary>
        protected IDbCommand Command { get; }

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
        /// A value indicating whether the command object should be visible in a customized interface control. Always false.
        /// </summary>
        public override bool DesignTimeVisible
        {
            get => false;
            set => _ = value;
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
        /// Attempts to cancels the execution of a <see cref="DbCommand"/>.
        /// </summary>
        public override void Cancel() => Command.Cancel();

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public override int ExecuteNonQuery() => Command.ExecuteNonQuery();

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set.</returns>
        public override object ExecuteScalar() => Command.ExecuteScalar();

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public override void Prepare() => Command.Prepare();

        /// <summary>
        /// Creates a new instance of a DbParameter object.
        /// </summary>
        /// <returns>A <see cref="DbParameter"/> object.</returns>
        protected override DbParameter CreateDbParameter()
        {
            var parameter = Command.CreateParameter();
            return parameter as DbParameter ?? new DbParameterAdapter(parameter);
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="CommandBehavior"/>.</param>
        /// <returns>A task representing the operation.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var reader = Command.ExecuteReader(behavior);
            return reader as DbDataReader ?? new DbDataReaderAdapter(reader, behavior);
        }

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