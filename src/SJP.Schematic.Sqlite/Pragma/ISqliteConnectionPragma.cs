using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    /// <summary>
    /// Contains asynchronous methods that provide access to connection-wide pragma functionality.
    /// </summary>
    public interface ISqliteConnectionPragma
    {
        /// <summary>
        /// Queries the limit of the approximate <c>ANALYZE</c> setting. This is approximate number of rows examined in each index by the <c>ANALYZE</c> command.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The approximate number of rows analysis is limited to when running <c>ANALYZE</c>. Zero indicates analyzing all rows.</returns>
        Task<uint> AnalysisLimitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Changes the limit of the approximate <c>ANALYZE</c> setting. This is approximate number of rows examined in each index by the <c>ANALYZE</c> command.
        /// </summary>
        /// <param name="rowLimit">The approximate number of rows to limit analysis to. Zero indicates analyzing all rows.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task AnalysisLimitAsync(uint rowLimit, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the automatic indexing capability.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if automatic indexing is enabled; otherwise <c>false</c>.</returns>
        Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the automatic indexing capability.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> enables automatic indexing.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the timeout used before a busy handler is invoked.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The timespan of the busy timeout.</returns>
        Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the timeout used before a busy handler is invoked.
        /// </summary>
        /// <param name="timeout">A timeout timespan.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets case sensitivity for <c>LIKE</c> expressions. When disabled, the default <c>LIKE</c> behavior is expressed. When enabled, case becomes significant.
        /// </summary>
        /// <param name="enable">If enabled, ensures case sensitivity for <c>LIKE</c> expressions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves whether cell size checking is enabled. When enabled, database corruption is detected earlier and is less likely to "spread". However, there is a small performance hit for doing the extra checks and so cell size checking is turned off by default.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if cell size checking is enabled; otherwise <c>false</c>.</returns>
        Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets whether cell size checking is enabled. When enabled, database corruption is detected earlier and is less likely to "spread". However, there is a small performance hit for doing the extra checks and so cell size checking is turned off by default.
        /// </summary>
        /// <param name="enable">If enabled, performs cell size checking.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query or change the fullfsync flag for checkpoint operations.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the fullfsync flag is enabled; otherwise <c>false</c>.</returns>
        Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Change the fullfsync flag for checkpoint operations.
        /// </summary>
        /// <param name="enable">If <c>true</c>, the flag must be enabled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return a list of the collating sequences defined for the current database connection
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of collation information.</returns>
        Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// This pragma returns the names of compile-time options used when building SQLite, one option per row.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of compile-time options.</returns>
        Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// This pragma works like a query to return one row for each database attached to the current database connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database information.</returns>
        Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves schema specific pragma accessors.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of schema specific pragma accessors.</returns>
        Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// The <c>PRAGMA data_version</c> command provides an indication that the database file has been modified.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A version number.</returns>
        Task<int> DataVersionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries whether <c>defer_foreign_keys</c> <c>PRAGMA</c> is on. When enabled, enforcement of all foreign key constraints is delayed until the outermost transaction is committed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if deferring of foreign key constraints is enabled; otherwise <c>false</c>.</returns>
        Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <c>defer_foreign_keys</c> <c>PRAGMA</c>. When enabled, enforcement of all foreign key constraints is delayed until the outermost transaction is committed.
        /// </summary>
        /// <param name="enable">If <c>true</c>, enabled deferring of foreign key enforcement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the text encoding used by the main database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The text encoding of the main database.</returns>
        Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the text encoding used by the main database.
        /// </summary>
        /// <param name="encoding">A text encoding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries whether foreign key constraints are enforced.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if foreign key constraints are enforced; otherwise <c>false</c>.</returns>
        Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables or disables enforcement of foreign key constraints.
        /// </summary>
        /// <param name="enable">If <c>true</c>, foreign key constraints should be enforced.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query the fullfsync flag.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the fullfsync flag is enabled; otherwise <c>false</c>.</returns>
        Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Change the fullfsync flag.
        /// </summary>
        /// <param name="enable">If <c>true</c>, sets the fullfsync flag to <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return a list of SQL functions known to the database connection
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of SQL functions.</returns>
        Task<IEnumerable<pragma_function_list>> FunctionListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables or disables the enforcement of <c>CHECK</c> constraints.
        /// </summary>
        /// <param name="enable">If <c>true</c>, disables enforcement of <c>CHECK</c> constraints.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the value of the <c>legacy_alter_table</c> flag.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the <c>ALTER TABLE</c> behaviour works as it did in v3.24.0 and earlier.</returns>
        Task<bool> LegacyAlterTableAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the value of the <c>legacy_alter_table</c> flag.
        /// </summary>
        /// <param name="enable">If set to <c>true</c> the <c>ALTER TABLE</c> behaviour will work as it did in v3.24.0 and earlier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task LegacyAlterTableAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of virtual table modules registered with the database connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of virtual table modules.</returns>
        Task<IEnumerable<string>> ModuleListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempt to optimize the database.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default);

        /// <summary>
        /// Return a list of <c>PRAGMA</c> commands known to the database connection.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of <c>PRAGMA</c> command names.</returns>
        Task<IEnumerable<string>> PragmaListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether the database can only be queried and not mutated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the database is read-only; otherwise <c>false</c>.</returns>
        Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Prevent all changes to database files when enabled, ensuring only queries are enabled.
        /// </summary>
        /// <param name="enable">If <c>true</c>, only read-only queries may be run.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query the <c>READ UNCOMMITTED</c> transaction isolation level. The default isolation level for SQLite is <c>SERIALIZABLE</c>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the isolation level is <c>READ UNCOMMITTED</c>.</returns>
        Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the <c>READ UNCOMMITTED</c> transaction isolation level. The default isolation level for SQLite is <c>SERIALIZABLE</c>.
        /// </summary>
        /// <param name="enable">If <c>true</c>, sets the isolation level to <c>READ UNCOMMITTED</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query the recursive trigger capability.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if recursive triggers are enabled; otherwise <c>false</c>.</returns>
        Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Set, or clear the recursive trigger capability.
        /// </summary>
        /// <param name="enable">If set to <c>true</c> recursive triggers are enabled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries whether statements missing an <c>ORDER BY</c> emit results in reverse order.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>If <c>true</c>, unordered <c>SELECT</c> queries are returned in reverse order; otherwise <c>false</c>.</returns>
        Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets whether statements missing an <c>ORDER BY</c> emit results in reverse order.
        /// </summary>
        /// <param name="enable">If <c>true</c>, unordered <c>SELECT</c> queries are returned in reverse order.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Causes the database connection on which it is invoked to free up as much memory as it can.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task ShrinkMemoryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the size of the heap used by all database connections within a single process.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The size of the heap limit.</returns>
        Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the size of the heap used by all database connections within a single process.
        /// </summary>
        /// <param name="heapLimit">The heap limit.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns information about each table or view in all schemas.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of table or view information, one element for each table or view in all schemas.</returns>
        Task<IEnumerable<pragma_table_list>> TableListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns information about a given table or view in the given schema.
        /// </summary>
        /// <param name="tableName">A table or view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Information relevant to the given table or view. Will be empty if the table or view does not exist.</returns>
        Task<IEnumerable<pragma_table_list>> TableListAsync(Identifier tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries where temporary storage is located.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The location of the temporary store.</returns>
        Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the temporary storage location.
        /// </summary>
        /// <param name="tempLocation">The temporary location.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default);

        /// <summary>
        /// This limit sets an upper bound on the number of auxiliary threads that a prepared statement is allowed to launch to assist with a query.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The upper bound on the number of threads that are able to be used.</returns>
        Task<int> ThreadsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets an upper bound on the number of auxiliary threads that a prepared statement is allowed to launch to assist with a query.
        /// </summary>
        /// <param name="maxThreads">The maximum threads.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the write-ahead log auto-checkpoint interval.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum number of pages in the write-ahead log before checkpointing.</returns>
        Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the write-ahead log auto-checkpoint interval.
        /// </summary>
        /// <param name="maxPages">The maximum number of pages before a checkpoint.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries whether the <c>sqlite_master</c> table can be changed using ordinary <c>UPDATE</c>, <c>INSERT</c>, and <c>DELETE</c> statements.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the <c>sqlite_master</c> is able to be modified directly; otherwise <c>false</c>.</returns>
        Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets whether the <c>sqlite_master</c> table can be changed using ordinary <c>UPDATE</c>, <c>INSERT</c>, and <c>DELETE</c> statements.
        /// </summary>
        /// <param name="enable">If <c>true</c> enables the <c>sqlite_master</c> to be modified directly.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default);
    }
}