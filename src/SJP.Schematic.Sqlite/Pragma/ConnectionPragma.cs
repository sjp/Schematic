using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    public class ConnectionPragma : ISqliteConnectionPragma
    {
        public ConnectionPragma(IDatabaseDialect dialect, IDbConnection connection)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected string PragmaPrefix { get; } = "PRAGMA ";

        public IEnumerable<ISqliteDatabasePragma> DatabasePragmas =>
            DatabaseList
                .OrderBy(d => d.seq)
                .Select(d => new DatabasePragma(Dialect, Connection, d.name))
                .ToList();

        public async Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync()
        {
            var databases = await DatabaseListAsync().ConfigureAwait(false);
            return databases
                .OrderBy(d => d.seq)
                .Select(d => new DatabasePragma(Dialect, Connection, d.name))
                .ToList();
        }

        public bool AutomaticIndex
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "automatic_index");
            set => Connection.Execute(PragmaPrefix + $"automatic_index = { Convert.ToInt32(value) }");
        }

        public Task<bool> AutomaticIndexAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "automatic_index");

        public Task AutomaticIndexAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"automatic_index = { Convert.ToInt32(enable) }");

        public TimeSpan BusyTimeout
        {
            get
            {
                var ms = Connection.ExecuteScalar<int>(PragmaPrefix + "busy_timeout");
                return TimeSpan.FromMilliseconds(ms);
            }
            set
            {
                var ms = value.TotalMilliseconds < 1 ? 0 : (int)value.TotalMilliseconds;
                Connection.Execute(PragmaPrefix + $"busy_timeout = { ms.ToString(CultureInfo.InvariantCulture) }");
            }
        }

        public async Task<TimeSpan> BusyTimeoutAsync()
        {
            var ms = await Connection.ExecuteScalarAsync<int>(PragmaPrefix + "busy_timeout").ConfigureAwait(false);
            return TimeSpan.FromMilliseconds(ms);
        }

        public Task BusyTimeoutAsync(TimeSpan timeout)
        {
            var ms = timeout.TotalMilliseconds < 1 ? 0 : (int)timeout.TotalMilliseconds;
            return Connection.ExecuteAsync(PragmaPrefix + $"busy_timeout = { ms.ToString(CultureInfo.InvariantCulture) }");
        }

        public void CaseSensitiveLike(bool enable) => Connection.Execute(PragmaPrefix + $"case_sensitive_like = { Convert.ToInt32(enable).ToString() }");

        public Task CaseSensitiveLikeAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"case_sensitive_like = { Convert.ToInt32(enable).ToString() }");

        public bool CellSizeCheck
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "cell_size_check");
            set => Connection.Execute(PragmaPrefix + $"cell_size_check = { Convert.ToInt32(value) }");
        }

        public Task<bool> CellSizeCheckAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "cell_size_check");

        public Task CellSizeCheckAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"cell_size_check = { Convert.ToInt32(enable) }");

        public bool CheckpointFullFsync
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "checkpoint_fullfsync");
            set => Connection.Execute(PragmaPrefix + $"checkpoint_fullfsync = { Convert.ToInt32(value) }");
        }

        public Task<bool> CheckpointFullFsyncAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "checkpoint_fullfsync");

        public Task CheckpointFullFsyncAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"checkpoint_fullfsync = { Convert.ToInt32(enable) }");

        public IEnumerable<pragma_collation_list> CollationList => Connection.Query<pragma_collation_list>(PragmaPrefix + "collation_list");

        public Task<IEnumerable<pragma_collation_list>> CollationListAsync() => Connection.QueryAsync<pragma_collation_list>(PragmaPrefix + "collation_list");

        public IEnumerable<string> CompileOptions => Connection.Query<string>(PragmaPrefix + "compile_options");

        public Task<IEnumerable<string>> CompileOptionsAsync() => Connection.QueryAsync<string>(PragmaPrefix + "compile_options");

        public int DataVersion => Connection.ExecuteScalar<int>(PragmaPrefix + "data_version");

        public Task<int> DataVersionAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "data_version");

        public IEnumerable<pragma_database_list> DatabaseList => Connection.Query<pragma_database_list>(PragmaPrefix + "database_list");

        public Task<IEnumerable<pragma_database_list>> DatabaseListAsync() => Connection.QueryAsync<pragma_database_list>(PragmaPrefix + "database_list");

        public bool DeferForeignKeys
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "defer_foreign_keys");
            set => Connection.Execute(PragmaPrefix + $"defer_foreign_keys = { Convert.ToInt32(value) }");
        }

        public Task<bool> DeferForeignKeysAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "defer_foreign_keys");

        public Task DeferForeignKeysAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"defer_foreign_keys = { Convert.ToInt32(enable) }");

        public Encoding Encoding
        {
            get
            {
                var encodingName = Connection.ExecuteScalar<string>(PragmaPrefix + "encoding");
                if (!_nameEncodingMapping.ContainsKey(encodingName))
                    throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

                return _nameEncodingMapping[encodingName];
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(value));

                var encodingName = _encodingNameMapping[value];
                Connection.Execute(PragmaPrefix + "encoding = '" + encodingName + "'");
            }
        }

        public async Task<Encoding> EncodingAsync()
        {
            var encodingName = await Connection.ExecuteScalarAsync<string>(PragmaPrefix + "encoding").ConfigureAwait(false);
            if (!_nameEncodingMapping.ContainsKey(encodingName))
                throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

            return _nameEncodingMapping[encodingName];
        }

        public Task EncodingAsync(Encoding encoding)
        {
            if (!encoding.IsValid())
                throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(encoding));

            var value = _encodingNameMapping[encoding];
            return Connection.ExecuteAsync(PragmaPrefix + "encoding = '" + value + "'");
        }

        public bool ForeignKeys
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "foreign_keys");
            set => Connection.Execute(PragmaPrefix + $"foreign_keys = { Convert.ToInt32(value) }");
        }

        public Task<bool> ForeignKeysAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "foreign_keys");

        public Task ForeignKeysAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"foreign_keys = { Convert.ToInt32(enable) }");

        public bool FullFsync
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "fullfsync");
            set => Connection.Execute(PragmaPrefix + $"fullfsync = { Convert.ToInt32(value) }");
        }

        public Task<bool> FullFsyncAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "fullfsync");

        public Task FullFsyncAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"fullfsync = { Convert.ToInt32(enable) }");

        public void IgnoreCheckConstraints(bool enable) => Connection.Execute(PragmaPrefix + $"ignore_check_constraints = { Convert.ToInt32(enable).ToString() }");

        public Task IgnoreCheckConstraintsAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"ignore_check_constraints = { Convert.ToInt32(enable).ToString() }");

        public bool LegacyFileFormat
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "legacy_file_format");
            set => Connection.Execute(PragmaPrefix + $"legacy_file_format = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> LegacyFileFormatAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "legacy_file_format");

        public Task LegacyFileFormatAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"legacy_file_format = { Convert.ToInt32(enable).ToString() }");

        public IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            return Connection.Query<string>(PragmaPrefix + $"optimize = { (int)features }");
        }

        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            return Connection.QueryAsync<string>(PragmaPrefix + $"optimize = { (int)features }");
        }

        public bool QueryOnly
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "query_only");
            set => Connection.Execute(PragmaPrefix + $"query_only = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> QueryOnlyAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "query_only");

        public Task QueryOnlyAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"query_only = { Convert.ToInt32(enable).ToString() }");

        public bool ReadUncommitted
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "read_uncommitted");
            set => Connection.Execute(PragmaPrefix + $"read_uncommitted = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> ReadUncommittedAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "read_uncommitted");

        public Task ReadUncommittedAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"read_uncommitted = { Convert.ToInt32(enable).ToString() }");

        public bool RecursiveTriggers
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "recursive_triggers");
            set => Connection.Execute(PragmaPrefix + $"recursive_triggers = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> RecursiveTriggersAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "recursive_triggers");

        public Task RecursiveTriggersAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"recursive_triggers = { Convert.ToInt32(enable).ToString() }");

        public bool ReverseUnorderedSelects
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "reverse_unordered_selects");
            set => Connection.Execute(PragmaPrefix + $"reverse_unordered_selects = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> ReverseUnorderedSelectsAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "reverse_unordered_selects");

        public Task ReverseUnorderedSelectsAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"reverse_unordered_selects = { Convert.ToInt32(enable).ToString() }");

        public void ShrinkMemory() => Connection.Execute(PragmaPrefix + "shrink_memory");

        public Task ShrinkMemoryAsync() => Connection.ExecuteAsync(PragmaPrefix + "shrink_memory");

        public long SoftHeapLimit
        {
            get => Connection.ExecuteScalar<long>(PragmaPrefix + "soft_heap_limit");
            set => Connection.Execute(PragmaPrefix + $"soft_heap_limit = { value.ToString(CultureInfo.InvariantCulture) }");
        }

        public Task<long> SoftHeapLimitAsync() => Connection.ExecuteScalarAsync<long>(PragmaPrefix + "soft_heap_limit");

        public Task SoftHeapLimitAsync(long heapLimit) => Connection.ExecuteAsync(PragmaPrefix + $"soft_heap_limit = { heapLimit.ToString(CultureInfo.InvariantCulture) }");

        public TemporaryStoreLocation TemporaryStore
        {
            get
            {
                var location = Connection.ExecuteScalar<int>(PragmaPrefix + "temp_store");
                if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
                    throw new InvalidOperationException($"Unable to map the value '{ location.ToString() }' to a member of { nameof(TemporaryStoreLocation) }.");

                return tempLocation;
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(value));

                Connection.Execute(PragmaPrefix + $"temp_store = { Enums.GetUnderlyingValue(value).ToString().ToUpperInvariant() }");
            }
        }

        public async Task<TemporaryStoreLocation> TemporaryStoreAsync()
        {
            var location = await Connection.ExecuteScalarAsync<int>(PragmaPrefix + "temp_store").ConfigureAwait(false);
            if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
                throw new InvalidOperationException($"Unable to map the value '{ location.ToString() }' to a member of { nameof(TemporaryStoreLocation) }.");

            return tempLocation;
        }

        public Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation)
        {
            if (!tempLocation.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(tempLocation));

            var value = tempLocation.ToString().ToUpperInvariant();
            return Connection.ExecuteAsync(PragmaPrefix + "temp_store = " + value);
        }

        public int Threads
        {
            get => Connection.ExecuteScalar<int>(PragmaPrefix + "threads");
            set => Connection.Execute(PragmaPrefix + $"threads = { value.ToString(CultureInfo.InvariantCulture) }");
        }

        public Task<int> ThreadsAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "threads");

        public Task ThreadsAsync(int maxThreads) => Connection.ExecuteAsync(PragmaPrefix + $"threads = { maxThreads.ToString(CultureInfo.InvariantCulture) }");

        public int WalAutoCheckpoint
        {
            get => Connection.ExecuteScalar<int>(PragmaPrefix + "wal_autocheckpoint");
            set => Connection.Execute(PragmaPrefix + $"wal_autocheckpoint = { value.ToString(CultureInfo.InvariantCulture) }");
        }

        public Task<int> WalAutoCheckpointAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "wal_autocheckpoint");

        public Task WalAutoCheckpointAsync(int maxPages) => Connection.ExecuteAsync(PragmaPrefix + $"wal_autocheckpoint = { maxPages.ToString() }");

        public bool WritableSchema
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "writable_schema");
            set => Connection.Execute(PragmaPrefix + $"writable_schema = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> WritableSchemaAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "writable_schema");

        public Task WritableSchemaAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"writable_schema = { Convert.ToInt32(enable).ToString() }");

        private readonly static IReadOnlyDictionary<Encoding, string> _encodingNameMapping = new Dictionary<Encoding, string>
        {
            [Encoding.Utf8] = "UTF-8",
            [Encoding.Utf16] = "UTF-16",
            [Encoding.Utf16le] = "UTF-16le",
            [Encoding.Utf16be] = "UTF-16be"
        };

        private readonly static IReadOnlyDictionary<string, Encoding> _nameEncodingMapping = new Dictionary<string, Encoding>
        {
            ["UTF-8"] = Encoding.Utf8,
            ["UTF-16"] = Encoding.Utf16,
            ["UTF-16le"] = Encoding.Utf16le,
            ["UTF-16be"] = Encoding.Utf16be
        };
    }
}
