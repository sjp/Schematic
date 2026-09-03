using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A database object comment provider that serves comments from in-memory collections supplied on construction.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public class RelationalDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
    private readonly FrozenDictionary<Identifier, IDatabaseSchemaComments> _schemaCommentsByName;
    private readonly FrozenDictionary<Identifier, IRelationalDatabaseTableComments> _tableCommentsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseViewComments> _viewCommentsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseSequenceComments> _sequenceCommentsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseSynonymComments> _synonymCommentsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseRoutineComments> _routineCommentsByName;
    private readonly FrozenDictionary<Identifier, IDatabaseUserDefinedTypeComments> _userDefinedTypeCommentsByName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="tableComments">A collection of database table comment information.</param>
    /// <param name="viewComments">A collection of database view comment information.</param>
    /// <param name="sequenceComments">A collection of database sequence comment information.</param>
    /// <param name="synonymComments">A collection of database synonym comment information.</param>
    /// <param name="routineComments">A collection of database routine comment information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/>, <paramref name="identifierResolver"/>, <paramref name="tableComments"/>, <paramref name="viewComments"/>, <paramref name="sequenceComments"/>, <paramref name="synonymComments"/> or <paramref name="routineComments"/> is <see langword="null" />.</exception>
    public RelationalDatabaseCommentProvider(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IEnumerable<IRelationalDatabaseTableComments> tableComments,
        IEnumerable<IDatabaseViewComments> viewComments,
        IEnumerable<IDatabaseSequenceComments> sequenceComments,
        IEnumerable<IDatabaseSynonymComments> synonymComments,
        IEnumerable<IDatabaseRoutineComments> routineComments
    )
        : this(identifierDefaults, identifierResolver, tableComments, viewComments, sequenceComments, synonymComments, routineComments, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="tableComments">A collection of database table comment information.</param>
    /// <param name="viewComments">A collection of database view comment information.</param>
    /// <param name="sequenceComments">A collection of database sequence comment information.</param>
    /// <param name="synonymComments">A collection of database synonym comment information.</param>
    /// <param name="routineComments">A collection of database routine comment information.</param>
    /// <param name="userDefinedTypeComments">A collection of database user-defined type comment information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/>, <paramref name="identifierResolver"/>, <paramref name="tableComments"/>, <paramref name="viewComments"/>, <paramref name="sequenceComments"/>, <paramref name="synonymComments"/>, <paramref name="routineComments"/> or <paramref name="userDefinedTypeComments"/> is <see langword="null" />.</exception>
    public RelationalDatabaseCommentProvider(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IEnumerable<IRelationalDatabaseTableComments> tableComments,
        IEnumerable<IDatabaseViewComments> viewComments,
        IEnumerable<IDatabaseSequenceComments> sequenceComments,
        IEnumerable<IDatabaseSynonymComments> synonymComments,
        IEnumerable<IDatabaseRoutineComments> routineComments,
        IEnumerable<IDatabaseUserDefinedTypeComments> userDefinedTypeComments
    )
        : this(identifierDefaults, identifierResolver, tableComments, viewComments, sequenceComments, synonymComments, routineComments, userDefinedTypeComments, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver to use when an object cannot be found using the given name.</param>
    /// <param name="tableComments">A collection of database table comment information.</param>
    /// <param name="viewComments">A collection of database view comment information.</param>
    /// <param name="sequenceComments">A collection of database sequence comment information.</param>
    /// <param name="synonymComments">A collection of database synonym comment information.</param>
    /// <param name="routineComments">A collection of database routine comment information.</param>
    /// <param name="userDefinedTypeComments">A collection of database user-defined type comment information.</param>
    /// <param name="schemaComments">A collection of database schema comment information.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/>, <paramref name="identifierResolver"/>, <paramref name="tableComments"/>, <paramref name="viewComments"/>, <paramref name="sequenceComments"/>, <paramref name="synonymComments"/>, <paramref name="routineComments"/>, <paramref name="userDefinedTypeComments"/> or <paramref name="schemaComments"/> is <see langword="null" />.</exception>
    public RelationalDatabaseCommentProvider(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IEnumerable<IRelationalDatabaseTableComments> tableComments,
        IEnumerable<IDatabaseViewComments> viewComments,
        IEnumerable<IDatabaseSequenceComments> sequenceComments,
        IEnumerable<IDatabaseSynonymComments> synonymComments,
        IEnumerable<IDatabaseRoutineComments> routineComments,
        IEnumerable<IDatabaseUserDefinedTypeComments> userDefinedTypeComments,
        IEnumerable<IDatabaseSchemaComments> schemaComments
    )
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        TableComments = tableComments?.ToList() ?? throw new ArgumentNullException(nameof(tableComments));
        ViewComments = viewComments?.ToList() ?? throw new ArgumentNullException(nameof(viewComments));
        SequenceComments = sequenceComments?.ToList() ?? throw new ArgumentNullException(nameof(sequenceComments));
        SynonymComments = synonymComments?.ToList() ?? throw new ArgumentNullException(nameof(synonymComments));
        RoutineComments = routineComments?.ToList() ?? throw new ArgumentNullException(nameof(routineComments));
        UserDefinedTypeComments = userDefinedTypeComments?.ToList() ?? throw new ArgumentNullException(nameof(userDefinedTypeComments));
        SchemaComments = schemaComments?.ToList() ?? throw new ArgumentNullException(nameof(schemaComments));

        _tableCommentsByName = BuildLookup(TableComments, static c => c.TableName);
        _viewCommentsByName = BuildLookup(ViewComments, static c => c.ViewName);
        _sequenceCommentsByName = BuildLookup(SequenceComments, static c => c.SequenceName);
        _synonymCommentsByName = BuildLookup(SynonymComments, static c => c.SynonymName);
        _routineCommentsByName = BuildLookup(RoutineComments, static c => c.RoutineName);
        _userDefinedTypeCommentsByName = BuildLookup(UserDefinedTypeComments, static c => c.TypeName);
        _schemaCommentsByName = BuildSchemaLookup(SchemaComments);
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
    /// An in-memory collection of all database schema comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseSchemaComments> SchemaComments { get; }

    /// <summary>
    /// An in-memory collection of all database table comment information.
    /// </summary>
    protected IReadOnlyCollection<IRelationalDatabaseTableComments> TableComments { get; }

    /// <summary>
    /// An in-memory collection of all database view comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseViewComments> ViewComments { get; }

    /// <summary>
    /// An in-memory collection of all database sequence comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseSequenceComments> SequenceComments { get; }

    /// <summary>
    /// An in-memory collection of all database synonym comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseSynonymComments> SynonymComments { get; }

    /// <summary>
    /// An in-memory collection of all database routine comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseRoutineComments> RoutineComments { get; }

    /// <summary>
    /// An in-memory collection of all database user-defined type comment information.
    /// </summary>
    protected IReadOnlyCollection<IDatabaseUserDefinedTypeComments> UserDefinedTypeComments { get; }

    /// <summary>
    /// Enumerates all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> EnumerateAllRoutineComments(CancellationToken cancellationToken = default) => RoutineComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> EnumerateAllSequenceComments(CancellationToken cancellationToken = default) => SequenceComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of synonym comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> EnumerateAllSynonymComments(CancellationToken cancellationToken = default) => SynonymComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> EnumerateAllTableComments(CancellationToken cancellationToken = default) => TableComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> EnumerateAllViewComments(CancellationToken cancellationToken = default) => ViewComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-defined type comments.</returns>
    public IAsyncEnumerable<IDatabaseUserDefinedTypeComments> EnumerateAllUserDefinedTypeComments(CancellationToken cancellationToken = default) => UserDefinedTypeComments.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default) => Task.FromResult(RoutineComments);

    /// <summary>
    /// Retrieves all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of sequence comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default) => Task.FromResult(SequenceComments);

    /// <summary>
    /// Retrieves all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of synonym comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default) => Task.FromResult(SynonymComments);

    /// <summary>
    /// Retrieves all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default) => Task.FromResult(TableComments);

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default) => Task.FromResult(ViewComments);

    /// <summary>
    /// Retrieves all database user-defined type comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of user-defined type comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> GetAllUserDefinedTypeComments(CancellationToken cancellationToken = default) => Task.FromResult(UserDefinedTypeComments);

    /// <summary>
    /// Retrieves comments for a particular database routine.
    /// </summary>
    /// <param name="routineName">The name of a database routine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database routine comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return GetResolvedComments(_routineCommentsByName, routineName);
    }

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database sequence comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return GetResolvedComments(_sequenceCommentsByName, sequenceName);
    }

    /// <summary>
    /// Retrieves comments for a particular database synonym.
    /// </summary>
    /// <param name="synonymName">The name of a database synonym.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database synonym comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return GetResolvedComments(_synonymCommentsByName, synonymName);
    }

    /// <summary>
    /// Retrieves comments for a particular database table.
    /// </summary>
    /// <param name="tableName">The name of a database table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database table comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetResolvedComments(_tableCommentsByName, tableName);
    }

    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database view comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetResolvedComments(_viewCommentsByName, viewName);
    }

    /// <summary>
    /// Retrieves comments for a particular database user-defined type.
    /// </summary>
    /// <param name="typeName">The name of a database user-defined type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Database user-defined type comments in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseUserDefinedTypeComments> GetUserDefinedTypeComments(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetResolvedComments(_userDefinedTypeCommentsByName, typeName);
    }

    /// <summary>
    /// Enumerates all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. Unused.</param>
    /// <returns>A collection of schema comments.</returns>
    public IAsyncEnumerable<IDatabaseSchemaComments> EnumerateAllSchemaComments(CancellationToken cancellationToken = default) => SchemaComments.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all database schema comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. Unused.</param>
    /// <returns>A collection of schema comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSchemaComments>> GetAllSchemaComments(CancellationToken cancellationToken = default) => Task.FromResult(SchemaComments);

    /// <summary>
    /// Retrieves comments for a particular database schema.
    /// </summary>
    /// <param name="schemaName">The name of a database schema.</param>
    /// <param name="cancellationToken">The cancellation token. Unused.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSchemaComments}" /> instance which holds the value of the schema's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSchemaComments> GetSchemaComments(Identifier schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schemaName);

        return IdentifierResolver
            .GetResolutionOrder(schemaName)
            .Select(name => _schemaCommentsByName.TryGetValue(new Identifier(name.LocalName), out var comments)
                ? Option<IDatabaseSchemaComments>.Some(comments)
                : Option<IDatabaseSchemaComments>.None)
            .FirstSome()
            .ToAsync();
    }

    /// <summary>
    /// Builds a lookup of schema comments. Schemas are never themselves schema-qualified, so they
    /// are keyed by their local name alone.
    /// </summary>
    /// <param name="comments">A collection of database schema comments.</param>
    /// <returns>A lookup of schema comments, keyed by schema name.</returns>
    private static FrozenDictionary<Identifier, IDatabaseSchemaComments> BuildSchemaLookup(IReadOnlyCollection<IDatabaseSchemaComments> comments)
    {
        var result = new Dictionary<Identifier, IDatabaseSchemaComments>(comments.Count);

        // when names collide the first set of comments encountered takes precedence
        foreach (var comment in comments)
            result.TryAdd(new Identifier(comment.SchemaName.LocalName), comment);

        return result.ToFrozenDictionary();
    }

    /// <summary>
    /// Qualifies the name of a database object so that they can be compared during lookup.
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
    /// Builds a lookup of database object comments, keyed by their qualified names.
    /// </summary>
    /// <typeparam name="T">The type of comments to index.</typeparam>
    /// <param name="comments">A collection of database object comments.</param>
    /// <param name="nameSelector">Retrieves the name of the object that a set of comments describes.</param>
    /// <returns>A lookup of database object comments, keyed by qualified name.</returns>
    private FrozenDictionary<Identifier, T> BuildLookup<T>(IReadOnlyCollection<T> comments, Func<T, Identifier> nameSelector)
    {
        var result = new Dictionary<Identifier, T>(comments.Count);

        // when names collide the first set of comments encountered takes precedence
        foreach (var comment in comments)
            result.TryAdd(QualifyObjectName(nameSelector(comment)), comment);

        return result.ToFrozenDictionary();
    }

    /// <summary>
    /// Attempts to retrieve comments for a database object.
    /// </summary>
    /// <typeparam name="T">The type of comments to retrieve.</typeparam>
    /// <param name="commentsByName">Database object comments, keyed by their qualified names.</param>
    /// <param name="objectName">The name of the database object whose comments should be retrieved.</param>
    /// <returns>An option type with database object comments, if available, otherwise an option type in the none state.</returns>
    private OptionAsync<T> GetResolvedComments<T>(FrozenDictionary<Identifier, T> commentsByName, Identifier objectName)
    {
        return IdentifierResolver
            .GetResolutionOrder(objectName)
            .Select(name => commentsByName.TryGetValue(QualifyObjectName(name), out var comments)
                ? Option<T>.Some(comments)
                : Option<T>.None)
            .FirstSome()
            .ToAsync();
    }
}