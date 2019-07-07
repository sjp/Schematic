using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    /// <summary>
    /// Contains asynchronous methods that provide access to schema-specific pragma functionality.
    /// </summary>
    public interface ISqliteDatabasePragma
    {
        string SchemaName { get; }

        Task<uint> ApplicationIdAsync(CancellationToken cancellationToken = default);
        Task ApplicationIdAsync(uint appId, CancellationToken cancellationToken = default);
        Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default);
        Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default);
        Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default);
        Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default);
        Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default);
        Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default);
        Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default);
        Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default);
        Task<int> DataVersionAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default);
        Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default);
        Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default);
        Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default);
        Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default);
        Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default);
        Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default);
        Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default);
        Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default);
        Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default);
        Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default);
        Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default);
        Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default);
        Task<ulong> PageCountAsync(CancellationToken cancellationToken = default);
        Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default);
        Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default);
        Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default);
        Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default);
        Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default);
        Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default);
        Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default);
        Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_table_xinfo>> TableXInfoAsync(Identifier tableName, CancellationToken cancellationToken = default);
        Task<int> UserVersionAsync(CancellationToken cancellationToken = default);
        Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default);
        Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default);
    }
}