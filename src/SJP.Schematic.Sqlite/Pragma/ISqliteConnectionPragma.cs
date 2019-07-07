using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    /// <summary>
    /// Contains asynchronous methods that provide access to connection-wide pragma functionality.
    /// </summary>
    public interface ISqliteConnectionPragma
    {
        Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default);
        Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default);
        Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default);
        Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
        Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default);
        Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default);
        Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default);
        Task<int> DataVersionAsync(CancellationToken cancellationToken = default);
        Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default);
        Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default);
        Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default);
        Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default);
        Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default);
        Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default);
        Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default);
        Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> LegacyAlterTableAsync(CancellationToken cancellationToken = default);
        Task LegacyAlterTableAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> LegacyFileFormatAsync(CancellationToken cancellationToken = default);
        Task LegacyFileFormatAsync(bool enable, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default);
        Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default);
        Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default);
        Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default);
        Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default);
        Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default);
        Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default);
        Task ShrinkMemoryAsync(CancellationToken cancellationToken = default);
        Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default);
        Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default);
        Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default);
        Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default);
        Task<int> ThreadsAsync(CancellationToken cancellationToken = default);
        Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default);
        Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default);
        Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default);
        Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default);
        Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default);
    }
}