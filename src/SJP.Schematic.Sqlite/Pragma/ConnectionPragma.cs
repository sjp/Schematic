using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
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

        public async Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default)
        {
            var databases = await DatabaseListAsync(cancellationToken).ConfigureAwait(false);
            return databases
                .OrderBy(d => d.seq)
                .Select(d => new DatabasePragma(Dialect, Connection, d.name))
                .ToList();
        }

        public Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(AutomaticIndexReadQuery, cancellationToken);

        public Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(AutomaticIndexSetQuery(enable), cancellationToken);

        protected virtual string AutomaticIndexReadQuery => PragmaPrefix + "automatic_index";

        protected virtual string AutomaticIndexSetQuery(bool enable) => PragmaPrefix + "automatic_index = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public async Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default)
        {
            var ms = await Connection.ExecuteScalarAsync<int>(BusyTimeoutReadQuery, cancellationToken).ConfigureAwait(false);
            return TimeSpan.FromMilliseconds(ms);
        }

        public Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(BusyTimeoutSetQuery(timeout), cancellationToken);

        protected virtual string BusyTimeoutReadQuery => PragmaPrefix + "busy_timeout";

        protected virtual string BusyTimeoutSetQuery(TimeSpan timeout)
        {
            var ms = timeout.TotalMilliseconds < 1 ? 0 : (int)timeout.TotalMilliseconds;
            return PragmaPrefix + "busy_timeout = " + ms.ToString(CultureInfo.InvariantCulture);
        }

        public Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(CaseSensitiveLikeSetQuery(enable), cancellationToken);

        protected virtual string CaseSensitiveLikeSetQuery(bool enable) => PragmaPrefix + "case_sensitive_like = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(CellSizeCheckReadQuery, cancellationToken);

        public Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(CellSizeCheckSetQuery(enable), cancellationToken);

        protected virtual string CellSizeCheckReadQuery => PragmaPrefix + "cell_size_check";

        protected virtual string CellSizeCheckSetQuery(bool enable) => PragmaPrefix + "cell_size_check = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(CheckpointFullFsyncReadQuery, cancellationToken);

        public Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(CheckpointFullFsyncSetQuery(enable), cancellationToken);

        protected virtual string CheckpointFullFsyncReadQuery => PragmaPrefix + "checkpoint_fullfsync";

        protected virtual string CheckpointFullFsyncSetQuery(bool enable) => PragmaPrefix + "checkpoint_fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default) => Connection.QueryAsync<pragma_collation_list>(CollationListReadQuery, cancellationToken);

        protected virtual string CollationListReadQuery => PragmaPrefix + "collation_list";

        public Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default) => Connection.QueryAsync<string>(CompileOptionsReadQuery, cancellationToken);

        protected virtual string CompileOptionsReadQuery => PragmaPrefix + "compile_options";

        public Task<int> DataVersionAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<int>(DataVersionReadQuery, cancellationToken);

        protected virtual string DataVersionReadQuery => PragmaPrefix + "data_version";

        public Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default) => Connection.QueryAsync<pragma_database_list>(DatabaseListReadQuery, cancellationToken);

        protected virtual string DatabaseListReadQuery => PragmaPrefix + "database_list";

        public Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(DeferForeignKeysReadQuery, cancellationToken);

        public Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(DeferForeignKeysSetQuery(enable), cancellationToken);

        protected virtual string DeferForeignKeysReadQuery => PragmaPrefix + "defer_foreign_keys";

        protected virtual string DeferForeignKeysSetQuery(bool enable) => PragmaPrefix + "defer_foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public async Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default)
        {
            var encodingName = await Connection.ExecuteScalarAsync<string>(EncodingReadQuery, cancellationToken).ConfigureAwait(false);
            if (!NameEncodingMapping.ContainsKey(encodingName))
                throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

            return NameEncodingMapping[encodingName];
        }

        public Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(EncodingSetQuery(encoding), cancellationToken);

        protected virtual string EncodingReadQuery => PragmaPrefix + "encoding";

        protected virtual string EncodingSetQuery(Encoding encoding)
        {
            if (!encoding.IsValid())
                throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(encoding));

            var value = EncodingNameMapping[encoding];
            return PragmaPrefix + "encoding = '" + value + "'";
        }

        public Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(ForeignKeysReadQuery, cancellationToken);

        public Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(ForeignKeysSetQuery(enable), cancellationToken);

        protected virtual string ForeignKeysReadQuery => PragmaPrefix + "foreign_keys";

        protected virtual string ForeignKeysSetQuery(bool enable) => PragmaPrefix + "foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(FullFsyncReadQuery, cancellationToken);

        public Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(FullFsyncSetQuery(enable), cancellationToken);

        protected virtual string FullFsyncReadQuery => PragmaPrefix + "fullfsync";

        protected virtual string FullFsyncSetQuery(bool enable) => PragmaPrefix + "fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(IgnoreCheckConstraintsSetQuery(enable), cancellationToken);

        protected virtual string IgnoreCheckConstraintsSetQuery(bool enable) => PragmaPrefix + "ignore_check_constraints = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> LegacyAlterTableAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(LegacyAlterTableReadQuery, cancellationToken);

        public Task LegacyAlterTableAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(LegacyAlterTableSetQuery(enable), cancellationToken);

        protected virtual string LegacyAlterTableReadQuery => PragmaPrefix + "legacy_alter_table";

        protected virtual string LegacyAlterTableSetQuery(bool enable) => PragmaPrefix + "legacy_alter_table = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> LegacyFileFormatAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(LegacyFileFormatReadQuery, cancellationToken);

        public Task LegacyFileFormatAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(LegacyFileFormatSetQuery(enable), cancellationToken);

        protected virtual string LegacyFileFormatReadQuery => PragmaPrefix + "legacy_file_format";

        protected virtual string LegacyFileFormatSetQuery(bool enable) => PragmaPrefix + "legacy_file_format = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default) => Connection.QueryAsync<string>(OptimizeSetQuery(features), cancellationToken);

        protected virtual string OptimizeSetQuery(OptimizeFeatures features)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            var value = (int)features;
            return PragmaPrefix + "optimize = " + value;
        }

        public Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(QueryOnlyReadQuery, cancellationToken);

        public Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(QueryOnlySetQuery(enable), cancellationToken);

        protected virtual string QueryOnlyReadQuery => PragmaPrefix + "query_only";

        protected virtual string QueryOnlySetQuery(bool enable) => PragmaPrefix + "query_only = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(ReadUncommittedReadQuery, cancellationToken);

        public Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(ReadUncommittedSetQuery(enable), cancellationToken);

        protected virtual string ReadUncommittedReadQuery => PragmaPrefix + "read_uncommitted";

        protected virtual string ReadUncommittedSetQuery(bool enable) => PragmaPrefix + "read_uncommitted = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(RecursiveTriggersReadQuery, cancellationToken);

        public Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(RecursiveTriggersSetQuery(enable), cancellationToken);

        protected virtual string RecursiveTriggersReadQuery => PragmaPrefix + "recursive_triggers";

        protected virtual string RecursiveTriggersSetQuery(bool enable) => PragmaPrefix + "recursive_triggers = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(ReverseUnorderedSelectsReadQuery, cancellationToken);

        public Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(ReverseUnorderedSelectsSetQuery(enable), cancellationToken);

        protected virtual string ReverseUnorderedSelectsReadQuery => PragmaPrefix + "reverse_unordered_selects";

        protected virtual string ReverseUnorderedSelectsSetQuery(bool enable) => PragmaPrefix + "reverse_unordered_selects = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task ShrinkMemoryAsync(CancellationToken cancellationToken = default) => Connection.ExecuteAsync(ShrinkMemoryQuery, cancellationToken);

        protected virtual string ShrinkMemoryQuery => PragmaPrefix + "shrink_memory";

        public Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<long>(SoftHeapLimitReadQuery, cancellationToken);

        public Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(SoftHeapLimitSetQuery(heapLimit), cancellationToken);

        protected virtual string SoftHeapLimitReadQuery => PragmaPrefix + "soft_heap_limit";

        protected virtual string SoftHeapLimitSetQuery(long heapLimit) => PragmaPrefix + "soft_heap_limit = " + heapLimit.ToString(CultureInfo.InvariantCulture);

        public async Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default)
        {
            var location = await Connection.ExecuteScalarAsync<int>(TemporaryStoreReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
                throw new InvalidOperationException($"Unable to map the value '{ location.ToString() }' to a member of { nameof(TemporaryStoreLocation) }.");

            return tempLocation;
        }

        public Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(TemporaryStoreSetQuery(tempLocation), cancellationToken);

        protected virtual string TemporaryStoreReadQuery => PragmaPrefix + "temp_store";

        protected virtual string TemporaryStoreSetQuery(TemporaryStoreLocation tempLocation)
        {
            if (!tempLocation.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(tempLocation));

            var value = tempLocation.ToString().ToUpperInvariant();
            return PragmaPrefix + "temp_store = " + value;
        }

        public Task<int> ThreadsAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<int>(ThreadsReadQuery, cancellationToken);

        public Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(ThreadsSetQuery(maxThreads), cancellationToken);

        protected virtual string ThreadsReadQuery => PragmaPrefix + "threads";

        protected virtual string ThreadsSetQuery(int maxThreads) => PragmaPrefix + "threads = " + maxThreads.ToString(CultureInfo.InvariantCulture);

        public Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<int>(WalAutoCheckpointReadQuery, cancellationToken);

        public Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(WalAutoCheckpointSetQuery(maxPages), cancellationToken);

        protected virtual string WalAutoCheckpointReadQuery => PragmaPrefix + "wal_autocheckpoint";

        protected virtual string WalAutoCheckpointSetQuery(int maxPages) => PragmaPrefix + "wal_autocheckpoint = " + maxPages.ToString(CultureInfo.InvariantCulture);

        public Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default) => Connection.ExecuteScalarAsync<bool>(WritableSchemaReadQuery, cancellationToken);

        public Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default) => Connection.ExecuteAsync(WritableSchemaSetQuery(enable), cancellationToken);

        protected virtual string WritableSchemaReadQuery => PragmaPrefix + "writable_schema";

        protected virtual string WritableSchemaSetQuery(bool enable) => PragmaPrefix + "writable_schema = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        private static readonly IReadOnlyDictionary<Encoding, string> EncodingNameMapping = new Dictionary<Encoding, string>
        {
            [Encoding.Utf8] = "UTF-8",
            [Encoding.Utf16] = "UTF-16",
            [Encoding.Utf16le] = "UTF-16le",
            [Encoding.Utf16be] = "UTF-16be"
        };

        private static readonly IReadOnlyDictionary<string, Encoding> NameEncodingMapping = new Dictionary<string, Encoding>
        {
            ["UTF-8"] = Encoding.Utf8,
            ["UTF-16"] = Encoding.Utf16,
            ["UTF-16le"] = Encoding.Utf16le,
            ["UTF-16be"] = Encoding.Utf16be
        };
    }
}
