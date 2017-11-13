using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    public class DatabasePragma
    {
        public DatabasePragma(IDatabaseDialect dialect, IDbConnection connection, string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            PragmaPrefix = $"PRAGMA { Dialect.QuoteIdentifier(schemaName) }.";
        }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected string PragmaPrefix { get; }

        public uint ApplicationId
        {
            get => Connection.ExecuteScalar<uint>(PragmaPrefix + "application_id");
            set => Connection.Execute(PragmaPrefix + $"application_id = { value.ToString() }");
        }

        public Task<uint> ApplicationIdAsync() => Connection.ExecuteScalarAsync<uint>(PragmaPrefix + "application_id");

        public Task ApplicationIdAsync(uint appId) => Connection.ExecuteAsync(PragmaPrefix + $"application_id = { appId.ToString() }");

        public AutoVacuumMode AutoVacuum
        {
            get => Connection.ExecuteScalar<AutoVacuumMode>(PragmaPrefix + "auto_vacuum");
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(AutoVacuumMode) } provided must be a valid enum.", nameof(AutoVacuum));

                Connection.Execute(PragmaPrefix + $"auto_vacuum = { value.ToString() }");
            }
        }

        public Task<AutoVacuumMode> AutoVacuumAsync() => Connection.ExecuteScalarAsync<AutoVacuumMode>(PragmaPrefix + "auto_vacuum");

        public Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode)
        {
            if (!autoVacuumMode.IsValid())
                throw new ArgumentException($"The { nameof(AutoVacuumMode) } provided must be a valid enum.", nameof(autoVacuumMode));

            return Connection.ExecuteAsync(PragmaPrefix + $"auto_vacuum = { autoVacuumMode.ToString().ToLowerInvariant() }");
        }

        public ulong CacheSizeInPages
        {
            get
            {
                var size = Connection.ExecuteScalar<long>(PragmaPrefix + "cache_size");
                if (size < 0)
                    size /= -PageSize / 1024;

                return (ulong)size;
            }
            set => Connection.Execute(PragmaPrefix + $"cache_size = { value.ToString() }");
        }

        public async Task<ulong> CacheSizeInPagesAsync()
        {
            var size = await Connection.ExecuteScalarAsync<long>(PragmaPrefix + "cache_size").ConfigureAwait(false);
            if (size < 0)
                size /= -PageSize / 1024;

            return (ulong)size;
        }

        public Task CacheSizeInPagesAsync(ulong cacheSize) => Connection.ExecuteAsync(PragmaPrefix + $"cache_size = { cacheSize.ToString() }");

        public ulong CacheSizeInKibibytes
        {
            get
            {
                var size = Connection.ExecuteScalar<long>(PragmaPrefix + "cache_size");
                if (size > 0)
                    size *= PageSize / 1024;
                else
                    size *= -1;

                return (ulong)size;
            }
            set => Connection.Execute(PragmaPrefix + $"cache_size = -{ value.ToString() }");
        }

        public async Task<ulong> CacheSizeInKibibytesAsync()
        {
            var size = await Connection.ExecuteScalarAsync<long>(PragmaPrefix + "cache_size").ConfigureAwait(false);
            if (size > 0)
                size *= await PageSizeAsync().ConfigureAwait(false) / 1024;
            else
                size *= -1;

            return (ulong)size;
        }

        public Task CacheSizeInKibibytesAsync(ulong cacheSize) => Connection.ExecuteAsync(PragmaPrefix + $"cache_size = -{ cacheSize.ToString() }");

        public bool CacheSpill
        {
            get => Connection.ExecuteScalar<bool>(PragmaPrefix + "cache_spill");
            set => Connection.Execute(PragmaPrefix + $"cache_spill = { Convert.ToInt32(value).ToString() }");
        }

        public Task<bool> CacheSpillAsync() => Connection.ExecuteScalarAsync<bool>(PragmaPrefix + "cache_spill");

        public Task CacheSpillAsync(bool enable) => Connection.ExecuteAsync(PragmaPrefix + $"cache_spill = { Convert.ToInt32(enable).ToString() }");

        public uint CacheSpillInPages
        {
            get => Connection.ExecuteScalar<uint>(PragmaPrefix + "page_size");
            set => Connection.Execute(PragmaPrefix + $"page_size = { value.ToString() }");
        }

        public Task<uint> CacheSpillInPagesAsync() => Connection.ExecuteScalarAsync<uint>(PragmaPrefix + "page_size");

        public Task CacheSpillInPagesAsync(uint pageSize) => Connection.ExecuteAsync(PragmaPrefix + $"page_size = { pageSize.ToString() }");

        public int DataVersion => Connection.ExecuteScalar<int>(PragmaPrefix + "data_version");

        public Task<int> DataVersionAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "data_version");

        public IEnumerable<pragma_foreign_key_check> ForeignKeyCheckDatabase => Connection.Query<pragma_foreign_key_check>(PragmaPrefix + "foreign_key_check");

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync() => Connection.QueryAsync<pragma_foreign_key_check>(PragmaPrefix + "foreign_key_check");

        public IEnumerable<pragma_foreign_key_check> ForeignKeyCheckTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.Query<pragma_foreign_key_check>(PragmaPrefix + $"foreign_key_check({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_foreign_key_check>(PragmaPrefix + $"foreign_key_check({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public IEnumerable<pragma_foreign_key_list> ForeignKeyList(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.Query<pragma_foreign_key_list>(PragmaPrefix + $"foreign_key_list({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_foreign_key_list>(PragmaPrefix + $"foreign_key_list({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public ulong FreeListCount => Connection.ExecuteScalar<ulong>(PragmaPrefix + "freelist_count");

        public Task<ulong> FreeListCountAsync() => Connection.ExecuteScalarAsync<ulong>(PragmaPrefix + "freelist_count");

        public void IncrementalVacuum(ulong pages = 0)
        {
            if (pages < 1)
                Connection.Execute(PragmaPrefix + "incremental_vacuum");
            else
                Connection.Execute(PragmaPrefix + $"incremental_vacuum = { pages.ToString() }");
        }

        public Task IncrementalVacuumAsync(ulong pages = 0)
        {
            return pages < 1
                ? Connection.ExecuteAsync(PragmaPrefix + "incremental_vacuum")
                : Connection.ExecuteAsync(PragmaPrefix + $"incremental_vacuum = { pages.ToString() }");
        }

        public IEnumerable<pragma_index_info> IndexInfo(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return Connection.Query<pragma_index_info>(PragmaPrefix + $"index_info({ Dialect.QuoteIdentifier(indexName) })");
        }

        public Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return Connection.QueryAsync<pragma_index_info>(PragmaPrefix + $"index_info({ Dialect.QuoteIdentifier(indexName) })");
        }

        public IEnumerable<pragma_index_list> IndexList(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.Query<pragma_index_list>(PragmaPrefix + $"index_list({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_index_list>(PragmaPrefix + $"index_list({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public IEnumerable<pragma_index_xinfo> IndexXInfo(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return Connection.Query<pragma_index_xinfo>(PragmaPrefix + $"index_xinfo({ Dialect.QuoteIdentifier(indexName) })");
        }

        public Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return Connection.QueryAsync<pragma_index_xinfo>(PragmaPrefix + $"index_xinfo({ Dialect.QuoteIdentifier(indexName) })");
        }

        public IEnumerable<string> IntegrityCheck(uint maxErrors = 0)
        {
            var sql = maxErrors == 0
                ? PragmaPrefix + "integrity_check"
                : PragmaPrefix + $"integrity_check({ maxErrors.ToString() })";
            var result = Connection.Query<string>(sql).ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Enumerable.Empty<string>();

            return result;
        }

        public async Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0)
        {
            var sql = maxErrors == 0
                ? PragmaPrefix + "integrity_check"
                : PragmaPrefix + $"integrity_check({ maxErrors.ToString() })";
            var errors = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Enumerable.Empty<string>();

            return result;
        }

        public JournalMode JournalMode
        {
            get
            {
                var journalModeName = Connection.ExecuteScalar<string>(PragmaPrefix + "journal_mode");
                if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                    throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

                return journalMode;
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(JournalMode) } provided must be a valid enum.", nameof(JournalMode));

                Connection.Execute(PragmaPrefix + $"journal_mode = { value.ToString().ToUpperInvariant() }");
            }
        }

        public async Task<JournalMode> JournalModeAsync()
        {
            var journalModeName = await Connection.ExecuteScalarAsync<string>(PragmaPrefix + "journal_mode").ConfigureAwait(false);
            if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

            return journalMode;
        }

        public Task JournalModeAsync(JournalMode journalMode)
        {
            if (!journalMode.IsValid())
                throw new ArgumentException($"The { nameof(JournalMode) } provided must be a valid enum.", nameof(journalMode));

            var value = journalMode.ToString().ToUpperInvariant();
            return Connection.ExecuteAsync(PragmaPrefix + "journal_mode = " + value);
        }

        public long JournalSizeLimit
        {
            get => Connection.ExecuteScalar<long>(PragmaPrefix + "journal_size_limit");
            set => Connection.Execute(PragmaPrefix + $"journal_size_limit = { value.ToString() }");
        }

        public Task<long> JournalSizeLimitAsync() => Connection.ExecuteScalarAsync<long>(PragmaPrefix + "journal_size_limit");

        public Task JournalSizeLimitAsync(long sizeLimit = -1) => Connection.ExecuteAsync(PragmaPrefix + $"journal_size_limit = { sizeLimit.ToString() }");

        public LockingMode LockingMode
        {
            get
            {
                var lockingModeName = Connection.ExecuteScalar<string>(PragmaPrefix + "locking_mode");
                if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                    throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

                return lockingMode;
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(LockingMode) } provided must be a valid enum.", nameof(LockingMode));

                Connection.Execute(PragmaPrefix + $"locking_mode = { value.ToString().ToUpperInvariant() }");
            }
        }

        public async Task<LockingMode> LockingModeAsync()
        {
            var lockingModeName = await Connection.ExecuteScalarAsync<string>(PragmaPrefix + "locking_mode").ConfigureAwait(false);
            if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

            return lockingMode;
        }

        public Task LockingModeAsync(LockingMode lockingMode)
        {
            if (!lockingMode.IsValid())
                throw new ArgumentException($"The { nameof(LockingMode) } provided must be a valid enum.", nameof(lockingMode));

            var value = lockingMode.ToString().ToUpperInvariant();
            return Connection.ExecuteAsync(PragmaPrefix + "locking_mode = " + value);
        }

        public ulong MaxPageCount
        {
            get => Connection.ExecuteScalar<ulong>(PragmaPrefix + "max_page_count");
            set => Connection.Execute(PragmaPrefix + $"max_page_count = { value.ToString() }");
        }

        public Task<ulong> MaxPageCountAsync() => Connection.ExecuteScalarAsync<ulong>(PragmaPrefix + "max_page_count");

        public Task MaxPageCountAsync(ulong maxPageCount) => Connection.ExecuteAsync(PragmaPrefix + $"max_page_count = { maxPageCount.ToString() }");

        public ulong MmapSize
        {
            get => Connection.ExecuteScalar<ulong>(PragmaPrefix + "mmap_size");
            set => Connection.Execute(PragmaPrefix + $"mmap_size = { value.ToString() }");
        }

        public Task<ulong> MmapSizeAsync() => Connection.ExecuteScalarAsync<ulong>(PragmaPrefix + "mmap_size");

        public Task MmapSizeAsync(ulong mmapLimit) => Connection.ExecuteAsync(PragmaPrefix + $"mmap_size = { mmapLimit.ToString() }");

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

        public ulong PageCount => Connection.ExecuteScalar<ulong>(PragmaPrefix + "page_count");

        public Task<ulong> PageCountAsync() => Connection.ExecuteScalarAsync<ulong>(PragmaPrefix + "page_count");

        public ushort PageSize
        {
            get => Connection.ExecuteScalar<ushort>(PragmaPrefix + "page_size");
            set
            {
                if (value < 512)
                    throw new ArgumentException("A page size must be a power of two whose value is at least 512. Given: " + value.ToString(), nameof(PageSize));

                if (!IsPowerOfTwo(value))
                    throw new ArgumentException("A page size must be a power of two. Given: " + value.ToString(), nameof(PageSize));

                Connection.Execute(PragmaPrefix + $"page_size = { value.ToString() }");
            }
        }

        public Task<ushort> PageSizeAsync() => Connection.ExecuteScalarAsync<ushort>(PragmaPrefix + "page_size");

        public Task PageSizeAsync(ushort pageSize)
        {
            if (pageSize < 512)
                throw new ArgumentException("A page size must be a power of two whose value is at least 512. Given: " + pageSize.ToString(), nameof(pageSize));

            if (!IsPowerOfTwo(pageSize))
                throw new ArgumentException("A page size must be a power of two. Given: " + pageSize.ToString(), nameof(pageSize));

            return Connection.ExecuteAsync(PragmaPrefix + $"page_size = { pageSize.ToString() }");
        }

        protected static bool IsPowerOfTwo(ulong value) => value != 0 && (value & (value - 1)) == 0;

        public IEnumerable<string> QuickCheck(uint maxErrors = 0)
        {
            var sql = maxErrors == 0
                ? PragmaPrefix + "quick_check"
                : PragmaPrefix + $"quick_check({ maxErrors.ToString() })";
            var result = Connection.Query<string>(sql).ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Enumerable.Empty<string>();

            return result;
        }

        public async Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0)
        {
            var sql = maxErrors == 0
                ? PragmaPrefix + "quick_check"
                : PragmaPrefix + $"quick_check({ maxErrors.ToString() })";
            var errors = await Connection.QueryAsync<string>(sql).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Enumerable.Empty<string>();

            return result;
        }

        public int SchemaVersion
        {
            get => Connection.ExecuteScalar<int>(PragmaPrefix + "schema_version");
            set => Connection.Execute(PragmaPrefix + $"schema_version = { value.ToString() }");
        }

        public Task<int> SchemaVersionAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "schema_version");

        public Task SchemaVersionAsync(int schemaVersion) => Connection.ExecuteAsync(PragmaPrefix + $"schema_version = { schemaVersion.ToString() }");

        public SecureDeleteMode SecureDelete
        {
            get
            {
                var secureDeleteValue = Connection.ExecuteScalar<int>(PragmaPrefix + "secure_delete");
                if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                    throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue.ToString() }' to a member of { nameof(SecureDeleteMode) }.");

                return deleteMode;
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(SecureDeleteMode) } provided must be a valid enum.", nameof(SecureDelete));

                Connection.Execute(PragmaPrefix + $"secure_delete = { value.ToString().ToUpperInvariant() }");
            }
        }

        public async Task<SecureDeleteMode> SecureDeleteAsync()
        {
            var secureDeleteValue = await Connection.ExecuteScalarAsync<int>(PragmaPrefix + "secure_delete").ConfigureAwait(false);
            if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue.ToString() }' to a member of { nameof(SecureDeleteMode) }.");

            return deleteMode;
        }

        public Task SecureDeleteAsync(SecureDeleteMode deleteMode)
        {
            if (!deleteMode.IsValid())
                throw new ArgumentException($"The { nameof(SecureDeleteMode) } provided must be a valid enum.", nameof(deleteMode));

            var value = deleteMode.ToString().ToUpperInvariant();
            return Connection.ExecuteAsync(PragmaPrefix + "secure_delete = " + value);
        }

        public SynchronousLevel Synchronous
        {
            get
            {
                var level = Connection.ExecuteScalar<int>(PragmaPrefix + "synchronous");
                if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                    throw new InvalidOperationException($"Unable to map the value '{ level.ToString() }' to a member of { nameof(SynchronousLevel) }.");

                return syncLevel;
            }
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(SynchronousLevel) } provided must be a valid enum.", nameof(Synchronous));

                Connection.Execute(PragmaPrefix + $"synchronous = { Enums.GetUnderlyingValue(value).ToString().ToUpperInvariant() }");
            }
        }

        public async Task<SynchronousLevel> SynchronousAsync()
        {
            var level = await Connection.ExecuteScalarAsync<int>(PragmaPrefix + "synchronous").ConfigureAwait(false);
            if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                throw new InvalidOperationException($"Unable to map the value '{ level.ToString() }' to a member of { nameof(SynchronousLevel) }.");

            return syncLevel;
        }

        public Task SynchronousAsync(SynchronousLevel synchronousLevel)
        {
            if (!synchronousLevel.IsValid())
                throw new ArgumentException($"The { nameof(SynchronousLevel) } provided must be a valid enum.", nameof(synchronousLevel));

            var value = synchronousLevel.ToString().ToUpperInvariant();
            return Connection.ExecuteAsync(PragmaPrefix + "synchronous = " + value);
        }

        public IEnumerable<pragma_table_info> TableInfo(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.Query<pragma_table_info>(PragmaPrefix + $"table_info({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Connection.QueryAsync<pragma_table_info>(PragmaPrefix + $"table_info({ Dialect.QuoteIdentifier(tableName.LocalName) })");
        }

        public int UserVersion
        {
            get => Connection.ExecuteScalar<int>(PragmaPrefix + "user_version");
            set => Connection.Execute(PragmaPrefix + $"user_version = { value.ToString() }");
        }

        public Task<int> UserVersionAsync() => Connection.ExecuteScalarAsync<int>(PragmaPrefix + "user_version");

        public Task UserVersionAsync(int userVersion) => Connection.ExecuteAsync(PragmaPrefix + $"user_version = { userVersion.ToString() }");

        public pragma_wal_checkpoint WalCheckpoint(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive)
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            var checkpointModeStr = checkpointMode.ToString().ToUpperInvariant();
            return Connection.Query<pragma_wal_checkpoint>(PragmaPrefix + "wal_checkpoint(" + checkpointModeStr + ")").Single();
        }

        public async Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive)
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            var checkpointModeStr = checkpointMode.ToString().ToUpperInvariant();
            var result = await Connection.QueryAsync<pragma_wal_checkpoint>(PragmaPrefix + "wal_checkpoint(" + checkpointModeStr + ")").ConfigureAwait(false);
            return result.Single();
        }
    }
}
