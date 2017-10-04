using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities
{
    /*
    // waiting on https://github.com/dotnet/corefx/issues/19748


    public static class CachingExtensions
    {
        public static IDbConnection AsCachedConnection(this IDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return null;
        }
    }

    // https://github.com/MiniProfiler/dotnet/blob/master/src/MiniProfiler.Shared/Data/ProfiledDbConnection.cs
    // https://github.com/MiniProfiler/dotnet/blob/master/src/MiniProfiler.Shared/Data/ProfiledDbCommand.cs

    public class CachingConnection : DbConnection
    {
        public CachingConnection(IDbConnection connection)
            : this(connection as DbConnection)
        {
        }

        public CachingConnection(DbConnection connection)
        {
            InnerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected ConcurrentDictionary<int, DataTable> Cache { get; } = new ConcurrentDictionary<int, DataTable>();

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
            return new CachingCommand(Cache, command);
        }
    }

    public class CachingCommand : DbCommand
    {
        public CachingCommand(ConcurrentDictionary<int, DataTable> cache, DbCommand command)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            InnerCommand = command ?? throw new ArgumentNullException(nameof(command));
        }

        protected ConcurrentDictionary<int, DataTable> Cache { get; }

        protected DbCommand InnerCommand { get; }

        public override string CommandText
        {
            get => InnerCommand.CommandText;
            set => InnerCommand.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => InnerCommand.CommandTimeout;
            set => InnerCommand.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => InnerCommand.CommandType;
            set => InnerCommand.CommandType = value;
        }

        public override bool DesignTimeVisible
        {
            get => InnerCommand.DesignTimeVisible;
            set => InnerCommand.DesignTimeVisible = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => InnerCommand.UpdatedRowSource;
            set => InnerCommand.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => InnerCommand.Connection;
            set => InnerCommand.Connection = value;
        }

        protected override DbParameterCollection DbParameterCollection => InnerCommand.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => InnerCommand.Transaction;
            set => InnerCommand.Transaction = value;
        }

        public override void Cancel() => InnerCommand.Cancel();

        public override int ExecuteNonQuery() => InnerCommand.ExecuteNonQuery();

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => InnerCommand.ExecuteNonQueryAsync(cancellationToken);

        public override object ExecuteScalar()
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                return cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
            }

            result = new DataTable();
            result.Columns.Add(new DataColumn());
            var reader = InnerCommand.ExecuteReader();
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
                value = null;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                var returnValue = cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
                return Task.FromResult(returnValue);
            }

            result = new DataTable();
            result.Columns.Add(new DataColumn());
            var reader = await InnerCommand.ExecuteReaderAsync().ConfigureAwait(false);
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
                value = null;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = InnerCommand.ExecuteReader(behavior);
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = await InnerCommand.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        public override void Prepare() => InnerCommand.Prepare();

        protected override DbParameter CreateDbParameter() => InnerCommand.CreateParameter();

    }

    public class CachingConnectionOriginal : IDbConnection
    {
        public CachingConnectionOriginal(IDbConnection connection)
        {
            InnerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected ConcurrentDictionary<int, DataTable> Cache { get; } = new ConcurrentDictionary<int, DataTable>();

        protected IDbConnection InnerConnection { get; }

        public void Clear() => Cache.Clear();

        public string ConnectionString
        {
            get => InnerConnection.ConnectionString;
            set => InnerConnection.ConnectionString = value;
        }

        public int ConnectionTimeout => InnerConnection.ConnectionTimeout;

        public string Database => InnerConnection.Database;

        public ConnectionState State => InnerConnection.State;

        public IDbTransaction BeginTransaction() => InnerConnection.BeginTransaction();

        public IDbTransaction BeginTransaction(IsolationLevel il) => InnerConnection.BeginTransaction(il);

        public void ChangeDatabase(string databaseName) => InnerConnection.ChangeDatabase(databaseName);

        public void Close() => InnerConnection.Close();

        public IDbCommand CreateCommand()
        {
            var command = InnerConnection.CreateCommand();
            return new CachingCommandOriginal(Cache, command);
        }

        public void Dispose() => InnerConnection.Dispose();

        public void Open() => InnerConnection.Open();
    }

    public class CachingCommandOriginal : IDbCommand
    {
        public CachingCommandOriginal(ConcurrentDictionary<int, DataTable> cache, IDbCommand command)
        {
            Cache = cache ?? throw new ArgumentNullException(nameof(command));
            InnerCommand = command ?? throw new ArgumentNullException(nameof(command));
        }

        protected ConcurrentDictionary<int, DataTable> Cache { get; }

        protected IDbCommand InnerCommand { get; }

        public string CommandText
        {
            get => InnerCommand.CommandText;
            set => InnerCommand.CommandText = value;
        }

        public int CommandTimeout
        {
            get => InnerCommand.CommandTimeout;
            set => InnerCommand.CommandTimeout = value;
        }

        public CommandType CommandType
        {
            get => InnerCommand.CommandType;
            set => InnerCommand.CommandType = value;
        }

        public IDbConnection Connection
        {
            get => InnerCommand.Connection;
            set => InnerCommand.Connection = value;
        }

        public IDataParameterCollection Parameters => InnerCommand.Parameters;

        public IDbTransaction Transaction
        {
            get => InnerCommand.Transaction;
            set => InnerCommand.Transaction = value;
        }

        public UpdateRowSource UpdatedRowSource
        {
            get => InnerCommand.UpdatedRowSource;
            set => InnerCommand.UpdatedRowSource = value;
        }

        public void Cancel() => InnerCommand.Cancel();

        public IDbDataParameter CreateParameter() => InnerCommand.CreateParameter();

        public void Dispose() => InnerCommand.Dispose();

        public int ExecuteNonQuery() => InnerCommand.ExecuteNonQuery();

        public IDataReader ExecuteReader()
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = InnerCommand.ExecuteReader();
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
                return result.CreateDataReader();

            result = new DataTable();
            var reader = InnerCommand.ExecuteReader(behavior);
            result.Load(reader);

            Cache.TryAdd(cacheKey, result);
            return result.CreateDataReader();
        }

        public object ExecuteScalar()
        {
            var cacheKey = new CommandIdentity(this).GetHashCode();
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                return cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
            }

            result = new DataTable();
            result.Columns.Add(new DataColumn());
            var reader = InnerCommand.ExecuteReader();
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
                value = null;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        public void Prepare() => InnerCommand.Prepare();
    }

    public class HashableDataParameter
    {
        public HashableDataParameter(IDataParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + (parameter.ParameterName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + parameter.Direction.GetHashCode();
                hashCode = (hashCode * 23) + (parameter.Value?.GetHashCode() ?? 0);
            }

            HashCode = hashCode;
        }

        public override int GetHashCode() => HashCode;

        private int HashCode { get; }
    }

    public class HashableDataParameterCollection
    {
        public HashableDataParameterCollection(IDataParameterCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var hashCode = 17;

            unchecked
            {
                var parameters = collection.OfType<IDataParameter>();
                foreach (var parameter in parameters)
                {
                    var hashParameter = new HashableDataParameter(parameter);
                    hashCode = (hashCode * 23) + hashParameter.GetHashCode();
                }
            }

            HashCode = hashCode;
        }

        public override int GetHashCode() => HashCode;

        private int HashCode { get; }
    }

    public class CommandIdentity
    {
        public CommandIdentity(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var hashCode = 17;

            unchecked
            {
                hashCode = (hashCode * 23) + command.CommandText.GetHashCode();
                hashCode = (hashCode * 23) + command.CommandType.GetHashCode();

                var parameters = new HashableDataParameterCollection(command.Parameters);
                hashCode = (hashCode * 23) + parameters.GetHashCode();
            }

            HashCode = hashCode;
        }

        public override int GetHashCode() => HashCode;

        private int HashCode { get; }
    }
    */
}