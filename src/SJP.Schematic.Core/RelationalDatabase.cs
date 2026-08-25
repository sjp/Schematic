using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// Not intended to be used directly. Used to store and access database objects in memory.
/// </summary>
public class RelationalDatabase : IRelationalDatabase
{
    private readonly FrozenDictionary<Identifier, IRelationalDatabaseTable> _tablesByName;
    private readonly FrozenDictionary<Identifier, IDatabaseView> _viewsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseSequence> _sequencesByName;
    private readonly FrozenDictionary<Identifier, IDatabaseSynonym> _synonymsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseRoutine> _routinesByName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabase"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="tables">A collection of database tables.</param>
    /// <param name="views">A collection of database views.</param>
    /// <param name="sequences">A collection of database sequences.</param>
    /// <param name="synonyms">A collection of database synonyms.</param>
    /// <param name="routines">A collection of database routines.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <see langword="null" />. Alternatively if <paramref name="tables"/>, <paramref name="views"/>, <paramref name="sequences"/>, <paramref name="synonyms"/> or <paramref name="routines"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    public RelationalDatabase(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines
    )
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        Tables = tables.ToDefensiveCopy(nameof(tables));
        Views = views.ToDefensiveCopy(nameof(views));
        Sequences = sequences.ToDefensiveCopy(nameof(sequences));
        Synonyms = synonyms.ToDefensiveCopy(nameof(synonyms));
        Routines = routines.ToDefensiveCopy(nameof(routines));

        _tablesByName = BuildLookup(Tables);
        _viewsByName = BuildLookup(Views);
        _sequencesByName = BuildLookup(Sequences);
        _synonymsByName = BuildLookup(Synonyms);
        _routinesByName = BuildLookup(Routines);
    }

    /// <summary>
    /// Default values for identifiers in a database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Resolves identifiers when objects cannot be found using a given identifier.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// An in-memory collection of database tables.
    /// </summary>
    /// <value>A collection of database tables.</value>
    protected IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    /// <summary>
    /// An in-memory collection of database views.
    /// </summary>
    /// <value>A collection of database views.</value>
    protected IReadOnlyCollection<IDatabaseView> Views { get; }

    /// <summary>
    /// An in-memory collection of database sequences.
    /// </summary>
    /// <value>A collection of database sequences.</value>
    protected IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

    /// <summary>
    /// An in-memory collection of database synonyms.
    /// </summary>
    /// <value>A collection of database synonyms.</value>
    protected IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

    /// <summary>
    /// An in-memory collection of database routines.
    /// </summary>
    /// <value>A collection of database routines.</value>
    protected IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

    /// <summary>
    /// Qualifies the name of the object so that they can be compared during lookup.
    /// </summary>
    /// <param name="objectName">The name or candidate name of a database object.</param>
    /// <returns>A qualified database object name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyObjectName(Identifier objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        var server = objectName.Server ?? IdentifierDefaults.Server;
        var database = objectName.Database ?? IdentifierDefaults.Database;
        var schema = objectName.Schema ?? IdentifierDefaults.Schema;

        return Identifier.CreateQualifiedIdentifier(server, database, schema, objectName.LocalName);
    }

    /// <summary>
    /// Builds a lookup of database objects, keyed by their qualified names.
    /// </summary>
    /// <typeparam name="T">The type of database object to index.</typeparam>
    /// <param name="objects">Database objects.</param>
    /// <returns>A lookup of database objects, keyed by qualified name.</returns>
    private FrozenDictionary<Identifier, T> BuildLookup<T>(IReadOnlyCollection<T> objects) where T : IDatabaseEntity
    {
        var result = new Dictionary<Identifier, T>(objects.Count);

        // when names collide the first object encountered takes precedence
        foreach (var obj in objects)
            result.TryAdd(QualifyObjectName(obj.Name), obj);

        return result.ToFrozenDictionary();
    }

    /// <summary>
    /// Attempts to retrieve a database object.
    /// </summary>
    /// <typeparam name="T">The type of database object to retrieve.</typeparam>
    /// <param name="objectsByName">Database objects, keyed by their qualified names.</param>
    /// <param name="objectName">The name of the database object to retrieve.</param>
    /// <returns>An option type with a database object, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectsByName"/> or <paramref name="objectName"/> is <see langword="null" />.</exception>
    protected OptionAsync<T> GetResolvedObject<T>(IReadOnlyDictionary<Identifier, T> objectsByName, Identifier objectName) where T : IDatabaseEntity
    {
        ArgumentNullException.ThrowIfNull(objectsByName);
        ArgumentNullException.ThrowIfNull(objectName);

        return IdentifierResolver
            .GetResolutionOrder(objectName)
            .Select(name => objectsByName.TryGetValue(QualifyObjectName(name), out var obj)
                ? Option<T>.Some(obj)
                : Option<T>.None)
            .FirstSome()
            .ToAsync();
    }

    /// <summary>
    /// Enumerates all of the database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables(CancellationToken cancellationToken = default) => Tables.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all of the database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database tables.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default) => Task.FromResult(Tables);

    /// <summary>
    /// Retrieves a database table by its name.
    /// </summary>
    /// <param name="tableName">The name of the table to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database table, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetResolvedObject(_tablesByName, tableName);
    }

    /// <summary>
    /// Enumerates all of the database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> EnumerateAllViews(CancellationToken cancellationToken = default) => Views.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all of the database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database views.</returns>
    public Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default) => Task.FromResult(Views);

    /// <summary>
    /// Retrieves a database view by its name.
    /// </summary>
    /// <param name="viewName">The name of the view to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database view, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetResolvedObject(_viewsByName, viewName);
    }

    /// <summary>
    /// Enumerates all of the database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> EnumerateAllSequences(CancellationToken cancellationToken = default) => Sequences.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all of the database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database sequences.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default) => Task.FromResult(Sequences);

    /// <summary>
    /// Retrieves a database sequence by its name.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database sequence, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return GetResolvedObject(_sequencesByName, sequenceName);
    }

    /// <summary>
    /// Enumerates all of the database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> EnumerateAllSynonyms(CancellationToken cancellationToken = default) => Synonyms.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all of the database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database synonyms.</returns>
    public Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default) => Task.FromResult(Synonyms);

    /// <summary>
    /// Retrieves a database synonym by its name.
    /// </summary>
    /// <param name="synonymName">The name of the synonym to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database synonym, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return GetResolvedObject(_synonymsByName, synonymName);
    }

    /// <summary>
    /// Enumerates all of the database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default) => Routines.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all of the database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database routines.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default) => Task.FromResult(Routines);

    /// <summary>
    /// Retrieves a database routine by its name.
    /// </summary>
    /// <param name="routineName">The name of the routine to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database routine, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return GetResolvedObject(_routinesByName, routineName);
    }
}
