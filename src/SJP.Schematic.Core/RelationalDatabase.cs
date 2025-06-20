using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Not intended to be used directly. Used to store and access database objects in memory.
/// </summary>
public class RelationalDatabase : IRelationalDatabase
{
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
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public RelationalDatabase(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IEnumerable<IRelationalDatabaseTable> tables,
        IEnumerable<IDatabaseView> views,
        IEnumerable<IDatabaseSequence> sequences,
        IEnumerable<IDatabaseSynonym> synonyms,
        IEnumerable<IDatabaseRoutine> routines
    )
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        Tables = tables?.ToList() ?? throw new ArgumentNullException(nameof(tables));
        Views = views?.ToList() ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences?.ToList() ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms?.ToList() ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines?.ToList() ?? throw new ArgumentNullException(nameof(routines));
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
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <c>null</c>.</exception>
    protected Identifier QualifyObjectName(Identifier objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);

        var schema = objectName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, objectName.LocalName);
    }

    /// <summary>
    /// Attempts to retrieve a database object.
    /// </summary>
    /// <typeparam name="T">The type of database object to retrieve.</typeparam>
    /// <param name="objects">Database objects.</param>
    /// <param name="objectName">The name of the database object to retrieve.</param>
    /// <returns>An option type with a database object, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <c>null</c>.</exception>
    protected OptionAsync<T> GetResolvedObject<T>(IReadOnlyCollection<T> objects, Identifier objectName) where T : IDatabaseEntity
    {
        ArgumentNullException.ThrowIfNull(objectName);

        var objectsByName = objects.ToLookup(o => QualifyObjectName(o.Name));
        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(objectName)
            .Select(QualifyObjectName);

        return resolvedNames
            .Select(name =>
            {
                var obj = objectsByName[name].FirstOrDefault();
                return obj != null
                    ? Option<T>.Some(obj)
                    : Option<T>.None;
            })
            .FirstSome()
            .ToAsync();
    }

    /// <summary>
    /// Retrieves all of the database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => Tables.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves a database table by its name.
    /// </summary>
    /// <param name="tableName">The name of the table to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database table, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetResolvedObject(Tables, tableName);
    }

    /// <summary>
    /// Retrieves all of the database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default) => Views.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves a database view by its name.
    /// </summary>
    /// <param name="viewName">The name of the view to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database view, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetResolvedObject(Views, viewName);
    }

    /// <summary>
    /// Retrieves all of the database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default) => Sequences.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves a database sequence by its name.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database sequence, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return GetResolvedObject(Sequences, sequenceName);
    }

    /// <summary>
    /// Retrieves all of the database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default) => Synonyms.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves a database synonym by its name.
    /// </summary>
    /// <param name="synonymName">The name of the synonym to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database synonym, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return GetResolvedObject(Synonyms, synonymName);
    }

    /// <summary>
    /// Retrieves all of the database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>A collection of database routines.</returns>
    public IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines(CancellationToken cancellationToken = default) => Routines.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves a database routine by its name.
    /// </summary>
    /// <param name="routineName">The name of the routine to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token. Unused.</param>
    /// <returns>An option type with a database routine, if available, otherwise an option type in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return GetResolvedObject(Routines, routineName);
    }

    /// <summary>
    /// Snapshots a relational database. Preserves the same behaviour, but enables querying in-memory, avoiding further database calls.
    /// </summary>
    /// <param name="database">A relational database.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A relational database with the same data as <paramref name="database"/>, but serialized into an in-memory copy.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public static Task<IRelationalDatabase> SnapshotAsync(IRelationalDatabase database, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        return SnapshotAsyncCore(database, identifierResolver, cancellationToken);
    }

    private static async Task<IRelationalDatabase> SnapshotAsyncCore(IRelationalDatabase database, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken)
    {
        var (
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await TaskUtilities.WhenAll(
            database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask()
        ).ConfigureAwait(false);

        return new RelationalDatabase(
            database.IdentifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );
    }
}