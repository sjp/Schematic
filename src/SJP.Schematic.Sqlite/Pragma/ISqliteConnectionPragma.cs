using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    public interface ISqliteConnectionPragma
    {
        bool AutomaticIndex { get; set; }
        TimeSpan BusyTimeout { get; set; }
        bool CellSizeCheck { get; set; }
        bool CheckpointFullFsync { get; set; }
        IEnumerable<pragma_collation_list> CollationList { get; }
        IEnumerable<string> CompileOptions { get; }
        IEnumerable<pragma_database_list> DatabaseList { get; }
        IEnumerable<ISqliteDatabasePragma> DatabasePragmas { get; }
        int DataVersion { get; }
        bool DeferForeignKeys { get; set; }
        Encoding Encoding { get; set; }
        bool ForeignKeys { get; set; }
        bool FullFsync { get; set; }
        bool LegacyFileFormat { get; set; }
        bool QueryOnly { get; set; }
        bool ReadUncommitted { get; set; }
        bool RecursiveTriggers { get; set; }
        bool ReverseUnorderedSelects { get; set; }
        long SoftHeapLimit { get; set; }
        TemporaryStoreLocation TemporaryStore { get; set; }
        int Threads { get; set; }
        int WalAutoCheckpoint { get; set; }
        bool WritableSchema { get; set; }

        Task<bool> AutomaticIndexAsync();
        Task AutomaticIndexAsync(bool enable);
        Task<TimeSpan> BusyTimeoutAsync();
        Task BusyTimeoutAsync(TimeSpan timeout);
        void CaseSensitiveLike(bool enable);
        Task CaseSensitiveLikeAsync(bool enable);
        Task<bool> CellSizeCheckAsync();
        Task CellSizeCheckAsync(bool enable);
        Task<bool> CheckpointFullFsyncAsync();
        Task CheckpointFullFsyncAsync(bool enable);
        Task<IEnumerable<pragma_collation_list>> CollationListAsync();
        Task<IEnumerable<string>> CompileOptionsAsync();
        Task<IEnumerable<pragma_database_list>> DatabaseListAsync();
        Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync();
        Task<int> DataVersionAsync();
        Task<bool> DeferForeignKeysAsync();
        Task DeferForeignKeysAsync(bool enable);
        Task<Encoding> EncodingAsync();
        Task EncodingAsync(Encoding encoding);
        Task<bool> ForeignKeysAsync();
        Task ForeignKeysAsync(bool enable);
        Task<bool> FullFsyncAsync();
        Task FullFsyncAsync(bool enable);
        void IgnoreCheckConstraints(bool enable);
        Task IgnoreCheckConstraintsAsync(bool enable);
        Task<bool> LegacyFileFormatAsync();
        Task LegacyFileFormatAsync(bool enable);
        IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<bool> QueryOnlyAsync();
        Task QueryOnlyAsync(bool enable);
        Task<bool> ReadUncommittedAsync();
        Task ReadUncommittedAsync(bool enable);
        Task<bool> RecursiveTriggersAsync();
        Task RecursiveTriggersAsync(bool enable);
        Task<bool> ReverseUnorderedSelectsAsync();
        Task ReverseUnorderedSelectsAsync(bool enable);
        void ShrinkMemory();
        Task ShrinkMemoryAsync();
        Task<long> SoftHeapLimitAsync();
        Task SoftHeapLimitAsync(long heapLimit);
        Task<TemporaryStoreLocation> TemporaryStoreAsync();
        Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation);
        Task<int> ThreadsAsync();
        Task ThreadsAsync(int maxThreads);
        Task<int> WalAutoCheckpointAsync();
        Task WalAutoCheckpointAsync(int maxPages);
        Task<bool> WritableSchemaAsync();
        Task WritableSchemaAsync(bool enable);
    }
}