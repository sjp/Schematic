using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        public async Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var databases = await DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            return databases
                .OrderBy(d => d.seq)
                .Select(d => new DatabasePragma(Dialect, Connection, d.name))
                .ToList();
        }

        public bool AutomaticIndex
        {
            get => Connection.ExecuteScalar<bool>(AutomaticIndexReadQuery);
            set => Connection.Execute(AutomaticIndexSetQuery(value));
        }

        public Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(AutomaticIndexReadQuery);

        public Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(AutomaticIndexSetQuery(enable));

        protected virtual string AutomaticIndexReadQuery => PragmaPrefix + "automatic_index";

        protected virtual string AutomaticIndexSetQuery(bool enable) => PragmaPrefix + "automatic_index = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public TimeSpan BusyTimeout
        {
            get
            {
                var ms = Connection.ExecuteScalar<int>(BusyTimeoutReadQuery);
                return TimeSpan.FromMilliseconds(ms);
            }
            set => Connection.Execute(BusyTimeoutSetQuery(value));
        }

        public async Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var ms = await Connection.ExecuteScalarAsync<int>(BusyTimeoutReadQuery).ConfigureAwait(false);
            return TimeSpan.FromMilliseconds(ms);
        }

        public Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(BusyTimeoutSetQuery(timeout));

        protected virtual string BusyTimeoutReadQuery => PragmaPrefix + "busy_timeout";

        protected virtual string BusyTimeoutSetQuery(TimeSpan timeout)
        {
            var ms = timeout.TotalMilliseconds < 1 ? 0 : (int)timeout.TotalMilliseconds;
            return PragmaPrefix + "busy_timeout = " + ms.ToString(CultureInfo.InvariantCulture);
        }

        public void CaseSensitiveLike(bool enable) => Connection.Execute(CaseSensitiveLikeSetQuery(enable));

        public Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CaseSensitiveLikeSetQuery(enable));

        protected virtual string CaseSensitiveLikeSetQuery(bool enable) => PragmaPrefix + "case_sensitive_like = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool CellSizeCheck
        {
            get => Connection.ExecuteScalar<bool>(CellSizeCheckReadQuery);
            set => Connection.Execute(CellSizeCheckSetQuery(value));
        }

        public Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(CellSizeCheckReadQuery);

        public Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CellSizeCheckSetQuery(enable));

        protected virtual string CellSizeCheckReadQuery => PragmaPrefix + "cell_size_check";

        protected virtual string CellSizeCheckSetQuery(bool enable) => PragmaPrefix + "cell_size_check = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool CheckpointFullFsync
        {
            get => Connection.ExecuteScalar<bool>(CheckpointFullFsyncReadQuery);
            set => Connection.Execute(CheckpointFullFsyncSetQuery(value));
        }

        public Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(CheckpointFullFsyncReadQuery);

        public Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CheckpointFullFsyncSetQuery(enable));

        protected virtual string CheckpointFullFsyncReadQuery => PragmaPrefix + "checkpoint_fullfsync";

        protected virtual string CheckpointFullFsyncSetQuery(bool enable) => PragmaPrefix + "checkpoint_fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public IEnumerable<pragma_collation_list> CollationList => Connection.Query<pragma_collation_list>(CollationListReadQuery);

        public Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_collation_list>(CollationListReadQuery);

        protected virtual string CollationListReadQuery => PragmaPrefix + "collation_list";

        public IEnumerable<string> CompileOptions => Connection.Query<string>(CompileOptionsReadQuery);

        public Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<string>(CompileOptionsReadQuery);

        protected virtual string CompileOptionsReadQuery => PragmaPrefix + "compile_options";

        public int DataVersion => Connection.ExecuteScalar<int>(DataVersionReadQuery);

        public Task<int> DataVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(DataVersionReadQuery);

        protected virtual string DataVersionReadQuery => PragmaPrefix + "data_version";

        public IEnumerable<pragma_database_list> DatabaseList => Connection.Query<pragma_database_list>(DatabaseListReadQuery);

        public Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_database_list>(DatabaseListReadQuery);

        protected virtual string DatabaseListReadQuery => PragmaPrefix + "database_list";

        public bool DeferForeignKeys
        {
            get => Connection.ExecuteScalar<bool>(DeferForeignKeysReadQuery);
            set => Connection.Execute(DeferForeignKeysSetQuery(value));
        }

        public Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(DeferForeignKeysReadQuery);

        public Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(DeferForeignKeysSetQuery(enable));

        protected virtual string DeferForeignKeysReadQuery => PragmaPrefix + "defer_foreign_keys";

        protected virtual string DeferForeignKeysSetQuery(bool enable) => PragmaPrefix + "defer_foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Encoding Encoding
        {
            get
            {
                var encodingName = Connection.ExecuteScalar<string>(EncodingReadQuery);
                if (!_nameEncodingMapping.ContainsKey(encodingName))
                    throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

                return _nameEncodingMapping[encodingName];
            }
            set => Connection.Execute(EncodingSetQuery(value));
        }

        public async Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var encodingName = await Connection.ExecuteScalarAsync<string>(EncodingReadQuery).ConfigureAwait(false);
            if (!_nameEncodingMapping.ContainsKey(encodingName))
                throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

            return _nameEncodingMapping[encodingName];
        }

        public Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(EncodingSetQuery(encoding));

        protected virtual string EncodingReadQuery => PragmaPrefix + "encoding";

        protected virtual string EncodingSetQuery(Encoding encoding)
        {
            if (!encoding.IsValid())
                throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(encoding));

            var value = _encodingNameMapping[encoding];
            return PragmaPrefix + "encoding = '" + value + "'";
        }

        public bool ForeignKeys
        {
            get => Connection.ExecuteScalar<bool>(ForeignKeysReadQuery);
            set => Connection.Execute(ForeignKeysSetQuery(value));
        }

        public Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(ForeignKeysReadQuery);

        public Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ForeignKeysSetQuery(enable));

        protected virtual string ForeignKeysReadQuery => PragmaPrefix + "foreign_keys";

        protected virtual string ForeignKeysSetQuery(bool enable) => PragmaPrefix + "foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool FullFsync
        {
            get => Connection.ExecuteScalar<bool>(FullFsyncReadQuery);
            set => Connection.Execute(FullFsyncSetQuery(value));
        }

        public Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(FullFsyncReadQuery);

        public Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(FullFsyncSetQuery(enable));

        protected virtual string FullFsyncReadQuery => PragmaPrefix + "fullfsync";

        protected virtual string FullFsyncSetQuery(bool enable) => PragmaPrefix + "fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public void IgnoreCheckConstraints(bool enable) => Connection.Execute(IgnoreCheckConstraintsSetQuery(enable));

        public Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(IgnoreCheckConstraintsSetQuery(enable));

        protected virtual string IgnoreCheckConstraintsSetQuery(bool enable) => PragmaPrefix + "ignore_check_constraints = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool LegacyFileFormat
        {
            get => Connection.ExecuteScalar<bool>(LegacyFileFormatReadQuery);
            set => Connection.Execute(LegacyFileFormatSetQuery(value));
        }

        public Task<bool> LegacyFileFormatAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(LegacyFileFormatReadQuery);

        public Task LegacyFileFormatAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(LegacyFileFormatSetQuery(enable));

        protected virtual string LegacyFileFormatReadQuery => PragmaPrefix + "legacy_file_format";

        protected virtual string LegacyFileFormatSetQuery(bool enable) => PragmaPrefix + "legacy_file_format = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze) => Connection.Query<string>(OptimizeSetQuery(features));

        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<string>(OptimizeSetQuery(features));

        protected virtual string OptimizeSetQuery(OptimizeFeatures features)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            var value = (int)features;
            return PragmaPrefix + "optimize = " + value;
        }

        public bool QueryOnly
        {
            get => Connection.ExecuteScalar<bool>(QueryOnlyReadQuery);
            set => Connection.Execute(QueryOnlySetQuery(value));
        }

        public Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(QueryOnlyReadQuery);

        public Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(QueryOnlySetQuery(enable));

        protected virtual string QueryOnlyReadQuery => PragmaPrefix + "query_only";

        protected virtual string QueryOnlySetQuery(bool enable) => PragmaPrefix + "query_only = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool ReadUncommitted
        {
            get => Connection.ExecuteScalar<bool>(ReadUncommittedReadQuery);
            set => Connection.Execute(ReadUncommittedSetQuery(value));
        }

        public Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(ReadUncommittedReadQuery);

        public Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ReadUncommittedSetQuery(enable));

        protected virtual string ReadUncommittedReadQuery => PragmaPrefix + "read_uncommitted";

        protected virtual string ReadUncommittedSetQuery(bool enable) => PragmaPrefix + "read_uncommitted = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool RecursiveTriggers
        {
            get => Connection.ExecuteScalar<bool>(RecursiveTriggersReadQuery);
            set => Connection.Execute(RecursiveTriggersSetQuery(value));
        }

        public Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(RecursiveTriggersReadQuery);

        public Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(RecursiveTriggersSetQuery(enable));

        protected virtual string RecursiveTriggersReadQuery => PragmaPrefix + "recursive_triggers";

        protected virtual string RecursiveTriggersSetQuery(bool enable) => PragmaPrefix + "recursive_triggers = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public bool ReverseUnorderedSelects
        {
            get => Connection.ExecuteScalar<bool>(ReverseUnorderedSelectsReadQuery);
            set => Connection.Execute(ReverseUnorderedSelectsSetQuery(value));
        }

        public Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(ReverseUnorderedSelectsReadQuery);

        public Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ReverseUnorderedSelectsSetQuery(enable));

        protected virtual string ReverseUnorderedSelectsReadQuery => PragmaPrefix + "reverse_unordered_selects";

        protected virtual string ReverseUnorderedSelectsSetQuery(bool enable) => PragmaPrefix + "reverse_unordered_selects = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public void ShrinkMemory() => Connection.Execute(ShrinkMemoryQuery);

        public Task ShrinkMemoryAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ShrinkMemoryQuery);

        protected virtual string ShrinkMemoryQuery => PragmaPrefix + "shrink_memory";

        public long SoftHeapLimit
        {
            get => Connection.ExecuteScalar<long>(SoftHeapLimitReadQuery);
            set => Connection.Execute(SoftHeapLimitSetQuery(value));
        }

        public Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<long>(SoftHeapLimitReadQuery);

        public Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SoftHeapLimitSetQuery(heapLimit));

        protected virtual string SoftHeapLimitReadQuery => PragmaPrefix + "soft_heap_limit";

        protected virtual string SoftHeapLimitSetQuery(long heapLimit) => PragmaPrefix + "soft_heap_limit = " + heapLimit.ToString(CultureInfo.InvariantCulture);

        public TemporaryStoreLocation TemporaryStore
        {
            get
            {
                var location = Connection.ExecuteScalar<int>(TemporaryStoreReadQuery);
                if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
                    throw new InvalidOperationException($"Unable to map the value '{ location.ToString() }' to a member of { nameof(TemporaryStoreLocation) }.");

                return tempLocation;
            }
            set => Connection.Execute(TemporaryStoreSetQuery(value));
        }

        public async Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var location = await Connection.ExecuteScalarAsync<int>(TemporaryStoreReadQuery).ConfigureAwait(false);
            if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
                throw new InvalidOperationException($"Unable to map the value '{ location.ToString() }' to a member of { nameof(TemporaryStoreLocation) }.");

            return tempLocation;
        }

        public Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(TemporaryStoreSetQuery(tempLocation));

        protected virtual string TemporaryStoreReadQuery => PragmaPrefix + "temp_store";

        protected virtual string TemporaryStoreSetQuery(TemporaryStoreLocation tempLocation)
        {
            if (!tempLocation.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(tempLocation));

            var value = tempLocation.ToString().ToUpperInvariant();
            return PragmaPrefix + "temp_store = " + value;
        }

        public int Threads
        {
            get => Connection.ExecuteScalar<int>(ThreadsReadQuery);
            set => Connection.Execute(ThreadsSetQuery(value));
        }

        public Task<int> ThreadsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(ThreadsReadQuery);

        public Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ThreadsSetQuery(maxThreads));

        protected virtual string ThreadsReadQuery => PragmaPrefix + "threads";

        protected virtual string ThreadsSetQuery(int maxThreads) => PragmaPrefix + "threads = " + maxThreads.ToString(CultureInfo.InvariantCulture);

        public int WalAutoCheckpoint
        {
            get => Connection.ExecuteScalar<int>(WalAutoCheckpointReadQuery);
            set => Connection.Execute(WalAutoCheckpointSetQuery(value));
        }

        public Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(WalAutoCheckpointReadQuery);

        public Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(WalAutoCheckpointSetQuery(maxPages));

        protected virtual string WalAutoCheckpointReadQuery => PragmaPrefix + "wal_autocheckpoint";

        protected virtual string WalAutoCheckpointSetQuery(int maxPages) => PragmaPrefix + "wal_autocheckpoint = " + maxPages.ToString(CultureInfo.InvariantCulture);

        public bool WritableSchema
        {
            get => Connection.ExecuteScalar<bool>(WritableSchemaReadQuery);
            set => Connection.Execute(WritableSchemaSetQuery(value));
        }

        public Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(WritableSchemaReadQuery);

        public Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(WritableSchemaSetQuery(enable));

        protected virtual string WritableSchemaReadQuery => PragmaPrefix + "writable_schema";

        protected virtual string WritableSchemaSetQuery(bool enable) => PragmaPrefix + "writable_schema = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

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
