using System.Collections.Generic;
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
        uint CacheSpillInPages { get; set; }
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

        Task<uint> ApplicationIdAsync();
        Task ApplicationIdAsync(uint appId);
        Task<AutoVacuumMode> AutoVacuumAsync();
        Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode);
        Task<ulong> CacheSizeInKibibytesAsync();
        Task CacheSizeInKibibytesAsync(ulong cacheSize);
        Task<ulong> CacheSizeInPagesAsync();
        Task CacheSizeInPagesAsync(ulong cacheSize);
        Task<bool> CacheSpillAsync();
        Task CacheSpillAsync(bool enable);
        Task<uint> CacheSpillInPagesAsync();
        Task CacheSpillInPagesAsync(uint pageSize);
        Task<int> DataVersionAsync();
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync();
        IEnumerable<pragma_foreign_key_check> ForeignKeyCheckTable(Identifier tableName);
        Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName);
        IEnumerable<pragma_foreign_key_list> ForeignKeyList(Identifier tableName);
        Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName);
        Task<ulong> FreeListCountAsync();
        void IncrementalVacuum(ulong pages = 0);
        Task IncrementalVacuumAsync(ulong pages = 0);
        IEnumerable<pragma_index_info> IndexInfo(string indexName);
        Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName);
        IEnumerable<pragma_index_list> IndexList(Identifier tableName);
        Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName);
        IEnumerable<pragma_index_xinfo> IndexXInfo(string indexName);
        Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName);
        IEnumerable<string> IntegrityCheck(uint maxErrors = 0);
        Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0);
        Task<JournalMode> JournalModeAsync();
        Task JournalModeAsync(JournalMode journalMode);
        Task<long> JournalSizeLimitAsync();
        Task JournalSizeLimitAsync(long sizeLimit);
        Task<LockingMode> LockingModeAsync();
        Task LockingModeAsync(LockingMode lockingMode);
        Task<ulong> MaxPageCountAsync();
        Task MaxPageCountAsync(ulong maxPageCount);
        Task<ulong> MmapSizeAsync();
        Task MmapSizeAsync(ulong mmapLimit);
        IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<ulong> PageCountAsync();
        Task<ushort> PageSizeAsync();
        Task PageSizeAsync(ushort pageSize);
        IEnumerable<string> QuickCheck(uint maxErrors = 0);
        Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0);
        Task<int> SchemaVersionAsync();
        Task SchemaVersionAsync(int schemaVersion);
        Task<SecureDeleteMode> SecureDeleteAsync();
        Task SecureDeleteAsync(SecureDeleteMode deleteMode);
        Task<SynchronousLevel> SynchronousAsync();
        Task SynchronousAsync(SynchronousLevel synchronousLevel);
        IEnumerable<pragma_table_info> TableInfo(Identifier tableName);
        Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName);
        Task<int> UserVersionAsync();
        Task UserVersionAsync(int userVersion);
        pragma_wal_checkpoint WalCheckpoint(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive);
        Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive);
    }
}