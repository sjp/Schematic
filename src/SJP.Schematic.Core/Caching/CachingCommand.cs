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
        public CachingCommand(DbCommand command, ICacheStore<int, DataTable> cacheStore)
        {
            InnerCommand = command ?? throw new ArgumentNullException(nameof(command));
            Cache = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        }

        protected ICacheStore<int, DataTable> Cache { get; }

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
            var cacheKey = new CachingCommandIdentity(this).Identity;
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
                value = DBNull.Value;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var cacheKey = new CachingCommandIdentity(this).Identity;
            if (Cache.TryGetValue(cacheKey, out var result))
            {
                var cachedReader = result.CreateDataReader();
                return cachedReader.Read()
                    ? cachedReader.GetValue(0)
                    : null;
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
                value = DBNull.Value;
            }

            Cache.TryAdd(cacheKey, result);
            return value;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var cacheKey = new CachingCommandIdentity(this).Identity;
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
            var cacheKey = new CachingCommandIdentity(this).Identity;
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
}