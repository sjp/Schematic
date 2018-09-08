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

        public uint ApplicationId
        {
            get => Connection.ExecuteScalar<uint>(ApplicationIdReadQuery);
            set => Connection.Execute(ApplicationIdSetQuery(value));
        }

        public Task<uint> ApplicationIdAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<uint>(ApplicationIdReadQuery);

        public Task ApplicationIdAsync(uint appId, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(ApplicationIdSetQuery(appId));

        protected virtual string ApplicationIdReadQuery => PragmaPrefix + "application_id";

        protected virtual string ApplicationIdSetQuery(uint appId) => PragmaPrefix + "application_id = " + appId.ToString(CultureInfo.InvariantCulture);

        public AutoVacuumMode AutoVacuum
        {
            get => Connection.ExecuteScalar<AutoVacuumMode>(AutoVacuumReadQuery);
            set => Connection.Execute(AutoVacuumSetQuery(value));
        }

        public Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<AutoVacuumMode>(AutoVacuumReadQuery);

        public Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(AutoVacuumSetQuery(autoVacuumMode));

        protected virtual string AutoVacuumReadQuery => PragmaPrefix + "auto_vacuum";

        protected virtual string AutoVacuumSetQuery(AutoVacuumMode autoVacuumMode)
        {
            if (!autoVacuumMode.IsValid())
                throw new ArgumentException($"The { nameof(AutoVacuumMode) } provided must be a valid enum.", nameof(autoVacuumMode));

            var value = autoVacuumMode.ToString().ToLowerInvariant();
            return PragmaPrefix + "auto_vacuum = " + value;
        }

        public ulong CacheSizeInPages
        {
            get
            {
                var size = Connection.ExecuteScalar<long>(CacheSizeInPagesReadQuery);
                if (size < 0)
                    size /= -PageSize / 1024;

                return (ulong)size;
            }
            set => Connection.Execute(CacheSizeInPagesSetQuery(value));
        }

        public async Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var size = await Connection.ExecuteScalarAsync<long>(CacheSizeInPagesReadQuery).ConfigureAwait(false);
            if (size < 0)
                size /= -PageSize / 1024;

            return (ulong)size;
        }

        public Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSizeInPagesSetQuery(cacheSize));

        protected virtual string CacheSizeInPagesReadQuery => PragmaPrefix + "cache_size";

        protected virtual string CacheSizeInPagesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = " + cacheSize.ToString(CultureInfo.InvariantCulture);

        public ulong CacheSizeInKibibytes
        {
            get
            {
                var size = Connection.ExecuteScalar<long>(CacheSizeInKibibytesReadQuery);
                if (size > 0)
                    size *= PageSize / 1024;
                else
                    size *= -1;

                return (ulong)size;
            }
            set => Connection.Execute(CacheSizeInKibibytesSetQuery(value));
        }

        public async Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var size = await Connection.ExecuteScalarAsync<long>(CacheSizeInKibibytesReadQuery).ConfigureAwait(false);
            if (size > 0)
                size *= await PageSizeAsync(cancellationToken).ConfigureAwait(false) / 1024;
            else
                size *= -1;

            return (ulong)size;
        }

        public Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSizeInKibibytesSetQuery(cacheSize));

        protected virtual string CacheSizeInKibibytesReadQuery => PragmaPrefix + "cache_size";

        protected virtual string CacheSizeInKibibytesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = -" + cacheSize.ToString(CultureInfo.InvariantCulture);

        public bool CacheSpill
        {
            get => Connection.ExecuteScalar<bool>(CacheSpillReadQuery);
            set => Connection.Execute(CacheSpillSetQuery(value));
        }

        public Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<bool>(CacheSpillReadQuery);

        public Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSpillSetQuery(enable));

        protected virtual string CacheSpillReadQuery => PragmaPrefix + "cache_spill";

        protected virtual string CacheSpillSetQuery(bool enable) => PragmaPrefix + "cache_spill = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        public uint CacheSpillInPages
        {
            get => Connection.ExecuteScalar<uint>(CacheSpillInPagesQuery);
            set => Connection.Execute(CacheSpillInPagesSetQuery(value));
        }

        public Task<uint> CacheSpillInPagesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<uint>(CacheSpillInPagesQuery);

        public Task CacheSpillInPagesAsync(uint pageSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(CacheSpillInPagesSetQuery(pageSize));

        protected virtual string CacheSpillInPagesQuery => PragmaPrefix + "page_size";

        protected virtual string CacheSpillInPagesSetQuery(uint pageSize) => PragmaPrefix + "page_size = " + pageSize.ToString(CultureInfo.InvariantCulture);

        public int DataVersion => Connection.ExecuteScalar<int>(DataVersionQuery);

        public Task<int> DataVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(DataVersionQuery);

        protected virtual string DataVersionQuery => PragmaPrefix + "data_version";

        public IEnumerable<pragma_foreign_key_check> ForeignKeyCheckDatabase => Connection.Query<pragma_foreign_key_check>(ForeignKeyCheckDatabaseQuery);

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckDatabaseQuery);

        protected virtual string ForeignKeyCheckDatabaseQuery => PragmaPrefix + "foreign_key_check";

        public IEnumerable<pragma_foreign_key_check> ForeignKeyCheckTable(Identifier tableName) => Connection.Query<pragma_foreign_key_check>(ForeignKeyCheckTableQuery(tableName));

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckTableQuery(tableName));

        protected virtual string ForeignKeyCheckTableQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_check(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public IEnumerable<pragma_foreign_key_list> ForeignKeyList(Identifier tableName) => Connection.Query<pragma_foreign_key_list>(ForeignKeyListQuery(tableName));

        public Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_foreign_key_list>(ForeignKeyListQuery(tableName));

        protected virtual string ForeignKeyListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public ulong FreeListCount => Connection.ExecuteScalar<ulong>(FreeListCountQuery);

        public Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(FreeListCountQuery);

        protected virtual string FreeListCountQuery => PragmaPrefix + "freelist_count";

        public void IncrementalVacuum(ulong pages = 0) => Connection.Execute(IncrementalVacuumQuery(pages));

        public Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(IncrementalVacuumQuery(pages));

        protected virtual string IncrementalVacuumReadQuery => PragmaPrefix + "incremental_vacuum";

        protected virtual string IncrementalVacuumQuery(ulong pages)
        {
            return pages < 1
                ? PragmaPrefix + "incremental_vacuum"
                : PragmaPrefix + "incremental_vacuum = " + pages.ToString(CultureInfo.InvariantCulture);
        }

        public IEnumerable<pragma_index_info> IndexInfo(string indexName) => Connection.Query<pragma_index_info>(IndexInfoQuery(indexName));

        public Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_index_info>(IndexInfoQuery(indexName));

        protected virtual string IndexInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_info(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        public IEnumerable<pragma_index_list> IndexList(Identifier tableName) => Connection.Query<pragma_index_list>(IndexListQuery(tableName));

        public Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_index_list>(IndexListQuery(tableName));

        protected virtual string IndexListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "index_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public IEnumerable<pragma_index_xinfo> IndexXInfo(string indexName) => Connection.Query<pragma_index_xinfo>(IndexXInfoQuery(indexName));

        public Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_index_xinfo>(IndexXInfoQuery(indexName));

        protected virtual string IndexXInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_xinfo(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        public IEnumerable<string> IntegrityCheck(uint maxErrors = 0)
        {
            var sql = IntegrityCheckQuery(maxErrors);
            var result = Connection.Query<string>(sql).ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        public async Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sql = IntegrityCheckQuery(maxErrors);
            var errors = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
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

        public JournalMode JournalMode
        {
            get
            {
                var journalModeName = Connection.ExecuteScalar<string>(JournalModeReadQuery);
                if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                    throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

                return journalMode;
            }
            set => Connection.Execute(JournalModeSetQuery(value));
        }

        public async Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var journalModeName = await Connection.ExecuteScalarAsync<string>(JournalModeReadQuery).ConfigureAwait(false);
            if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

            return journalMode;
        }

        public Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(JournalModeSetQuery(journalMode));

        protected virtual string JournalModeReadQuery => PragmaPrefix + "journal_mode";

        protected virtual string JournalModeSetQuery(JournalMode journalMode)
        {
            if (!journalMode.IsValid())
                throw new ArgumentException($"The { nameof(JournalMode) } provided must be a valid enum.", nameof(journalMode));

            var value = journalMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "journal_mode = " + value;
        }

        public long JournalSizeLimit
        {
            get => Connection.ExecuteScalar<long>(JournalSizeLimitReadQuery);
            set => Connection.Execute(JournalSizeLimitSetQuery(value));
        }

        public Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<long>(JournalSizeLimitReadQuery);

        public Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(JournalSizeLimitSetQuery(sizeLimit));

        protected virtual string JournalSizeLimitReadQuery => PragmaPrefix + "journal_size_limit";

        protected virtual string JournalSizeLimitSetQuery(long sizeLimit) => PragmaPrefix + "journal_size_limit = " + sizeLimit.ToString(CultureInfo.InvariantCulture);

        public LockingMode LockingMode
        {
            get
            {
                var lockingModeName = Connection.ExecuteScalar<string>(LockingModeReadQuery);
                if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                    throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

                return lockingMode;
            }
            set => Connection.Execute(LockingModeSetQuery(value));
        }

        public async Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var lockingModeName = await Connection.ExecuteScalarAsync<string>(LockingModeReadQuery).ConfigureAwait(false);
            if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

            return lockingMode;
        }

        public Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(LockingModeSetQuery(lockingMode));

        protected virtual string LockingModeReadQuery => PragmaPrefix + "locking_mode";

        protected virtual string LockingModeSetQuery(LockingMode lockingMode)
        {
            if (!lockingMode.IsValid())
                throw new ArgumentException($"The { nameof(LockingMode) } provided must be a valid enum.", nameof(lockingMode));

            var value = lockingMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "locking_mode = " + value;
        }

        public ulong MaxPageCount
        {
            get => Connection.ExecuteScalar<ulong>(MaxPageCountReadQuery);
            set => Connection.Execute(MaxPageCountSetQuery(value));
        }

        public Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(MaxPageCountReadQuery);

        public Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(MaxPageCountSetQuery(maxPageCount));

        protected virtual string MaxPageCountReadQuery => PragmaPrefix + "max_page_count";

        protected virtual string MaxPageCountSetQuery(ulong maxPageCount) => PragmaPrefix + "max_page_count = " + maxPageCount.ToString(CultureInfo.InvariantCulture);

        public ulong MmapSize
        {
            get => Connection.ExecuteScalar<ulong>(MmapSizeReadQuery);
            set => Connection.Execute(MmapSizeSetQuery(value));
        }

        public Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(MmapSizeReadQuery);

        public Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(MmapSizeSetQuery(mmapLimit));

        protected virtual string MmapSizeReadQuery => PragmaPrefix + "mmap_size";

        protected virtual string MmapSizeSetQuery(ulong mmapLimit) => PragmaPrefix + "mmap_size = " + mmapLimit.ToString(CultureInfo.InvariantCulture);

        public IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze) => Connection.Query<string>(OptimizeSetQuery(features));

        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<string>(OptimizeSetQuery(features));

        protected virtual string OptimizeSetQuery(OptimizeFeatures features)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            var value = (int)features;
            return PragmaPrefix + "optimize = " + value;
        }

        public ulong PageCount => Connection.ExecuteScalar<ulong>(PageCountReadQuery);

        public Task<ulong> PageCountAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ulong>(PageCountReadQuery);

        protected virtual string PageCountReadQuery => PragmaPrefix + "page_count";

        public ushort PageSize
        {
            get => Connection.ExecuteScalar<ushort>(PageSizeReadQuery);
            set => Connection.Execute(PageSizeSetQuery(value));
        }

        public Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<ushort>(PageSizeReadQuery);

        public Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(PageSizeSetQuery(pageSize));

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

        public IEnumerable<string> QuickCheck(uint maxErrors = 0)
        {
            var sql = QuickCheckQuery(maxErrors);
            var result = Connection.Query<string>(sql).ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        public async Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            var sql = QuickCheckQuery(maxErrors);
            var errors = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
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

        public int SchemaVersion
        {
            get => Connection.ExecuteScalar<int>(SchemaVersionReadQuery);
            set => Connection.Execute(SchemaVersionSetQuery(value));
        }

        public Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(SchemaVersionReadQuery);

        public Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SchemaVersionSetQuery(schemaVersion));

        protected virtual string SchemaVersionReadQuery => PragmaPrefix + "schema_version";

        protected virtual string SchemaVersionSetQuery(int schemaVersion) => PragmaPrefix + "schema_version = " + schemaVersion.ToString(CultureInfo.InvariantCulture);

        public SecureDeleteMode SecureDelete
        {
            get
            {
                var secureDeleteValue = Connection.ExecuteScalar<int>(SecureDeleteReadQuery);
                if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                    throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue.ToString() }' to a member of { nameof(SecureDeleteMode) }.");

                return deleteMode;
            }
            set => Connection.Execute(SecureDeleteSetQuery(value));
        }

        public async Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var secureDeleteValue = await Connection.ExecuteScalarAsync<int>(SecureDeleteReadQuery).ConfigureAwait(false);
            if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue.ToString() }' to a member of { nameof(SecureDeleteMode) }.");

            return deleteMode;
        }

        public Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SecureDeleteSetQuery(deleteMode));

        protected virtual string SecureDeleteReadQuery => PragmaPrefix + "secure_delete";

        protected virtual string SecureDeleteSetQuery(SecureDeleteMode deleteMode)
        {
            if (!deleteMode.IsValid())
                throw new ArgumentException($"The { nameof(SecureDeleteMode) } provided must be a valid enum.", nameof(deleteMode));

            var value = deleteMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "secure_delete = " + value;
        }

        public SynchronousLevel Synchronous
        {
            get
            {
                var level = Connection.ExecuteScalar<int>(SynchronousReadQuery);
                if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                    throw new InvalidOperationException($"Unable to map the value '{ level.ToString() }' to a member of { nameof(SynchronousLevel) }.");

                return syncLevel;
            }
            set => Connection.Execute(SynchronousSetQuery(value));
        }

        public async Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var level = await Connection.ExecuteScalarAsync<int>(SynchronousReadQuery).ConfigureAwait(false);
            if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                throw new InvalidOperationException($"Unable to map the value '{ level.ToString() }' to a member of { nameof(SynchronousLevel) }.");

            return syncLevel;
        }

        public Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(SynchronousSetQuery(synchronousLevel));

        protected virtual string SynchronousReadQuery => PragmaPrefix + "synchronous";

        protected virtual string SynchronousSetQuery(SynchronousLevel synchronousLevel)
        {
            if (!synchronousLevel.IsValid())
                throw new ArgumentException($"The { nameof(SynchronousLevel) } provided must be a valid enum.", nameof(synchronousLevel));

            var value = synchronousLevel.ToString().ToUpperInvariant();
            return PragmaPrefix + "synchronous = " + value;
        }

        public IEnumerable<pragma_table_info> TableInfo(Identifier tableName) => Connection.Query<pragma_table_info>(TableInfoQuery(tableName));

        public Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Connection.QueryAsync<pragma_table_info>(TableInfoQuery(tableName));

        protected virtual string TableInfoQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "table_info(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        public int UserVersion
        {
            get => Connection.ExecuteScalar<int>(UserVersionReadQuery);
            set => Connection.Execute(UserVersionSetQuery(value));
        }

        public Task<int> UserVersionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteScalarAsync<int>(UserVersionReadQuery);

        public Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default(CancellationToken)) => Connection.ExecuteAsync(UserVersionSetQuery(userVersion));

        protected virtual string UserVersionReadQuery => PragmaPrefix + "user_version";

        protected virtual string UserVersionSetQuery(int userVersion) => PragmaPrefix + "user_version = " + userVersion.ToString(CultureInfo.InvariantCulture);

        public pragma_wal_checkpoint WalCheckpoint(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive) => Connection.Query<pragma_wal_checkpoint>(WalCheckpointQuery(checkpointMode)).Single();

        public Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            return WalCheckpointAsyncCore(checkpointMode, cancellationToken);
        }

        private async Task<pragma_wal_checkpoint> WalCheckpointAsyncCore(WalCheckpointMode checkpointMode, CancellationToken cancellationToken)
        {
            var result = await Connection.QueryAsync<pragma_wal_checkpoint>(WalCheckpointQuery(checkpointMode)).ConfigureAwait(false);
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
