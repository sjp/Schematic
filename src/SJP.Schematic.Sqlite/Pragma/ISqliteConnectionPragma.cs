using System;
using System.Collections.Generic;
using System.Threading;
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
        bool LegacyAlterTable { get; set; }
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

        Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken));
        void CaseSensitiveLike(bool enable);
        Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<int> DataVersionAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        void IgnoreCheckConstraints(bool enable);
        Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> LegacyAlterTableAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task LegacyAlterTableAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> LegacyFileFormatAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task LegacyFileFormatAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        IEnumerable<string> Optimize(OptimizeFeatures features = OptimizeFeatures.Analyze);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
        void ShrinkMemory();
        Task ShrinkMemoryAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default(CancellationToken));
        Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> ThreadsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));
    }
}