using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    public interface ISqliteDatabasePragma
    {
        uint ApplicationId { get; set; }
        AutoVacuumMode AutoVacuum { get; set; }
        ulong CacheSizeInKibibytes { get; set; }
        ulong CacheSizeInPages { get; set; }
        bool CacheSpill { get; set; }
        int DataVersion { get; }
        IEnumerable<pragma_foreign_key_check> ForeignKeyCheckDatabase { get; }
        ulong FreeListCount { get; }
        JournalMode JournalMode { get; set; }
        long JournalSizeLimit { get; set; }
        LockingMode LockingMode { get; set; }
        ulong MaxPageCount { get; set; }
        ulong MmapSize { get; set; }
        ulong PageCount { get; }
        ushort PageSize { get; set; }
        string SchemaName { get; }
        int SchemaVersion { get; set; }
        SecureDeleteMode SecureDelete { get; set; }
        SynchronousLevel Synchronous { get; set; }
        int UserVersion { get; set; }

        Task<uint> ApplicationIdAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ApplicationIdAsync(uint appId, CancellationToken cancellationToken = default(CancellationToken));
        Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> DataVersionAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_foreign_key_check> ForeignKeyCheckTable(Identifier tableName);
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_foreign_key_list> ForeignKeyList(Identifier tableName);
        Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default(CancellationToken));
        void IncrementalVacuum(ulong pages = 0);
        Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_index_info> IndexInfo(string indexName);
        Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_index_list> IndexList(Identifier tableName);
        Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_index_xinfo> IndexXInfo(string indexName);
        Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<string> IntegrityCheck(uint maxErrors = 0);
        Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken));
        Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default(CancellationToken));
        Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default(CancellationToken));
        Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default(CancellationToken));
        Task<ulong> PageCountAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<string> QuickCheck(uint maxErrors = 0);
        Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default(CancellationToken));
        Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default(CancellationToken));
        Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<pragma_table_info> TableInfo(Identifier tableName);
        Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> UserVersionAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default(CancellationToken));
        pragma_wal_checkpoint WalCheckpoint(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive);
        Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default(CancellationToken));
    }
}