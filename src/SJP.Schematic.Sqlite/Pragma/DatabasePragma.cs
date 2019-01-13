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
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    public class DatabasePragma : ISqliteDatabasePragma
    {
        public DatabasePragma(IDatabaseDialect dialect, IDbConnection connection, string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            SchemaName = schemaName;
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            PragmaPrefix = "PRAGMA " + Dialect.QuoteIdentifier(schemaName) + ".";
        }

        public string SchemaName { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected string PragmaPrefix { get; }

        public Task<uint> ApplicationIdAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<uint>(ApplicationIdReadQuery, cancellationToken);

        public Task ApplicationIdAsync(uint appId, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ApplicationIdSetQuery(appId), cancellationToken);

        protected virtual string ApplicationIdReadQuery => PragmaPrefix + "application_id";

        protected virtual string ApplicationIdSetQuery(uint appId) => PragmaPrefix + "application_id = " + appId.ToString(CultureInfo.InvariantCulture);

        public Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<AutoVacuumMode>(AutoVacuumReadQuery, cancellationToken);

        public Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(AutoVacuumSetQuery(autoVacuumMode), cancellationToken);

        protected virtual string AutoVacuumReadQuery => PragmaPrefix + "auto_vacuum";

        protected virtual string AutoVacuumSetQuery(AutoVacuumMode autoVacuumMode)
        {
            if (!autoVacuumMode.IsValid())
                throw new ArgumentException($"The { nameof(AutoVacuumMode) } provided must be a valid enum.", nameof(autoVacuumMode));

            var value = autoVacuumMode.ToString().ToLowerInvariant();
            return PragmaPrefix + "auto_vacuum = " + value;
        }

        public async Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var size = await Connection.ExecuteScalarAsync<long>(CacheSizeInPagesReadQuery, cancellationToken).ConfigureAwait(false);
            if (size < 0)
            {
                var pageSize = await PageSizeAsync(cancellationToken).ConfigureAwait(false);
                size /= -pageSize / 1024;
            }

            return (ulong)size;
        }

        public Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSizeInPagesSetQuery(cacheSize), cancellationToken);

        protected virtual string CacheSizeInPagesReadQuery => PragmaPrefix + "cache_size";

        protected virtual string CacheSizeInPagesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = " + cacheSize.ToString(CultureInfo.InvariantCulture);

        public async Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var size = await Connection.ExecuteScalarAsync<long>(CacheSizeInKibibytesReadQuery, cancellationToken).ConfigureAwait(false);
            if (size > 0)
                size *= await PageSizeAsync(cancellationToken).ConfigureAwait(false) / 1024;
            else
                size *= -1;

            return (ulong)size;
        }

        public Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSizeInKibibytesSetQuery(cacheSize), cancellationToken);

        protected virtual string CacheSizeInKibibytesReadQuery => PragmaPrefix + "cache_size";

        protected virtual string CacheSizeInKibibytesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = -" + cacheSize.ToString(CultureInfo.InvariantCulture);

        public Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(CacheSpillReadQuery, cancellationToken);

        public Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSpillSetQuery(enable), cancellationToken);

        protected virtual string CacheSpillReadQuery => PragmaPrefix + "cache_spill";

        protected virtual string CacheSpillSetQuery(bool enable) => PragmaPrefix + "cache_spill = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public Task<int> DataVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(DataVersionQuery, cancellationToken);

        protected virtual string DataVersionQuery => PragmaPrefix + "data_version";

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckDatabaseQuery, cancellationToken);

        protected virtual string ForeignKeyCheckDatabaseQuery => PragmaPrefix + "foreign_key_check";

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckTableQuery(tableName), cancellationToken);
        }

        protected virtual string ForeignKeyCheckTableQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_check(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_foreign_key_list>(ForeignKeyListQuery(tableName), cancellationToken);
        }

        protected virtual string ForeignKeyListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(FreeListCountQuery, cancellationToken);

        protected virtual string FreeListCountQuery => PragmaPrefix + "freelist_count";

        public Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(IncrementalVacuumQuery(pages), cancellationToken);

        protected virtual string IncrementalVacuumQuery(ulong pages)
        {
            return pages < 1
                ? PragmaPrefix + "incremental_vacuum"
                : PragmaPrefix + "incremental_vacuum = " + pages.ToString(CultureInfo.InvariantCulture);
        }

        public Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_index_info>(IndexInfoQuery(indexName), cancellationToken);

        protected virtual string IndexInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_info(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        public Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_index_list>(IndexListQuery(tableName), cancellationToken);
        }

        protected virtual string IndexListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "index_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return Connection.QueryAsync<pragma_index_xinfo>(IndexXInfoQuery(indexName), cancellationToken);
        }

        protected virtual string IndexXInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_xinfo(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        public async Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sql = IntegrityCheckQuery(maxErrors);
            var errors = await Connection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        protected virtual string IntegrityCheckQuery(uint maxErrors)
        {
            return maxErrors == 0
                ? PragmaPrefix + "integrity_check"
                : PragmaPrefix + "integrity_check(" + maxErrors.ToString(CultureInfo.InvariantCulture) + ")";
        }

        public async Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var journalModeName = await Connection.ExecuteScalarAsync<string>(JournalModeReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

            return journalMode;
        }

        public Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(JournalModeSetQuery(journalMode), cancellationToken);

        protected virtual string JournalModeReadQuery => PragmaPrefix + "journal_mode";

        protected virtual string JournalModeSetQuery(JournalMode journalMode)
        {
            if (!journalMode.IsValid())
                throw new ArgumentException($"The { nameof(JournalMode) } provided must be a valid enum.", nameof(journalMode));

            var value = journalMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "journal_mode = " + value;
        }

        public Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<long>(JournalSizeLimitReadQuery, cancellationToken);

        public Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(JournalSizeLimitSetQuery(sizeLimit), cancellationToken);

        protected virtual string JournalSizeLimitReadQuery => PragmaPrefix + "journal_size_limit";

        protected virtual string JournalSizeLimitSetQuery(long sizeLimit) => PragmaPrefix + "journal_size_limit = " + sizeLimit.ToString(CultureInfo.InvariantCulture);

        public async Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var lockingModeName = await Connection.ExecuteScalarAsync<string>(LockingModeReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

            return lockingMode;
        }

        public Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(LockingModeSetQuery(lockingMode), cancellationToken);

        protected virtual string LockingModeReadQuery => PragmaPrefix + "locking_mode";

        protected virtual string LockingModeSetQuery(LockingMode lockingMode)
        {
            if (!lockingMode.IsValid())
                throw new ArgumentException($"The { nameof(LockingMode) } provided must be a valid enum.", nameof(lockingMode));

            var value = lockingMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "locking_mode = " + value;
        }

        public Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(MaxPageCountReadQuery, cancellationToken);

        public Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(MaxPageCountSetQuery(maxPageCount), cancellationToken);

        protected virtual string MaxPageCountReadQuery => PragmaPrefix + "max_page_count";

        protected virtual string MaxPageCountSetQuery(ulong maxPageCount) => PragmaPrefix + "max_page_count = " + maxPageCount.ToString(CultureInfo.InvariantCulture);

        public Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(MmapSizeReadQuery, cancellationToken);

        public Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(MmapSizeSetQuery(mmapLimit), cancellationToken);

        protected virtual string MmapSizeReadQuery => PragmaPrefix + "mmap_size";

        protected virtual string MmapSizeSetQuery(ulong mmapLimit) => PragmaPrefix + "mmap_size = " + mmapLimit.ToString(CultureInfo.InvariantCulture);

        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<string>(OptimizeSetQuery(features), cancellationToken);

        protected virtual string OptimizeSetQuery(OptimizeFeatures features)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            var value = (int)features;
            return PragmaPrefix + "optimize = " + value;
        }

        public Task<ulong> PageCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(PageCountReadQuery, cancellationToken);

        protected virtual string PageCountReadQuery => PragmaPrefix + "page_count";

        public Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ushort>(PageSizeReadQuery, cancellationToken);

        public Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(PageSizeSetQuery(pageSize), cancellationToken);

        protected virtual string PageSizeReadQuery => PragmaPrefix + "page_size";

        protected virtual string PageSizeSetQuery(ushort pageSize)
        {
            if (pageSize < 512)
                throw new ArgumentException("A page size must be a power of two whose value is at least 512. Given: " + pageSize.ToString(CultureInfo.InvariantCulture), nameof(pageSize));
            if (!IsPowerOfTwo(pageSize))
                throw new ArgumentException("A page size must be a power of two. Given: " + pageSize.ToString(CultureInfo.InvariantCulture), nameof(pageSize));

            return PragmaPrefix + "page_size = " + pageSize.ToString(CultureInfo.InvariantCulture);
        }

        protected static bool IsPowerOfTwo(ulong value) => value != 0 && (value & (value - 1)) == 0;

        public async Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sql = QuickCheckQuery(maxErrors);
            var errors = await Connection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        protected virtual string QuickCheckQuery(uint maxErrors)
        {
            return maxErrors == 0
                ? PragmaPrefix + "quick_check"
                : PragmaPrefix + "quick_check(" + maxErrors.ToString(CultureInfo.InvariantCulture) + ")";
        }

        public Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(SchemaVersionReadQuery, cancellationToken);

        public Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SchemaVersionSetQuery(schemaVersion), cancellationToken);

        protected virtual string SchemaVersionReadQuery => PragmaPrefix + "schema_version";

        protected virtual string SchemaVersionSetQuery(int schemaVersion) => PragmaPrefix + "schema_version = " + schemaVersion.ToString(CultureInfo.InvariantCulture);

        public async Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var secureDeleteValue = await Connection.ExecuteScalarAsync<int>(SecureDeleteReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue.ToString() }' to a member of { nameof(SecureDeleteMode) }.");

            return deleteMode;
        }

        public Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SecureDeleteSetQuery(deleteMode), cancellationToken);

        protected virtual string SecureDeleteReadQuery => PragmaPrefix + "secure_delete";

        protected virtual string SecureDeleteSetQuery(SecureDeleteMode deleteMode)
        {
            if (!deleteMode.IsValid())
                throw new ArgumentException($"The { nameof(SecureDeleteMode) } provided must be a valid enum.", nameof(deleteMode));

            var value = deleteMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "secure_delete = " + value;
        }

        public async Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var level = await Connection.ExecuteScalarAsync<int>(SynchronousReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                throw new InvalidOperationException($"Unable to map the value '{ level.ToString() }' to a member of { nameof(SynchronousLevel) }.");

            return syncLevel;
        }

        public Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SynchronousSetQuery(synchronousLevel), cancellationToken);

        protected virtual string SynchronousReadQuery => PragmaPrefix + "synchronous";

        protected virtual string SynchronousSetQuery(SynchronousLevel synchronousLevel)
        {
            if (!synchronousLevel.IsValid())
                throw new ArgumentException($"The { nameof(SynchronousLevel) } provided must be a valid enum.", nameof(synchronousLevel));

            var value = synchronousLevel.ToString().ToUpperInvariant();
            return PragmaPrefix + "synchronous = " + value;
        }

        public Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_table_info>(TableInfoQuery(tableName), cancellationToken);
        }

        protected virtual string TableInfoQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "table_info(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public Task<IEnumerable<pragma_table_xinfo>> TableXInfoAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_table_xinfo>(TableXInfoQuery(tableName), cancellationToken);
        }

        protected virtual string TableXInfoQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "table_xinfo(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public Task<int> UserVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(UserVersionReadQuery, cancellationToken);

        public Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(UserVersionSetQuery(userVersion), cancellationToken);

        protected virtual string UserVersionReadQuery => PragmaPrefix + "user_version";

        protected virtual string UserVersionSetQuery(int userVersion) => PragmaPrefix + "user_version = " + userVersion.ToString(CultureInfo.InvariantCulture);

        public Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            return WalCheckpointAsyncCore(checkpointMode, cancellationToken);
        }

        private async Task<pragma_wal_checkpoint> WalCheckpointAsyncCore(WalCheckpointMode checkpointMode, CancellationToken cancellationToken)
        {
            var result = await Connection.QueryAsync<pragma_wal_checkpoint>(WalCheckpointQuery(checkpointMode), cancellationToken).ConfigureAwait(false);
            return result.Single();
        }

        protected virtual string WalCheckpointQuery(WalCheckpointMode checkpointMode)
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            var checkpointModeStr = checkpointMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "wal_checkpoint(" + checkpointModeStr + ")";
        }
    }
}
