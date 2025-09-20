using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A relational database used to access and manage a SQLite database.
/// </summary>
/// <seealso cref="IRelationalDatabase"/>
public class SqliteRelationalDatabase : ISqliteDatabase
{
    /// <summary>
    /// Constructs a new <see cref="SqliteRelationalDatabase"/>.
    /// </summary>
    /// <param name="connection">The connection to a SQLite database.</param>
    /// <param name="identifierDefaults">Default values for identifier components.</param>
    /// <param name="connectionPragma">Default values for identifier components.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/>, or <paramref name="identifierDefaults"/>, or <paramref name="connectionPragma"/> are <see langword="null" />.</exception>
    public SqliteRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, ISqliteConnectionPragma connectionPragma)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, connectionPragma, identifierDefaults);
        _viewProvider = new SqliteDatabaseViewProvider(connection, connectionPragma, identifierDefaults);
    }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection that is specific to a given SQLite database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
    {
        return _tableProvider.GetAllTables(cancellationToken);
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables2(CancellationToken cancellationToken = default)
    {
        return _tableProvider.GetAllTables2(cancellationToken);
    }

    /// <summary>
    /// Gets a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return _tableProvider.GetTable(tableName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default)
    {
        return _viewProvider.EnumerateAllViews(cancellationToken);
    }

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews2(CancellationToken cancellationToken = default)
    {
        return _viewProvider.GetAllViews2(cancellationToken);
    }

    /// <summary>
    /// Gets a database view.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return _viewProvider.GetView(viewName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database sequences. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> EnumerateAllSequences(CancellationToken cancellationToken = default)
    {
        return SequenceProvider.EnumerateAllSequences(cancellationToken);
    }

    /// <summary>
    /// Gets all database sequences. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database sequences.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences2(CancellationToken cancellationToken = default)
    {
        return SequenceProvider.GetAllSequences2(cancellationToken);
    }

    /// <summary>
    /// Gets a database sequence. This will always be a 'none' result.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return SequenceProvider.GetSequence(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database synonyms. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> EnumerateAllSynonyms(CancellationToken cancellationToken = default)
    {
        return SynonymProvider.EnumerateAllSynonyms(cancellationToken);
    }

    /// <summary>
    /// Gets all database synonyms. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database synonyms.</returns>
    public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms2(CancellationToken cancellationToken = default)
    {
        return SynonymProvider.GetAllSynonyms2(cancellationToken);
    }

    /// <summary>
    /// Gets a database synonym. This will always be a 'none' result.
    /// </summary>
    /// <param name="synonymName">A database synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database synonym in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return SynonymProvider.GetSynonym(synonymName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database routines. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default)
    {
        return RoutineProvider.EnumerateAllRoutines(cancellationToken);
    }

    /// <summary>
    /// Gets all database routines. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database routines.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines2(CancellationToken cancellationToken = default)
    {
        return RoutineProvider.GetAllRoutines2(cancellationToken);
    }

    /// <summary>
    /// Gets a database routine. This will always be a 'none' result.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return RoutineProvider.GetRoutine(routineName, cancellationToken);
    }

    /// <summary>
    /// Adds another database file to the current database connection.
    /// </summary>
    /// <param name="schemaName">The name to assign for the attached database.</param>
    /// <param name="fileName">The path to a SQLite database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="fileName"/> or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <c>main</c>.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("'main' is not a valid name to assign to an attached database. It will always be present.", nameof(schemaName));

        var sql = AttachDatabaseQuery(schemaName, fileName);
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Constructs a SQL query that adds another database file to the current database connection.
    /// </summary>
    /// <param name="schemaName">The name to assign for the attached database.</param>
    /// <param name="fileName">The path to a SQLite database.</param>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="fileName"/> or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <c>main</c>.</exception>
    /// <returns>A SQL query that can be used to add a database file to the current connection.</returns>
    protected string AttachDatabaseQuery(string schemaName, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("'main' is not a valid name to assign to an attached database. It will always be present.", nameof(schemaName));

        var quotedSchemaName = Dialect.QuoteIdentifier(schemaName);
        var escapedFileName = fileName.Replace("'", "''", StringComparison.Ordinal);

        return $"ATTACH DATABASE '{escapedFileName}' AS {quotedSchemaName}";
    }

    /// <summary>
    /// Removes a database file from the current database connection.
    /// </summary>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <c>main</c>.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("'main' is not a valid database name to remove. It must always be present.", nameof(schemaName));

        var sql = DetachDatabaseQuery(schemaName);
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Constructs a SQL query that removes an attached database from the current database connection.
    /// </summary>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemaName"/> is <c>main</c>.</exception>
    /// <returns>A SQL query that can be used to remove a database file from the current connection.</returns>
    protected string DetachDatabaseQuery(string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        if ("main".Equals(schemaName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("'main' is not a valid database name to remove. It must always be present.", nameof(schemaName));

        return "DETACH DATABASE " + Dialect.QuoteIdentifier(schemaName);
    }

    /// <summary>
    /// The <c>VACUUM</c> command rebuilds the database file, repacking it into a minimal amount of disk space.
    /// </summary>
    /// <remarks>This will be run only on the main database.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task VacuumAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "VACUUM";
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// The <c>VACUUM</c> command rebuilds the database file, repacking it into a minimal amount of disk space.
    /// </summary>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        var sql = VacuumQuery(schemaName);
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// The <c>VACUUM INTO</c> command rebuilds the database file, repacking it into a minimal amount of disk space in a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="filePath"/> is null, empty or whitespace.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task VacuumIntoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var sql = VacuumIntoQuery(filePath);
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// The <c>VACUUM INTO</c> command rebuilds the database file, repacking it into a minimal amount of disk space in a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task VacuumIntoAsync(string filePath, string schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        var sql = VacuumIntoQuery(filePath, schemaName);
        return DbConnection.ExecuteAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Constructs a SQL query that rebuild and repack a database file.
    /// </summary>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <returns>A SQL query that can be used to rebuild and repack a database file.</returns>
    protected string VacuumQuery(string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return "VACUUM " + Dialect.QuoteIdentifier(schemaName);
    }

    /// <summary>
    /// Constructs a SQL query that rebuild and repack a database into a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="filePath"/> is null, empty or whitespace.</exception>
    /// <returns>A SQL query that can be used to rebuild and repack a database file.</returns>
    protected string VacuumIntoQuery(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return "VACUUM INTO '" + filePath.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    /// <summary>
    /// Constructs a SQL query that rebuild and repack a database into a separate file.
    /// </summary>
    /// <param name="filePath">A file path that will store the resulting vacuum'd database.</param>
    /// <param name="schemaName">The name of an attached database.</param>
    /// <exception cref="ArgumentNullException">Thrown when or <paramref name="schemaName"/> is null, empty or whitespace.</exception>
    /// <returns>A SQL query that can be used to rebuild and repack a database file.</returns>
    protected string VacuumIntoQuery(string filePath, string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        return "VACUUM " + Dialect.QuoteIdentifier(schemaName) + " INTO '" + filePath.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    private readonly IRelationalDatabaseTableProvider _tableProvider;
    private readonly IDatabaseViewProvider _viewProvider;
    private static readonly IDatabaseSequenceProvider SequenceProvider = new EmptyDatabaseSequenceProvider();
    private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
    private static readonly IDatabaseRoutineProvider RoutineProvider = new EmptyDatabaseRoutineProvider();
}