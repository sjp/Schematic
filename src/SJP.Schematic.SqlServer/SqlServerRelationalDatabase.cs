using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A relational database used to access and manage a SQL Server database.
/// </summary>
/// <seealso cref="IRelationalDatabase"/>
public class SqlServerRelationalDatabase : IRelationalDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerRelationalDatabase"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
    public SqlServerRelationalDatabase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        ArgumentNullException.ThrowIfNull(connection);

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        _tableProvider = new SqlServerRelationalDatabaseTableProvider(connection, identifierDefaults);
        _viewProvider = new SqlServerDatabaseViewProvider(connection, identifierDefaults);
        _sequenceProvider = new SqlServerDatabaseSequenceProvider(connection.DbConnection, identifierDefaults);
        _synonymProvider = new SqlServerDatabaseSynonymProvider(connection.DbConnection, identifierDefaults);
        _routineProvider = new SqlServerDatabaseRoutineProvider(connection.DbConnection, identifierDefaults);
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
        ArgumentNullException.ThrowIfNull(tableName);

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
        ArgumentNullException.ThrowIfNull(viewName);

        return _viewProvider.GetView(viewName, cancellationToken);
    }

    /// <summary>
    /// Gets all database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default)
    {
        return _sequenceProvider.GetAllSequences(cancellationToken);
    }

    /// <summary>
    /// Gets a database sequence.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return _sequenceProvider.GetSequence(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Gets all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
    {
        return _synonymProvider.GetAllSynonyms(cancellationToken);
    }

    /// <summary>
    /// Gets a database synonym.
    /// </summary>
    /// <param name="synonymName">A database synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return _synonymProvider.GetSynonym(synonymName, cancellationToken);
    }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        return _routineProvider.GetAllRoutines(cancellationToken);
    }

    /// <summary>
    /// Gets a database routine.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return _routineProvider.GetRoutine(routineName, cancellationToken);
    }

    private readonly IRelationalDatabaseTableProvider _tableProvider;
    private readonly IDatabaseViewProvider _viewProvider;
    private readonly IDatabaseSequenceProvider _sequenceProvider;
    private readonly IDatabaseSynonymProvider _synonymProvider;
    private readonly IDatabaseRoutineProvider _routineProvider;
}