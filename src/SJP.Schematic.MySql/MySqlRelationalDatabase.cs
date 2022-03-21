using System;
using System.Collections.Generic;
using System.Threading;
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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
    public MySqlRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

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
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
    {
        return _tableProvider.GetAllTables(cancellationToken);
    }

    /// <summary>
    /// Gets a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        return _tableProvider.GetTable(tableName, cancellationToken);
    }

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default)
    {
        return _viewProvider.GetAllViews(cancellationToken);
    }

    /// <summary>
    /// Gets a database view.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return _viewProvider.GetView(viewName, cancellationToken);
    }

    /// <summary>Gets all database sequences. This will always be an empty collection.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default)
    {
        return SequenceProvider.GetAllSequences(cancellationToken);
    }

    /// <summary>
    /// Gets a database sequence. This will always be a 'none' result.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        if (sequenceName == null)
            throw new ArgumentNullException(nameof(sequenceName));

        return SequenceProvider.GetSequence(sequenceName, cancellationToken);
    }

    /// <summary>Gets all database synonyms. This will always be an empty collection.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
    {
        return SynonymProvider.GetAllSynonyms(cancellationToken);
    }

    /// <summary>
    /// Gets a database synonym. This will always be a 'none' result.
    /// </summary>
    /// <param name="synonymName">A database synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database synonym in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        if (synonymName == null)
            throw new ArgumentNullException(nameof(synonymName));

        return SynonymProvider.GetSynonym(synonymName, cancellationToken);
    }

    /// <summary>
    /// Retrieves all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        return _routineProvider.GetAllRoutines(cancellationToken);
    }

    /// <summary>
    /// Retrieves a database routine, if available.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        if (routineName == null)
            throw new ArgumentNullException(nameof(routineName));

        return _routineProvider.GetRoutine(routineName, cancellationToken);
    }

    private readonly IRelationalDatabaseTableProvider _tableProvider;
    private readonly IDatabaseViewProvider _viewProvider;
    private readonly IDatabaseRoutineProvider _routineProvider;
    private static readonly IDatabaseSequenceProvider SequenceProvider = new EmptyDatabaseSequenceProvider();
    private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
}