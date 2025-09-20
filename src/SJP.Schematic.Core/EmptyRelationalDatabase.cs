using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// An <see cref="IRelationalDatabase"/> instance that never returns any database objects. Not intended to be used directly.
/// </summary>
/// <seealso cref="IRelationalDatabase" />
public sealed class EmptyRelationalDatabase : IRelationalDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyRelationalDatabase"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public EmptyRelationalDatabase(IIdentifierDefaults identifierDefaults)
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <inheritdoc />
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Enumerates all database tables. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
    {
        return TableProvider.GetAllTables(cancellationToken);
    }

    /// <summary>
    /// Gets all database tables. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database tables.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables2(CancellationToken cancellationToken = default)
    {
        return TableProvider.GetAllTables2(cancellationToken);
    }

    /// <summary>
    /// Gets a database table. This will always be a 'none' result.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return TableProvider.GetTable(tableName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database views. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default)
    {
        return ViewProvider.EnumerateAllViews(cancellationToken);
    }

    /// <summary>
    /// Gets all database views. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database views.</returns>
    public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews2(CancellationToken cancellationToken = default)
    {
        return ViewProvider.GetAllViews2(cancellationToken);
    }

    /// <summary>
    /// Gets a database view. This will always be a 'none' result.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return ViewProvider.GetView(viewName, cancellationToken);
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

    private static readonly IRelationalDatabaseTableProvider TableProvider = new EmptyRelationalDatabaseTableProvider();
    private static readonly IDatabaseViewProvider ViewProvider = new EmptyDatabaseViewProvider();
    private static readonly IDatabaseSequenceProvider SequenceProvider = new EmptyDatabaseSequenceProvider();
    private static readonly IDatabaseSynonymProvider SynonymProvider = new EmptyDatabaseSynonymProvider();
    private static readonly IDatabaseRoutineProvider RoutineProvider = new EmptyDatabaseRoutineProvider();
}