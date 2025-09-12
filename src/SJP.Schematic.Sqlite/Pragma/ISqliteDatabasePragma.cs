using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Contains asynchronous methods that provide access to schema-specific pragma functionality.
/// </summary>
public interface ISqliteDatabasePragma
{
    /// <summary>
    /// The name of the database or schema that this instance accesses and modifies.
    /// </summary>
    /// <value>A database name.
    /// </value>
    string SchemaName { get; }

    /// <summary>
    /// Retrieves the application ID of the given database. An application ID is an integer stored in the file header to enable file type identification.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer, representing an application ID</returns>
    Task<int> ApplicationIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the application ID of the given database. An application ID is an integer stored in the file header to enable file type identification.
    /// </summary>
    /// <param name="appId">The application identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task ApplicationIdAsync(int appId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the auto-vacuum status in the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current mode of auto-vacuum in the database.</returns>
    Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the auto-vacuum status in the database.
    /// </summary>
    /// <param name="autoVacuumMode">The automatic vacuum mode.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The size of the cache, in kibibytes.</returns>
    Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
    /// </summary>
    /// <param name="cacheSize">Size of the cache (in kibibytes).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The size of the cache, in pages.</returns>
    Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
    /// </summary>
    /// <param name="cacheSize">Size of the cache (in pages).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the ability of the pager to spill dirty cache pages to the database file in the middle of a transaction.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> if the dirty cache pages are able to be spilled, otherwise <see langword="false" />.</returns>
    Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the ability of the pager to spill dirty cache pages to the database file in the middle of a transaction.
    /// </summary>
    /// <param name="enable">if set to <see langword="true" /> dirty cache pages are able to be spilled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides an indication that the database file has been modified. A database change will cause <c>data_version</c> to be updated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer indicating whether the database has been changed.</returns>
    Task<int> DataVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the database, for foreign key constraints that are violated and returns one row of output for each violation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign key violation information.</returns>
    Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks a database table, for foreign key constraints that are violated and returns one row of output for each violation.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign key violation information.</returns>
    Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries for foreign keys defined in a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign key definitions.</returns>
    Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of unused pages in the database file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer representing the number of unused pages.</returns>
    Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Frees pages from the freelist. Given a number of pages, no more than the given number of pages will be freed.
    /// </summary>
    /// <param name="pages">The maximum number of pages to be freed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns index information for a given index.
    /// </summary>
    /// <param name="indexName">An index name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One element for each key column in the named index.</returns>
    Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns indexes applied to a given table..
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One element for each index associated with the given table.</returns>
    Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns index information for a given index.
    /// </summary>
    /// <param name="indexName">An index name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One element for each column in the named index, not just key columns.</returns>
    Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Integrity check of the entire database. The <c>integrity_check</c> pragma looks for out-of-order records, missing pages, malformed records, missing index entries, and <c>UNIQUE</c>, <c>CHECK</c>, and <c>NOT NULL</c> constraint errors.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of informative error messages describing integrity failures.</returns>
    Task<IEnumerable<string>> IntegrityCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Integrity check of the entire database up to a given number of errors. The <c>integrity_check</c> pragma looks for out-of-order records, missing pages, malformed records, missing index entries, and <c>UNIQUE</c>, <c>CHECK</c>, and <c>NOT NULL</c> constraint errors.
    /// </summary>
    /// <param name="maxErrors">The maximum errors.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of informative error messages describing integrity failures.</returns>
    Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors, CancellationToken cancellationToken = default);

    /// <summary>
    /// Integrity check of a given table. The <c>integrity_check</c> pragma looks for out-of-order records, missing pages, malformed records, missing index entries, and <c>UNIQUE</c>, <c>CHECK</c>, and <c>NOT NULL</c> constraint errors.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of informative error messages describing integrity failures.</returns>
    Task<IEnumerable<string>> IntegrityCheckAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the journal mode for the current database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current journal mode.</returns>
    Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the journal mode.
    /// </summary>
    /// <param name="journalMode">A journal mode.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the maximum size of the journal.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The maximum size of the journal, in bytes.</returns>
    Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the maximum size of the journal.
    /// </summary>
    /// <param name="sizeLimit">The maximum size of the journal, in bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the database connection locking-mode. The locking-mode is either <c>NORMAL</c> or <c>EXCLUSIVE</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The database locking mode.</returns>
    Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the database connection locking-mode. The locking-mode is either <c>NORMAL</c> or <c>EXCLUSIVE</c>.
    /// </summary>
    /// <param name="lockingMode">The locking mode.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the maximum number of pages in the database file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer, representing the maximum number of pages.</returns>
    Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the maximum number of pages in the database file.
    /// </summary>
    /// <param name="maxPageCount">The maximum page count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the maximum number of bytes that are set aside for memory-mapped I/O on a single database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The maximum number of bytes that are set aside for memory-mapped I/O.</returns>
    Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Change the maximum number of bytes that are set aside for memory-mapped I/O on a single database.
    /// </summary>
    /// <param name="mmapLimit">The mmap limit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempt to optimize the database.
    /// </summary>
    /// <param name="features">The set of features to optimize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of messages returned from the optimization process.</returns>
    Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return the total number of pages in the database file.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of pages in the database.</returns>
    Task<ulong> PageCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the page size of the database. The page size must be a power of two between 512 and 65536 inclusive.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page size in bytes.</returns>
    Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the page size of the database. The page size must be a power of two between 512 and 65536 inclusive.
    /// </summary>
    /// <param name="pageSize">The page size to set in bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// The pragma is like <see cref="IntegrityCheckAsync(uint, CancellationToken)"/> except that it does not verify <c>UNIQUE</c> constraints and does not verify that index content matches table content.
    /// </summary>
    /// <param name="maxErrors">The maximum error count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of integrity check failures.</returns>
    Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the schema version. Used to determine whether the schema has changed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer that represents a schema version.</returns>
    Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the schema version. Used to determine whether the schema has changed.
    /// </summary>
    /// <param name="schemaVersion">A schema version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query or change the secure-delete setting. When secure delete is on, SQLite overwrites deleted content with zeros.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The secure delete mode.</returns>
    Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Change the secure-delete setting. When secure delete is on, SQLite overwrites deleted content with zeros.
    /// </summary>
    /// <param name="deleteMode">The secure delete mode.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query the setting of the "synchronous" flag, which determines how much of the file operations require synchronization.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A value which determines the behaviour of file operations.</returns>
    Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the "synchronous" flag, which determines how much of the file operations require synchronization.
    /// </summary>
    /// <param name="synchronousLevel">The synchronous level.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns information about each column in a database table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of column information, one element for each column in the table.</returns>
    Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns information about each table or view in the current schema.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table or view information, one element for each table or view in the schema.</returns>
    Task<IEnumerable<pragma_table_list>> TableListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns information about a given table or view in the given schema.
    /// </summary>
    /// <param name="tableName">A table or view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Information relevant to the given table or view. Will be empty if the table or view does not exist.</returns>
    Task<IEnumerable<pragma_table_list>> TableListAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns information about each column, and hidden columns in a database table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of column information, one element for each column or hidden column in the table.</returns>
    Task<IEnumerable<pragma_table_xinfo>> TableXInfoAsync(Identifier tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the user-version, an integer that is available to applications to use however they want.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An integer representing a user-defined version of the database.</returns>
    Task<int> UserVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the user-version, an integer that is available to applications to use however they want.
    /// </summary>
    /// <param name="userVersion">The user-defined database version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Causes a checkpoint operation to run on the database.
    /// </summary>
    /// <param name="checkpointMode">The checkpoint mode.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Information on the status of the checkpoint operation once completed.</returns>
    Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default);
}