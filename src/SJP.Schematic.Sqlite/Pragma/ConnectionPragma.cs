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

namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// A <c>PRAGMA</c> accessor for a SQLite connection.
/// </summary>
/// <seealso cref="ISqliteConnectionPragma" />
public class ConnectionPragma : ISqliteConnectionPragma
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionPragma"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public ConnectionPragma(ISchematicConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// A database connection that is specific to a given SQLite database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory for querying the SQLite connection.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// A prefix for generating queries for PRAGMA.
    /// </summary>
    /// <value>A pragma prefix.</value>
    protected string PragmaPrefix { get; } = "PRAGMA ";

    /// <summary>
    /// Retrieves the schema-specific pragma accessors.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of schema-specific pragma accessors.</returns>
    public async Task<IEnumerable<ISqliteDatabasePragma>> DatabasePragmasAsync(CancellationToken cancellationToken = default)
    {
        var databases = await DatabaseListAsync(cancellationToken).ConfigureAwait(false);
        return databases
            .OrderBy(static d => d.seq)
            .Select(d => new DatabasePragma(Connection, d.name!))
            .ToList();
    }

    /// <summary>
    /// Queries the limit of the approximate <c>ANALYZE</c> setting. This is approximate number of rows examined in each index by the <c>ANALYZE</c> command.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The approximate number of rows analysis is limited to when running <c>ANALYZE</c>. Zero indicates analyzing all rows.</returns>
    public Task<uint> AnalysisLimitAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<uint>(AnalysisLimitReadQuery, cancellationToken);

    /// <summary>
    /// Changes the limit of the approximate <c>ANALYZE</c> setting. This is approximate number of rows examined in each index by the <c>ANALYZE</c> command.
    /// </summary>
    /// <param name="rowLimit">The approximate number of rows to limit analysis to. Zero indicates analyzing all rows.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task AnalysisLimitAsync(uint rowLimit, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(AnalysisLimitSetQuery(rowLimit), cancellationToken);

    /// <summary>
    /// Gets a query to read the analysis limit pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string AnalysisLimitReadQuery => PragmaPrefix + "analysis_limit";

    /// <summary>
    /// Creates a query that sets the analysis limit pragma.
    /// </summary>
    /// <param name="rowLimit">The approximate number of rows to limit analysis to. Zero indicates analyzing all rows.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string AnalysisLimitSetQuery(uint rowLimit) => PragmaPrefix + "analysis_limit = " + rowLimit.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries the automatic indexing capability.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if automatic indexing is enabled; otherwise <c>false</c>.</returns>
    public Task<bool> AutomaticIndexAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(AutomaticIndexReadQuery, cancellationToken);

    /// <summary>
    /// Sets the automatic indexing capability.
    /// </summary>
    /// <param name="enable">if set to <c>true</c> enables automatic indexing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task AutomaticIndexAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(AutomaticIndexSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the automatic index pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string AutomaticIndexReadQuery => PragmaPrefix + "automatic_index";

    /// <summary>
    /// Creates a query that sets the automatic index pragma.
    /// </summary>
    /// <param name="enable">if set to <c>true</c> enables automatic indexing.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string AutomaticIndexSetQuery(bool enable) => PragmaPrefix + "automatic_index = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries the timeout used before a busy handler is invoked.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The timespan of the busy timeout.</returns>
    public async Task<TimeSpan> BusyTimeoutAsync(CancellationToken cancellationToken = default)
    {
        var ms = await DbConnection.ExecuteScalarAsync<int>(BusyTimeoutReadQuery, cancellationToken).ConfigureAwait(false);
        return TimeSpan.FromMilliseconds(ms);
    }

    /// <summary>
    /// Sets the timeout used before a busy handler is invoked.
    /// </summary>
    /// <param name="timeout">A timeout timespan.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task BusyTimeoutAsync(TimeSpan timeout, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(BusyTimeoutSetQuery(timeout), cancellationToken);

    /// <summary>
    /// Gets a query to read the busy timeout pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string BusyTimeoutReadQuery => PragmaPrefix + "busy_timeout";

    /// <summary>
    /// Creates a query that sets the busy timeout pragma.
    /// </summary>
    /// <param name="timeout">A timeout timespan.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string BusyTimeoutSetQuery(TimeSpan timeout)
    {
        var ms = timeout.TotalMilliseconds < 1 ? 0 : (int)timeout.TotalMilliseconds;
        return PragmaPrefix + "busy_timeout = " + ms.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Sets case sensitivity for <c>LIKE</c> expressions. When disabled, the default <c>LIKE</c> behavior is expressed. When enabled, case becomes significant.
    /// </summary>
    /// <param name="enable">If enabled, ensures case sensitivity for <c>LIKE</c> expressions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task CaseSensitiveLikeAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CaseSensitiveLikeSetQuery(enable), cancellationToken);

    /// <summary>
    /// Creates a query that sets the case sensitive like pragma.
    /// </summary>
    /// <param name="enable">If enabled, ensures case sensitivity for <c>LIKE</c> expressions.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string CaseSensitiveLikeSetQuery(bool enable) => PragmaPrefix + "case_sensitive_like = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Retrieves whether cell size checking is enabled. When enabled, database corruption is detected earlier and is less likely to "spread". However, there is a small performance hit for doing the extra checks and so cell size checking is turned off by default.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if cell size checking is enabled; otherwise <c>false</c>.</returns>
    public Task<bool> CellSizeCheckAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(CellSizeCheckReadQuery, cancellationToken);

    /// <summary>
    /// Sets whether cell size checking is enabled. When enabled, database corruption is detected earlier and is less likely to "spread". However, there is a small performance hit for doing the extra checks and so cell size checking is turned off by default.
    /// </summary>
    /// <param name="enable">If enabled, performs cell size checking.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task CellSizeCheckAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CellSizeCheckSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the cell size check pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string CellSizeCheckReadQuery => PragmaPrefix + "cell_size_check";

    /// <summary>
    /// Creates a query that sets the cell size check pragma.
    /// </summary>
    /// <param name="enable">If enabled, performs cell size checking.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string CellSizeCheckSetQuery(bool enable) => PragmaPrefix + "cell_size_check = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Query or change the fullfsync flag for checkpoint operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the fullfsync flag is enabled; otherwise <c>false</c>.</returns>
    public Task<bool> CheckpointFullFsyncAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(CheckpointFullFsyncReadQuery, cancellationToken);

    /// <summary>
    /// Change the fullfsync flag for checkpoint operations.
    /// </summary>
    /// <param name="enable">If <c>true</c>, the flag must be enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task CheckpointFullFsyncAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(CheckpointFullFsyncSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the checkpoint full fsync pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string CheckpointFullFsyncReadQuery => PragmaPrefix + "checkpoint_fullfsync";

    /// <summary>
    /// Creates a query that sets the checkpoint fullfsync pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, the flag must be enabled.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string CheckpointFullFsyncSetQuery(bool enable) => PragmaPrefix + "checkpoint_fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Return a list of the collating sequences defined for the current database connection
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of collation information.</returns>
    public Task<IEnumerable<pragma_collation_list>> CollationListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_collation_list>(CollationListReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the collation list pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string CollationListReadQuery => PragmaPrefix + "collation_list";

    /// <summary>
    /// This pragma returns the names of compile-time options used when building SQLite, one option per row.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of compile-time options.</returns>
    public Task<IEnumerable<string>> CompileOptionsAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<string>(CompileOptionsReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the compile time options pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string CompileOptionsReadQuery => PragmaPrefix + "compile_options";

    /// <summary>
    /// The <c>PRAGMA data_version</c> command provides an indication that the database file has been modified.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A version number.</returns>
    public Task<int> DataVersionAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(DataVersionReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the data version pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string DataVersionReadQuery => PragmaPrefix + "data_version";

    /// <summary>
    /// This pragma works like a query to return one row for each database attached to the current database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database information.</returns>
    public Task<IEnumerable<pragma_database_list>> DatabaseListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_database_list>(DatabaseListReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the database list pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string DatabaseListReadQuery => PragmaPrefix + "database_list";

    /// <summary>
    /// Queries whether <c>defer_foreign_keys</c> <c>PRAGMA</c> is on. When enabled, enforcement of all foreign key constraints is delayed until the outermost transaction is committed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if deferring of foreign key constraints is enabled; otherwise <c>false</c>.</returns>
    public Task<bool> DeferForeignKeysAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(DeferForeignKeysReadQuery, cancellationToken);

    /// <summary>
    /// Sets the <c>defer_foreign_keys</c> <c>PRAGMA</c>. When enabled, enforcement of all foreign key constraints is delayed until the outermost transaction is committed.
    /// </summary>
    /// <param name="enable">If <c>true</c>, enabled deferring of foreign key enforcement.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task DeferForeignKeysAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(DeferForeignKeysSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the defer foreign keys pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string DeferForeignKeysReadQuery => PragmaPrefix + "defer_foreign_keys";

    /// <summary>
    /// Creates a query that sets the defer foreign keys pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, enabled deferring of foreign key enforcement.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string DeferForeignKeysSetQuery(bool enable) => PragmaPrefix + "defer_foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Retrieves the text encoding used by the main database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The text encoding of the main database.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unknown encoding was found.</exception>
    public async Task<Encoding> EncodingAsync(CancellationToken cancellationToken = default)
    {
        var encodingName = await DbConnection.ExecuteScalarAsync<string>(EncodingReadQuery, cancellationToken).ConfigureAwait(false);
        if (!NameEncodingMapping.ContainsKey(encodingName))
            throw new InvalidOperationException("Unknown and unsupported encoding found: " + encodingName);

        return NameEncodingMapping[encodingName];
    }

    /// <summary>
    /// Sets the text encoding used by the main database.
    /// </summary>
    /// <param name="encoding">A text encoding.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task EncodingAsync(Encoding encoding, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(EncodingSetQuery(encoding), cancellationToken);

    /// <summary>
    /// Gets a query to read the encoding pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string EncodingReadQuery => PragmaPrefix + "encoding";

    /// <summary>
    /// Creates a query that sets the encoding pragma.
    /// </summary>
    /// <param name="encoding">A text encoding.</param>
    /// <returns>A SQL query.</returns>
    /// <exception cref="ArgumentException"><paramref name="encoding"/> is an invalid enum.</exception>
    protected virtual string EncodingSetQuery(Encoding encoding)
    {
        if (!encoding.IsValid())
            throw new ArgumentException($"The { nameof(Encoding) } provided must be a valid enum.", nameof(encoding));

        var value = EncodingNameMapping[encoding];
        return PragmaPrefix + "encoding = '" + value + "'";
    }

    /// <summary>
    /// Queries whether foreign key constraints are enforced.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if foreign key constraints are enforced; otherwise <c>false</c>.</returns>
    public Task<bool> ForeignKeysAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(ForeignKeysReadQuery, cancellationToken);

    /// <summary>
    /// Enables or disables enforcement of foreign key constraints.
    /// </summary>
    /// <param name="enable">If <c>true</c>, foreign key constraints should be enforced.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task ForeignKeysAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ForeignKeysSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the foreign keys pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ForeignKeysReadQuery => PragmaPrefix + "foreign_keys";

    /// <summary>
    /// Creates a query that sets the foreign keys pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, foreign key constraints should be enforced.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string ForeignKeysSetQuery(bool enable) => PragmaPrefix + "foreign_keys = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Query the fullfsync flag.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the fullfsync flag is enabled; otherwise <c>false</c>.</returns>
    public Task<bool> FullFsyncAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(FullFsyncReadQuery, cancellationToken);

    /// <summary>
    /// Change the fullfsync flag.
    /// </summary>
    /// <param name="enable">If <c>true</c>, sets the fullfsync flag to <c>true</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task FullFsyncAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(FullFsyncSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the full fsync pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string FullFsyncReadQuery => PragmaPrefix + "fullfsync";

    /// <summary>
    /// Creates a query that sets the fullfsync pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, sets the fullfsync flag to <c>true</c>.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string FullFsyncSetQuery(bool enable) => PragmaPrefix + "fullfsync = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Return a list of SQL functions known to the database connection
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of SQL functions.</returns>
    public Task<IEnumerable<pragma_function_list>> FunctionListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_function_list>(FunctionListReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the function list pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string FunctionListReadQuery => PragmaPrefix + "function_list";

    /// <summary>
    /// Enables or disables the enforcement of <c>CHECK</c> constraints.
    /// </summary>
    /// <param name="enable">If <c>true</c>, disables enforcement of <c>CHECK</c> constraints.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task IgnoreCheckConstraintsAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(IgnoreCheckConstraintsSetQuery(enable), cancellationToken);

    /// <summary>
    /// Creates a query that sets the ignore check constraints pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, disables enforcement of <c>CHECK</c> constraints.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string IgnoreCheckConstraintsSetQuery(bool enable) => PragmaPrefix + "ignore_check_constraints = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries the value of the <c>legacy_alter_table</c> flag.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the <c>ALTER TABLE</c> behaviour works as it did in v3.24.0 and earlier.</returns>
    public Task<bool> LegacyAlterTableAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(LegacyAlterTableReadQuery, cancellationToken);

    /// <summary>
    /// Sets the value of the <c>legacy_alter_table</c> flag.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> the <c>ALTER TABLE</c> behaviour will work as it did in v3.24.0 and earlier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task LegacyAlterTableAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(LegacyAlterTableSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the legacy alter table pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string LegacyAlterTableReadQuery => PragmaPrefix + "legacy_alter_table";

    /// <summary>
    /// Creates a query that sets the legacy alter table pragma.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> the <c>ALTER TABLE</c> behaviour will work as it did in v3.24.0 and earlier.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string LegacyAlterTableSetQuery(bool enable) => PragmaPrefix + "legacy_alter_table = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Retrieves a list of virtual table modules registered with the database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of virtual table modules.</returns>
    public Task<IEnumerable<string>> ModuleListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<string>(ModuleListReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the module list pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ModuleListReadQuery => PragmaPrefix + "module_list";

    /// <summary>
    /// Attempt to optimize the database.
    /// </summary>
    /// <param name="features">The optimisation options to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<IEnumerable<string>> OptimizeAsync(OptimizeFeatures features = OptimizeFeatures.Analyze, CancellationToken cancellationToken = default) => DbConnection.QueryAsync<string>(OptimizeSetQuery(features), cancellationToken);

    /// <summary>
    /// Creates a query that sets the legacy alter table pragma.
    /// </summary>
    /// <param name="features">The optimisation options to run.</param>
    /// <returns>A SQL query.</returns>
    /// <exception cref="ArgumentException"><paramref name="features"/> is an invalid enum.</exception>
    protected virtual string OptimizeSetQuery(OptimizeFeatures features)
    {
        if (!features.IsValid())
            throw new ArgumentException($"The { nameof(OptimizeFeatures) } provided must be a valid enum.", nameof(features));

        var value = (int)features;
        return PragmaPrefix + "optimize = " + value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Return a list of <c>PRAGMA</c> commands known to the database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of <c>PRAGMA</c> command names.</returns>
    public Task<IEnumerable<string>> PragmaListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<string>(PragmaListReadQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the pragma list pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string PragmaListReadQuery => PragmaPrefix + "pragma_list";

    /// <summary>
    /// Determines whether the database can only be queried and not mutated.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the database is read-only; otherwise <c>false</c>.</returns>
    public Task<bool> QueryOnlyAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(QueryOnlyReadQuery, cancellationToken);

    /// <summary>
    /// Prevent all changes to database files when enabled, ensuring only queries are enabled.
    /// </summary>
    /// <param name="enable">If <c>true</c>, only read-only queries may be run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task QueryOnlyAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(QueryOnlySetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the query only pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string QueryOnlyReadQuery => PragmaPrefix + "query_only";

    /// <summary>
    /// Creates a query that sets the query only pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, only read-only queries may be run.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string QueryOnlySetQuery(bool enable) => PragmaPrefix + "query_only = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Query the <c>READ UNCOMMITTED</c> transaction isolation level. The default isolation level for SQLite is <c>SERIALIZABLE</c>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the isolation level is <c>READ UNCOMMITTED</c>.</returns>
    public Task<bool> ReadUncommittedAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(ReadUncommittedReadQuery, cancellationToken);

    /// <summary>
    /// Set the <c>READ UNCOMMITTED</c> transaction isolation level. The default isolation level for SQLite is <c>SERIALIZABLE</c>.
    /// </summary>
    /// <param name="enable">If <c>true</c>, sets the isolation level to <c>READ UNCOMMITTED</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task ReadUncommittedAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ReadUncommittedSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the read uncommitted pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ReadUncommittedReadQuery => PragmaPrefix + "read_uncommitted";

    /// <summary>
    /// Creates a query that sets the read uncommitted pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, sets the isolation level to <c>READ UNCOMMITTED</c>.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string ReadUncommittedSetQuery(bool enable) => PragmaPrefix + "read_uncommitted = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Query the recursive trigger capability.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if recursive triggers are enabled; otherwise <c>false</c>.</returns>
    public Task<bool> RecursiveTriggersAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(RecursiveTriggersReadQuery, cancellationToken);

    /// <summary>
    /// Set, or clear the recursive trigger capability.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> recursive triggers are enabled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task RecursiveTriggersAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(RecursiveTriggersSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the recursive triggers pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string RecursiveTriggersReadQuery => PragmaPrefix + "recursive_triggers";

    /// <summary>
    /// Creates a query that sets the recursive triggers pragma.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> recursive triggers are enabled.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string RecursiveTriggersSetQuery(bool enable) => PragmaPrefix + "recursive_triggers = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries whether statements missing an <c>ORDER BY</c> emit results in reverse order.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>If <c>true</c>, unordered <c>SELECT</c> queries are returned in reverse order; otherwise <c>false</c>.</returns>
    public Task<bool> ReverseUnorderedSelectsAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(ReverseUnorderedSelectsReadQuery, cancellationToken);

    /// <summary>
    /// Sets whether statements missing an <c>ORDER BY</c> emit results in reverse order.
    /// </summary>
    /// <param name="enable">If <c>true</c>, unordered <c>SELECT</c> queries are returned in reverse order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns> A task indicating the completion of this query.</returns>
    public Task ReverseUnorderedSelectsAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ReverseUnorderedSelectsSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the reverse unordered selects pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ReverseUnorderedSelectsReadQuery => PragmaPrefix + "reverse_unordered_selects";

    /// <summary>
    /// Creates a query that sets the reverse unordered selects pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c>, unordered <c>SELECT</c> queries are returned in reverse order.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string ReverseUnorderedSelectsSetQuery(bool enable) => PragmaPrefix + "reverse_unordered_selects = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Causes the database connection on which it is invoked to free up as much memory as it can.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task ShrinkMemoryAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ShrinkMemoryQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read the shrink memory pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ShrinkMemoryQuery => PragmaPrefix + "shrink_memory";

    /// <summary>
    /// Queries the size of the heap used by all database connections within a single process.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The size of the heap limit.</returns>
    public Task<long> SoftHeapLimitAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<long>(SoftHeapLimitReadQuery, cancellationToken);

    /// <summary>
    /// Sets the size of the heap used by all database connections within a single process.
    /// </summary>
    /// <param name="heapLimit">The heap limit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task SoftHeapLimitAsync(long heapLimit, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(SoftHeapLimitSetQuery(heapLimit), cancellationToken);

    /// <summary>
    /// Gets a query to read the soft heap limit pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string SoftHeapLimitReadQuery => PragmaPrefix + "soft_heap_limit";

    /// <summary>
    /// Creates a query that sets the soft heap limit pragma.
    /// </summary>
    /// <param name="heapLimit">The heap limit.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string SoftHeapLimitSetQuery(long heapLimit) => PragmaPrefix + "soft_heap_limit = " + heapLimit.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns information about each table or view in all schemas.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table or view information, one element for each table or view in all schemas.</returns>
    public Task<IEnumerable<pragma_table_list>> TableListAsync(CancellationToken cancellationToken = default) => DbConnection.QueryAsync<pragma_table_list>(TableListQuery, cancellationToken);

    /// <summary>
    /// Gets a query to read table information for the connection.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableListQuery => PragmaPrefix + "table_list";

    /// <summary>
    /// Returns information about a given table or view in the given schema.
    /// </summary>
    /// <param name="tableName">A table or view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Information relevant to the given table or view. Will be <c>null</c> if the table or view does not exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    public Task<IEnumerable<pragma_table_list>> TableListAsync(Identifier tableName, CancellationToken cancellationToken = default)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        return DbConnection.QueryAsync<pragma_table_list>(TableListTableQuery(tableName), cancellationToken);
    }

    /// <summary>
    /// Gets a query to read table information pragma for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <returns>A SQL query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual string TableListTableQuery(Identifier tableName)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        // default to 'main'
        var identifier = Identifier.CreateQualifiedIdentifier(tableName.Schema ?? "main", tableName.LocalName);
        return PragmaPrefix + "table_list(" + Connection.Dialect.QuoteName(identifier) + ")";
    }

    /// <summary>
    /// Queries where temporary storage is located.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The location of the temporary store.</returns>
    /// <exception cref="InvalidOperationException">Throws when an unknown temporary storage location was encountered.</exception>
    public async Task<TemporaryStoreLocation> TemporaryStoreAsync(CancellationToken cancellationToken = default)
    {
        var location = await DbConnection.ExecuteScalarAsync<int>(TemporaryStoreReadQuery, cancellationToken).ConfigureAwait(false);
        if (!Enums.TryToObject(location, out TemporaryStoreLocation tempLocation))
            throw new InvalidOperationException($"Unable to map the value '{ location.ToString(CultureInfo.InvariantCulture) }' to a member of { nameof(TemporaryStoreLocation) }.");

        return tempLocation;
    }

    /// <summary>
    /// Sets the temporary storage location.
    /// </summary>
    /// <param name="tempLocation">The temporary location.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task TemporaryStoreAsync(TemporaryStoreLocation tempLocation, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(TemporaryStoreSetQuery(tempLocation), cancellationToken);

    /// <summary>
    /// Gets a query to read the temporary store pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TemporaryStoreReadQuery => PragmaPrefix + "temp_store";

    /// <summary>
    /// Creates a query that sets the temporary store pragma.
    /// </summary>
    /// <param name="tempLocation">The temporary location.</param>
    /// <returns>A SQL query.</returns>
    /// <exception cref="ArgumentException"><paramref name="tempLocation"/> is an invalid enum.</exception>
    protected virtual string TemporaryStoreSetQuery(TemporaryStoreLocation tempLocation)
    {
        if (!tempLocation.IsValid())
            throw new ArgumentException($"The { nameof(TemporaryStoreLocation) } provided must be a valid enum.", nameof(tempLocation));

        var value = tempLocation.ToString().ToUpperInvariant();
        return PragmaPrefix + "temp_store = " + value;
    }

    /// <summary>
    /// This limit sets an upper bound on the number of auxiliary threads that a prepared statement is allowed to launch to assist with a query.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The upper bound on the number of threads that are able to be used.</returns>
    public Task<int> ThreadsAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(ThreadsReadQuery, cancellationToken);

    /// <summary>
    /// Sets an upper bound on the number of auxiliary threads that a prepared statement is allowed to launch to assist with a query.
    /// </summary>
    /// <param name="maxThreads">The maximum threads.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task ThreadsAsync(int maxThreads, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(ThreadsSetQuery(maxThreads), cancellationToken);

    /// <summary>
    /// Gets a query to read the threads pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ThreadsReadQuery => PragmaPrefix + "threads";

    /// <summary>
    /// Creates a query that sets the threads pragma.
    /// </summary>
    /// <param name="maxThreads">The maximum threads.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string ThreadsSetQuery(int maxThreads) => PragmaPrefix + "threads = " + maxThreads.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries the write-ahead log auto-checkpoint interval.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The maximum number of pages in the write-ahead log before checkpointing.</returns>
    public Task<int> WalAutoCheckpointAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<int>(WalAutoCheckpointReadQuery, cancellationToken);

    /// <summary>
    /// Sets the write-ahead log auto-checkpoint interval.
    /// </summary>
    /// <param name="maxPages">The maximum number of pages before a checkpoint.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task WalAutoCheckpointAsync(int maxPages, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(WalAutoCheckpointSetQuery(maxPages), cancellationToken);

    /// <summary>
    /// Gets a query to read the WAL auto checkpoint pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string WalAutoCheckpointReadQuery => PragmaPrefix + "wal_autocheckpoint";

    /// <summary>
    /// Creates a query that sets the WAL auto checkpoint pragma.
    /// </summary>
    /// <param name="maxPages">The maximum number of pages before a checkpoint.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string WalAutoCheckpointSetQuery(int maxPages) => PragmaPrefix + "wal_autocheckpoint = " + maxPages.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Queries whether the <c>sqlite_master</c> table can be changed using ordinary <c>UPDATE</c>, <c>INSERT</c>, and <c>DELETE</c> statements.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the <c>sqlite_master</c> is able to be modified directly; otherwise <c>false</c>.</returns>
    public Task<bool> WritableSchemaAsync(CancellationToken cancellationToken = default) => DbConnection.ExecuteScalarAsync<bool>(WritableSchemaReadQuery, cancellationToken);

    /// <summary>
    /// Sets whether the <c>sqlite_master</c> table can be changed using ordinary <c>UPDATE</c>, <c>INSERT</c>, and <c>DELETE</c> statements.
    /// </summary>
    /// <param name="enable">If <c>true</c> enables the <c>sqlite_master</c> to be modified directly.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating the completion of this query.</returns>
    public Task WritableSchemaAsync(bool enable, CancellationToken cancellationToken = default) => DbConnection.ExecuteAsync(WritableSchemaSetQuery(enable), cancellationToken);

    /// <summary>
    /// Gets a query to read the writable schema pragma.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string WritableSchemaReadQuery => PragmaPrefix + "writable_schema";

    /// <summary>
    /// Creates a query that sets the writable schema pragma.
    /// </summary>
    /// <param name="enable">If <c>true</c> enables the <c>sqlite_master</c> to be modified directly.</param>
    /// <returns>A SQL query.</returns>
    protected virtual string WritableSchemaSetQuery(bool enable) => PragmaPrefix + "writable_schema = " + Convert.ToInt32(enable).ToString(CultureInfo.InvariantCulture);

    private static readonly IReadOnlyDictionary<Encoding, string> EncodingNameMapping = new Dictionary<Encoding, string>
    {
        [Encoding.Utf8] = "UTF-8",
        [Encoding.Utf16] = "UTF-16",
        [Encoding.Utf16le] = "UTF-16le",
        [Encoding.Utf16be] = "UTF-16be"
    };

    private static readonly IReadOnlyDictionary<string, Encoding> NameEncodingMapping = new Dictionary<string, Encoding>(StringComparer.Ordinal)
    {
        ["UTF-8"] = Encoding.Utf8,
        ["UTF-16"] = Encoding.Utf16,
        ["UTF-16le"] = Encoding.Utf16le,
        ["UTF-16be"] = Encoding.Utf16be
    };
}
