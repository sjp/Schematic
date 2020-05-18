using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite.Pragma
{
    /// <summary>
    /// A <c>PRAGMA</c> accessor for particular SQLite schema.
    /// </summary>
    /// <seealso cref="ISqliteDatabasePragma" />
    public class DatabasePragma : ISqliteDatabasePragma
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePragma"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="schemaName">A schema  name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>. If <paramref name="schemaName"/> is <c>null</c>, empty or whitespace.</exception>
        public DatabasePragma(ISchematicConnection connection, string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            SchemaName = schemaName;
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            PragmaPrefix = "PRAGMA " + connection.Dialect.QuoteIdentifier(schemaName) + ".";
        }

        /// <summary>
        /// The name of the schema that the <c>PRAGMA</c> relates to.
        /// </summary>
        /// <value>A schema name.</value>
        public string SchemaName { get; }

        /// <summary>
        /// A database connection specific to Schematic.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// A database connection factory used to query the database.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// The dialect for the associated database.
        /// </summary>
        /// <value>A database dialect.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// A prefix used to create queries to access <c>PRAGMA</c> information.
        /// </summary>
        /// <value>A prefix for <c>PRAGMA</c> queries.</value>
        protected string PragmaPrefix { get; }

        /// <summary>
        /// Retrieves the application ID of the given database. An application ID is an integer stored in the file header to enable file type identification.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer, representing an application ID.</returns>
        public Task<int> ApplicationIdAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(ApplicationIdReadQuery, cancellationToken);

        /// <summary>
        /// Sets the application ID of the given database. An application ID is an integer stored in the file header to enable file type identification.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task ApplicationIdAsync(int appId, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ApplicationIdSetQuery(appId), cancellationToken);

        /// <summary>
        /// Gets a query to read the user version pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ApplicationIdReadQuery => PragmaPrefix + "application_id";

        /// <summary>
        /// Creates a query to set the user version pragma.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string ApplicationIdSetQuery(int appId) => PragmaPrefix + "application_id = " + appId.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Query the auto-vacuum status in the database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The current mode of auto-vacuum in the database.</returns>
        public Task<AutoVacuumMode> AutoVacuumAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<AutoVacuumMode>(AutoVacuumReadQuery, cancellationToken);

        /// <summary>
        /// Set the auto-vacuum status in the database.
        /// </summary>
        /// <param name="autoVacuumMode">The automatic vacuum mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task AutoVacuumAsync(AutoVacuumMode autoVacuumMode, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(AutoVacuumSetQuery(autoVacuumMode), cancellationToken);

        /// <summary>
        /// Gets a query to read the auto vacuum pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AutoVacuumReadQuery => PragmaPrefix + "auto_vacuum";

        /// <summary>
        /// Creates a query to set the auto vacuum pragma.
        /// </summary>
        /// <param name="autoVacuumMode">The automatic vacuum mode.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="autoVacuumMode"/> is an invalid enum value.</exception>
        protected virtual string AutoVacuumSetQuery(AutoVacuumMode autoVacuumMode)
        {
            if (!autoVacuumMode.IsValid())
                throw new ArgumentException($"The { nameof(AutoVacuumMode) } provided must be a valid enum.", nameof(autoVacuumMode));

            var value = autoVacuumMode.ToString().ToLowerInvariant();
            return PragmaPrefix + "auto_vacuum = " + value;
        }

        /// <summary>
        /// Query the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The size of the cache, in pages.</returns>
        public async Task<ulong> CacheSizeInPagesAsync(CancellationToken cancellationToken = default)
        {
            var size = await DbConnection.ExecuteScalarAsync<long>(CacheSizeInPagesReadQuery, cancellationToken).ConfigureAwait(false);
            if (size < 0)
            {
                var pageSize = await PageSizeAsync(cancellationToken).ConfigureAwait(false);
                size /= -pageSize / 1024;
            }

            return (ulong)size;
        }

        /// <summary>
        /// Sets the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
        /// </summary>
        /// <param name="cacheSize">Size of the cache (in pages).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task CacheSizeInPagesAsync(ulong cacheSize, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CacheSizeInPagesSetQuery(cacheSize), cancellationToken);

        /// <summary>
        /// Gets a query to read the cache size pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string CacheSizeInPagesReadQuery => PragmaPrefix + "cache_size";

        /// <summary>
        /// Creates a query to set the cache size pragma.
        /// </summary>
        /// <param name="cacheSize">Size of the cache (in pages).</param>
        /// <returns>A SQL query.</returns>
        protected virtual string CacheSizeInPagesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = " + cacheSize.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Query the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The size of the cache, in kibibytes.</returns>
        public async Task<ulong> CacheSizeInKibibytesAsync(CancellationToken cancellationToken = default)
        {
            var size = await DbConnection.ExecuteScalarAsync<long>(CacheSizeInKibibytesReadQuery, cancellationToken).ConfigureAwait(false);
            if (size > 0)
                size *= await PageSizeAsync(cancellationToken).ConfigureAwait(false) / 1024;
            else
                size *= -1;

            return (ulong)size;
        }

        /// <summary>
        /// Sets the suggested maximum number of database disk pages that SQLite will hold in memory at once per open database file.
        /// </summary>
        /// <param name="cacheSize">Size of the cache (in kibibytes).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task CacheSizeInKibibytesAsync(ulong cacheSize, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CacheSizeInKibibytesSetQuery(cacheSize), cancellationToken);

        /// <summary>
        /// Gets a query to read the cache size pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string CacheSizeInKibibytesReadQuery => PragmaPrefix + "cache_size";

        /// <summary>
        /// Creates a query to set the cache size pragma.
        /// </summary>
        /// <param name="cacheSize">Size of the cache (in kibibytes).</param>
        /// <returns>A SQL query.</returns>
        protected virtual string CacheSizeInKibibytesSetQuery(ulong cacheSize) => PragmaPrefix + "cache_size = -" + cacheSize.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Queries the ability of the pager to spill dirty cache pages to the database file in the middle of a transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if the dirty cache pages are able to be spilled, otherwise <c>false</c>.</returns>
        public Task<bool> CacheSpillAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(CacheSpillReadQuery, cancellationToken);

        /// <summary>
        /// Sets the ability of the pager to spill dirty cache pages to the database file in the middle of a transaction.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> dirty cache pages are able to be spilled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task CacheSpillAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CacheSpillSetQuery(enable), cancellationToken);

        /// <summary>
        /// Gets a query to read the cache spill pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string CacheSpillReadQuery => PragmaPrefix + "cache_spill";

        /// <summary>
        /// Creates a query to set the cache spill pragma.
        /// </summary>
        /// <param name="enable">if set to <c>true</c> dirty cache pages are able to be spilled.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string CacheSpillSetQuery(bool enable) => PragmaPrefix + "cache_spill = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Provides an indication that the database file has been modified. A database change will cause <c>data_version</c> to be updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer indicating whether the database has been changed.</returns>
        public Task<int> DataVersionAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(DataVersionQuery, cancellationToken);

        /// <summary>
        /// Gets a query to read the data version pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DataVersionQuery => PragmaPrefix + "data_version";

        /// <summary>
        /// Checks the database, for foreign key constraints that are violated and returns one row of output for each violation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign key violation information.</returns>
        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckDatabaseAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckDatabaseQuery, cancellationToken);

        /// <summary>
        /// Gets a query to read the foreign key check pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ForeignKeyCheckDatabaseQuery => PragmaPrefix + "foreign_key_check";

        /// <summary>
        /// Checks a database table, for foreign key constraints that are violated and returns one row of output for each violation.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign key violation information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        public Task<IEnumerable<pragma_foreign_key_check>> ForeignKeyCheckTableAsync(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return DbConnection.QueryAsync<pragma_foreign_key_check>(ForeignKeyCheckTableQuery(tableName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the foreign key check pragma for a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        protected virtual string ForeignKeyCheckTableQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_check(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        /// <summary>
        /// Queries for foreign keys defined in a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign key definitions.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        public Task<IEnumerable<pragma_foreign_key_list>> ForeignKeyListAsync(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return DbConnection.QueryAsync<pragma_foreign_key_list>(ForeignKeyListQuery(tableName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the foreign key list pragma.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        protected virtual string ForeignKeyListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "foreign_key_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        /// <summary>
        /// Returns the number of unused pages in the database file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer representing the number of unused pages.</returns>
        public Task<ulong> FreeListCountAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<ulong>(FreeListCountQuery, cancellationToken);

        /// <summary>
        /// Gets a query to read the freelist count pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string FreeListCountQuery => PragmaPrefix + "freelist_count";

        /// <summary>
        /// Frees pages from the freelist. Given a number of pages, no more than the given number of pages will be freed.
        /// </summary>
        /// <param name="pages">The maximum number of pages to be freed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task IncrementalVacuumAsync(ulong pages = 0, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(IncrementalVacuumQuery(pages), cancellationToken);

        /// <summary>
        /// Gets a query to read the incremental vacuum pragma.
        /// </summary>
        /// <param name="pages">The maximum number of pages to be freed.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string IncrementalVacuumQuery(ulong pages)
        {
            return pages < 1
                ? PragmaPrefix + "incremental_vacuum"
                : PragmaPrefix + "incremental_vacuum = " + pages.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns index information for a given index.
        /// </summary>
        /// <param name="indexName">An index name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>One element for each key column in the named index.</returns>
        public Task<IEnumerable<pragma_index_info>> IndexInfoAsync(string indexName, CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_index_info>(IndexInfoQuery(indexName), cancellationToken);

        /// <summary>
        /// Gets a query to read the index info pragma.
        /// </summary>
        /// <param name="indexName">An index name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="indexName"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual string IndexInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_info(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        /// <summary>
        /// Returns indexes applied to a given table..
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>One element for each index associated with the given table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        public Task<IEnumerable<pragma_index_list>> IndexListAsync(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return DbConnection.QueryAsync<pragma_index_list>(IndexListQuery(tableName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the index list pragma.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        protected virtual string IndexListQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "index_list(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        /// <summary>
        /// Returns index information for a given index.
        /// </summary>
        /// <param name="indexName">An index name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>One element for each column in the named index, not just key columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="indexName"/> is <c>null</c>, empty or whitespace.</exception>
        public Task<IEnumerable<pragma_index_xinfo>> IndexXInfoAsync(string indexName, CancellationToken cancellationToken = default)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return DbConnection.QueryAsync<pragma_index_xinfo>(IndexXInfoQuery(indexName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the index extra info pragma.
        /// </summary>
        /// <param name="indexName">An index name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="indexName"/> is <c>null</c>, empty or whitespace.</exception>
        protected virtual string IndexXInfoQuery(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));

            return PragmaPrefix + "index_xinfo(" + Dialect.QuoteIdentifier(indexName) + ")";
        }

        /// <summary>
        /// Integrity check of the entire database. The <c>integrity_check</c> pragma looks for out-of-order records, missing pages, malformed records, missing index entries, and <c>UNIQUE</c>, <c>CHECK</c>, and <c>NOT NULL</c> constraint errors.
        /// </summary>
        /// <param name="maxErrors">The maximum error count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of informative error messages describing integrity failures.</returns>
        public async Task<IEnumerable<string>> IntegrityCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default)
        {
            var sql = IntegrityCheckQuery(maxErrors);
            var errors = await DbConnection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        /// <summary>
        /// Gets a query to read the integrity check pragma.
        /// </summary>
        /// <param name="maxErrors">The maximum error count.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string IntegrityCheckQuery(uint maxErrors)
        {
            return maxErrors == 0
                ? PragmaPrefix + "integrity_check"
                : PragmaPrefix + "integrity_check(" + maxErrors.ToString(CultureInfo.InvariantCulture) + ")";
        }

        /// <summary>
        /// Queries the journal mode for the current database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The current journal mode.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unknown and unsupported journal mode is returned.</exception>
        public async Task<JournalMode> JournalModeAsync(CancellationToken cancellationToken = default)
        {
            var journalModeName = await DbConnection.ExecuteScalarAsync<string>(JournalModeReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enum.TryParse(journalModeName, true, out JournalMode journalMode))
                throw new InvalidOperationException("Unknown and unsupported journal mode found: " + journalModeName);

            return journalMode;
        }

        /// <summary>
        /// Sets the journal mode.
        /// </summary>
        /// <param name="journalMode">A journal mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task JournalModeAsync(JournalMode journalMode, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(JournalModeSetQuery(journalMode), cancellationToken);

        /// <summary>
        /// Gets a query to read the journal mode pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string JournalModeReadQuery => PragmaPrefix + "journal_mode";

        /// <summary>
        /// Creates a query to set the journal mode pragma.
        /// </summary>
        /// <param name="journalMode">The journal mode.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="journalMode"/> has an invalid enum value.</exception>
        protected virtual string JournalModeSetQuery(JournalMode journalMode)
        {
            if (!journalMode.IsValid())
                throw new ArgumentException($"The { nameof(JournalMode) } provided must be a valid enum.", nameof(journalMode));

            var value = journalMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "journal_mode = " + value;
        }

        /// <summary>
        /// Queries the maximum size of the journal.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum size of the journal, in bytes.</returns>
        public Task<long> JournalSizeLimitAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<long>(JournalSizeLimitReadQuery, cancellationToken);

        /// <summary>
        /// Sets the maximum size of the journal.
        /// </summary>
        /// <param name="sizeLimit">The maximum size of the journal, in bytes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task JournalSizeLimitAsync(long sizeLimit, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(JournalSizeLimitSetQuery(sizeLimit), cancellationToken);

        /// <summary>
        /// Gets a query to read the journal size limit pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string JournalSizeLimitReadQuery => PragmaPrefix + "journal_size_limit";

        /// <summary>
        /// Creates a query to set the journal size limit pragma.
        /// </summary>
        /// <param name="sizeLimit">The maximum size of the journal, in bytes.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string JournalSizeLimitSetQuery(long sizeLimit) => PragmaPrefix + "journal_size_limit = " + sizeLimit.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Queries the database connection locking-mode. The locking-mode is either <c>NORMAL</c> or <c>EXCLUSIVE</c>.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The database locking mode.</returns>
        /// <exception cref="InvalidOperationException">An unknown and unsupported locking mode was returned.</exception>
        public async Task<LockingMode> LockingModeAsync(CancellationToken cancellationToken = default)
        {
            var lockingModeName = await DbConnection.ExecuteScalarAsync<string>(LockingModeReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enum.TryParse(lockingModeName, true, out LockingMode lockingMode))
                throw new InvalidOperationException("Unknown and unsupported locking mode found: " + lockingModeName);

            return lockingMode;
        }

        /// <summary>
        /// Sets the database connection locking-mode. The locking-mode is either <c>NORMAL</c> or <c>EXCLUSIVE</c>.
        /// </summary>
        /// <param name="lockingMode">The locking mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task LockingModeAsync(LockingMode lockingMode, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(LockingModeSetQuery(lockingMode), cancellationToken);

        /// <summary>
        /// Gets a query to read the locking mode pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string LockingModeReadQuery => PragmaPrefix + "locking_mode";

        /// <summary>
        /// Creates a query to set the locking mode pragma.
        /// </summary>
        /// <param name="lockingMode">The locking mode.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="lockingMode"/> is an invalid enum value.</exception>
        protected virtual string LockingModeSetQuery(LockingMode lockingMode)
        {
            if (!lockingMode.IsValid())
                throw new ArgumentException($"The { nameof(LockingMode) } provided must be a valid enum.", nameof(lockingMode));

            var value = lockingMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "locking_mode = " + value;
        }

        /// <summary>
        /// Query the maximum number of pages in the database file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer, representing the maximum number of pages.</returns>
        public Task<ulong> MaxPageCountAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<ulong>(MaxPageCountReadQuery, cancellationToken);

        /// <summary>
        /// Set the maximum number of pages in the database file.
        /// </summary>
        /// <param name="maxPageCount">The maximum page count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task MaxPageCountAsync(ulong maxPageCount, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(MaxPageCountSetQuery(maxPageCount), cancellationToken);

        /// <summary>
        /// Gets a query to read the max page count pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string MaxPageCountReadQuery => PragmaPrefix + "max_page_count";

        /// <summary>
        /// Creates a query to set the max page count pragma.
        /// </summary>
        /// <param name="maxPageCount">The maximum page count.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string MaxPageCountSetQuery(ulong maxPageCount) => PragmaPrefix + "max_page_count = " + maxPageCount.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Query the maximum number of bytes that are set aside for memory-mapped I/O on a single database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum number of bytes that are set aside for memory-mapped I/O.</returns>
        public Task<ulong> MmapSizeAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<ulong>(MmapSizeReadQuery, cancellationToken);

        /// <summary>
        /// Change the maximum number of bytes that are set aside for memory-mapped I/O on a single database.
        /// </summary>
        /// <param name="mmapLimit">The mmap limit.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task MmapSizeAsync(ulong mmapLimit, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(MmapSizeSetQuery(mmapLimit), cancellationToken);

        /// <summary>
        /// Gets a query to read the mmap size read query pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string MmapSizeReadQuery => PragmaPrefix + "mmap_size";

        /// <summary>
        /// Creates a query to set the mmap size pragma.
        /// </summary>
        /// <param name="mmapLimit">The mmap limit.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string MmapSizeSetQuery(ulong mmapLimit) => PragmaPrefix + "mmap_size = " + mmapLimit.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Attempt to optimize the database.
        /// </summary>
        /// <param name="features">The set of features to optimize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of messages returned from the optimization process.</returns>
        public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default) => DbConnection.QueryAsync<string>(OptimizeSetQuery(features), cancellationToken);

        /// <summary>
        /// Generates a query that attempts to optimize the database.
        /// </summary>
        /// <param name="features">The set of features to optimize.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="features"/> is an invalid enum value.</exception>
        protected virtual string OptimizeSetQuery(OptimizeFeatures features)
        {
            if (!features.IsValid())
                throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

            var value = (int)features;
            return PragmaPrefix + "optimize = " + value;
        }

        /// <summary>
        /// Return the total number of pages in the database file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of pages in the database.</returns>
        public Task<ulong> PageCountAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<ulong>(PageCountReadQuery, cancellationToken);

        /// <summary>
        /// Gets a query to read the page count pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PageCountReadQuery => PragmaPrefix + "page_count";

        /// <summary>
        /// Query the page size of the database. The page size must be a power of two between 512 and 65536 inclusive.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The page size in bytes.</returns>
        public Task<ushort> PageSizeAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<ushort>(PageSizeReadQuery, cancellationToken);

        /// <summary>
        /// Set the page size of the database. The page size must be a power of two between 512 and 65536 inclusive.
        /// </summary>
        /// <param name="pageSize">The page size to set in bytes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task PageSizeAsync(ushort pageSize, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(PageSizeSetQuery(pageSize), cancellationToken);

        /// <summary>
        /// Gets a query to read the page size pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PageSizeReadQuery => PragmaPrefix + "page_size";

        /// <summary>
        /// Creates a query to set the page size pragma.
        /// </summary>
        /// <param name="pageSize">The page size to set in bytes.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException">An invalid <paramref name="pageSize"/> value was provided. Must be a power of two, that is also at least 512.</exception>
        protected virtual string PageSizeSetQuery(ushort pageSize)
        {
            if (pageSize < 512)
                throw new ArgumentException("A page size must be a power of two whose value is at least 512. Given: " + pageSize.ToString(CultureInfo.InvariantCulture), nameof(pageSize));
            if (!IsPowerOfTwo(pageSize))
                throw new ArgumentException("A page size must be a power of two. Given: " + pageSize.ToString(CultureInfo.InvariantCulture), nameof(pageSize));

            return PragmaPrefix + "page_size = " + pageSize.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether a given value is a power of two.
        /// </summary>
        /// <param name="value">A number.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is a power of two; otherwise, <c>false</c>.</returns>
        protected static bool IsPowerOfTwo(ulong value) => value != 0 && (value & (value - 1)) == 0;

        /// <summary>
        /// The pragma is like <see cref="IntegrityCheckAsync(uint, CancellationToken)" /> except that it does not verify <c>UNIQUE</c> constraints and does not verify that index content matches table content.
        /// </summary>
        /// <param name="maxErrors">The maximum error count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of integrity check failures.</returns>
        public async Task<IEnumerable<string>> QuickCheckAsync(uint maxErrors = 0, CancellationToken cancellationToken = default)
        {
            var sql = QuickCheckQuery(maxErrors);
            var errors = await DbConnection.QueryAsync<string>(sql, cancellationToken).ConfigureAwait(false);
            var result = errors.ToList();
            if (result.Count == 1 && result[0] == "ok")
                return Array.Empty<string>();

            return result;
        }

        /// <summary>
        /// Gets a query to read the quick check pragma.
        /// </summary>
        /// <param name="maxErrors">The maximum error count.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string QuickCheckQuery(uint maxErrors)
        {
            return maxErrors == 0
                ? PragmaPrefix + "quick_check"
                : PragmaPrefix + "quick_check(" + maxErrors.ToString(CultureInfo.InvariantCulture) + ")";
        }

        /// <summary>
        /// Query the schema version. Used to determine whether the schema has changed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer that represents a schema version.</returns>
        public Task<int> SchemaVersionAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(SchemaVersionReadQuery, cancellationToken);

        /// <summary>
        /// Sets the schema version. Used to determine whether the schema has changed.
        /// </summary>
        /// <param name="schemaVersion">A schema version.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task SchemaVersionAsync(int schemaVersion, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(SchemaVersionSetQuery(schemaVersion), cancellationToken);

        /// <summary>
        /// Gets a query to read the schema version pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SchemaVersionReadQuery => PragmaPrefix + "schema_version";

        /// <summary>
        /// Creates a query to read the schema version pragma.
        /// </summary>
        /// <param name="schemaVersion">A schema version.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string SchemaVersionSetQuery(int schemaVersion) => PragmaPrefix + "schema_version = " + schemaVersion.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Query or change the secure-delete setting. When secure delete is on, SQLite overwrites deleted content with zeros.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The secure delete mode.</returns>
        /// <exception cref="InvalidOperationException">An invalid or unsupported <see cref="SecureDeleteMode"/> was returned from the database.</exception>
        public async Task<SecureDeleteMode> SecureDeleteAsync(CancellationToken cancellationToken = default)
        {
            var secureDeleteValue = await DbConnection.ExecuteScalarAsync<int>(SecureDeleteReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enums.TryToObject(secureDeleteValue, out SecureDeleteMode deleteMode))
                throw new InvalidOperationException($"Unable to map the value '{ secureDeleteValue }' to a member of { nameof(SecureDeleteMode) }.");

            return deleteMode;
        }

        /// <summary>
        /// Change the secure-delete setting. When secure delete is on, SQLite overwrites deleted content with zeros.
        /// </summary>
        /// <param name="deleteMode">The secure delete mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task SecureDeleteAsync(SecureDeleteMode deleteMode, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(SecureDeleteSetQuery(deleteMode), cancellationToken);

        /// <summary>
        /// Gets a query to read the secure delete pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SecureDeleteReadQuery => PragmaPrefix + "secure_delete";

        /// <summary>
        /// Creates a query to set the secure delete pragma.
        /// </summary>
        /// <param name="deleteMode">The secure delete mode.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="deleteMode"/> is an invalid enum value.</exception>
        protected virtual string SecureDeleteSetQuery(SecureDeleteMode deleteMode)
        {
            if (!deleteMode.IsValid())
                throw new ArgumentException($"The { nameof(SecureDeleteMode) } provided must be a valid enum.", nameof(deleteMode));

            var value = deleteMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "secure_delete = " + value;
        }

        /// <summary>
        /// Query the setting of the "synchronous" flag, which determines how much of the file operations require synchronization.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A value which determines the behaviour of file operations.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database returns an unknown or unsupported value.</exception>
        public async Task<SynchronousLevel> SynchronousAsync(CancellationToken cancellationToken = default)
        {
            var level = await DbConnection.ExecuteScalarAsync<int>(SynchronousReadQuery, cancellationToken).ConfigureAwait(false);
            if (!Enums.TryToObject(level, out SynchronousLevel syncLevel))
                throw new InvalidOperationException($"Unable to map the value '{ level }' to a member of { nameof(SynchronousLevel) }.");

            return syncLevel;
        }

        /// <summary>
        /// Sets the "synchronous" flag, which determines how much of the file operations require synchronization.
        /// </summary>
        /// <param name="synchronousLevel">The synchronous level.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task SynchronousAsync(SynchronousLevel synchronousLevel, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(SynchronousSetQuery(synchronousLevel), cancellationToken);

        /// <summary>
        /// Gets a query to read the synchronous pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynchronousReadQuery => PragmaPrefix + "synchronous";

        /// <summary>
        /// Creates a query to set the synchronous pragma.
        /// </summary>
        /// <param name="synchronousLevel">The synchronous level.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentException"><paramref name="synchronousLevel"/> is an invalid enum value.</exception>
        protected virtual string SynchronousSetQuery(SynchronousLevel synchronousLevel)
        {
            if (!synchronousLevel.IsValid())
                throw new ArgumentException($"The { nameof(SynchronousLevel) } provided must be a valid enum.", nameof(synchronousLevel));

            var value = synchronousLevel.ToString().ToUpperInvariant();
            return PragmaPrefix + "synchronous = " + value;
        }

        /// <summary>
        /// Returns information about each column in a database table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of column information, one element for each column in the table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        public Task<IEnumerable<pragma_table_info>> TableInfoAsync(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return DbConnection.QueryAsync<pragma_table_info>(TableInfoQuery(tableName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the table info pragma.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the given table name's does not match the current schema.</exception>
        protected virtual string TableInfoQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "table_info(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        /// <summary>
        /// Returns information about each column, and hidden columns in a database table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of column information, one element for each column or hidden column in the table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="tableName"/> has a schema that does not match the given database.</exception>
        public Task<IEnumerable<pragma_table_xinfo>> TableXInfoAsync(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return DbConnection.QueryAsync<pragma_table_xinfo>(TableXInfoQuery(tableName), cancellationToken);
        }

        /// <summary>
        /// Gets a query to read the table extra info pragma.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <returns>A SQL query.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the given table name's does not match the current schema.</exception>
        protected virtual string TableXInfoQuery(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema != null && !string.Equals(tableName.Schema, SchemaName, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The given table name's does not match the current schema. Given '{ tableName.Schema }', expected '{ SchemaName }'", nameof(tableName));

            return PragmaPrefix + "table_xinfo(" + Dialect.QuoteIdentifier(tableName.LocalName) + ")";
        }

        /// <summary>
        /// Queries the user-version, an integer that is available to applications to use however they want.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An integer representing a user-defined version of the database.</returns>
        public Task<int> UserVersionAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(UserVersionReadQuery, cancellationToken);

        /// <summary>
        /// Sets the user-version, an integer that is available to applications to use however they want.
        /// </summary>
        /// <param name="userVersion">The user-defined database version.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task indicating the completion of this query.</returns>
        public Task UserVersionAsync(int userVersion, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(UserVersionSetQuery(userVersion), cancellationToken);

        /// <summary>
        /// Gets a query to read the user version pragma.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserVersionReadQuery => PragmaPrefix + "user_version";

        /// <summary>
        /// Gets a query to set the user version pragma.
        /// </summary>
        /// <param name="userVersion">The user-defined database version.</param>
        /// <returns>A SQL query.</returns>
        protected virtual string UserVersionSetQuery(int userVersion) => PragmaPrefix + "user_version = " + userVersion.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Causes a checkpoint operation to run on the database.
        /// </summary>
        /// <param name="checkpointMode">The checkpoint mode.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Information on the status of the checkpoint operation once completed.</returns>
        /// <exception cref="ArgumentException"><paramref name="checkpointMode"/> is an invalid enum value.</exception>
        public Task<pragma_wal_checkpoint> WalCheckpointAsync(WalCheckpointMode checkpointMode = WalCheckpointMode.Passive, CancellationToken cancellationToken = default)
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            return WalCheckpointAsyncCore(checkpointMode, cancellationToken);
        }

        private async Task<pragma_wal_checkpoint> WalCheckpointAsyncCore(WalCheckpointMode checkpointMode, CancellationToken cancellationToken)
        {
            var result = await DbConnection.QueryAsync<pragma_wal_checkpoint>(WalCheckpointQuery(checkpointMode), cancellationToken).ConfigureAwait(false);
            return result.Single();
        }

        /// <summary>
        /// Gets a query to read the wal checkpoint pragma.
        /// </summary>
        /// <param name="checkpointMode">The checkpoint mode.</param>
        /// <returns>A SQL query</returns>
        /// <exception cref="ArgumentException"><paramref name="checkpointMode"/> is an invalid enum value.</exception>
        protected virtual string WalCheckpointQuery(WalCheckpointMode checkpointMode)
        {
            if (!checkpointMode.IsValid())
                throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(checkpointMode));

            var checkpointModeStr = checkpointMode.ToString().ToUpperInvariant();
            return PragmaPrefix + "wal_checkpoint(" + checkpointModeStr + ")";
        }
    }
}
