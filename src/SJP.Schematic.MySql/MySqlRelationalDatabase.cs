using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql;

/// <summary>
/// A relational database used to access and manage a MySQL database.
/// </summary>
/// <seealso cref="IRelationalDatabase"/>
public class MySqlRelationalDatabase : IRelationalDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlRelationalDatabase"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" /> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public MySqlRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        ArgumentNullException.ThrowIfNull(connection);

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        _tableProvider = new MySqlRelationalDatabaseTableProvider(connection, identifierDefaults);
        _viewProvider = new MySqlDatabaseViewProvider(connection, identifierDefaults);
        _routineProvider = new MySqlDatabaseRoutineProvider(connection, identifierDefaults);
    }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

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
    /// Enumerates all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default)
    {
        return _routineProvider.EnumerateAllRoutines(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines2(CancellationToken cancellationToken = default)
    {
        return _routineProvider.GetAllRoutines2(cancellationToken);
    }

    /// <summary>
    /// Retrieves a database routine, if available.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return _routineProvider.GetRoutine(routineName, cancellationToken);
    }

    private readonly IRelationalDatabaseTableProvider _tableProvider;
    private readonly IDatabaseViewProvider _viewProvider;
    private readonly IDatabaseRoutineProvider _routineProvider;
    private static readonly IDatabaseSequenceProvider SequenceProvider = new EmptyDatabaseSequenceProvider();
    private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
}