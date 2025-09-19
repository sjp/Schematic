using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A database object comment provider that always returns no results.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public class RelationalDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
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
    public RelationalDatabaseCommentProvider(
        IIdentifierDefaults identifierDefaults,
        IIdentifierResolutionStrategy identifierResolver,
        IEnumerable<IRelationalDatabaseTableComments> tableComments,
        IEnumerable<IDatabaseViewComments> viewComments,
        IEnumerable<IDatabaseSequenceComments> sequenceComments,
        IEnumerable<IDatabaseSynonymComments> synonymComments,
        IEnumerable<IDatabaseRoutineComments> routineComments
    )
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        TableComments = tableComments?.ToList() ?? throw new ArgumentNullException(nameof(tableComments));
        ViewComments = viewComments?.ToList() ?? throw new ArgumentNullException(nameof(viewComments));
        SequenceComments = sequenceComments?.ToList() ?? throw new ArgumentNullException(nameof(sequenceComments));
        SynonymComments = synonymComments?.ToList() ?? throw new ArgumentNullException(nameof(synonymComments));
        RoutineComments = routineComments?.ToList() ?? throw new ArgumentNullException(nameof(routineComments));
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
    /// Enumerates all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default) => RoutineComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default) => SequenceComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of synonym comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default) => SynonymComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default) => TableComments.ToAsyncEnumerable();

    /// <summary>
    /// Enumerates all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default) => ViewComments.ToAsyncEnumerable();

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments2(CancellationToken cancellationToken = default) => Task.FromResult(RoutineComments);

    /// <summary>
    /// Retrieves all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of sequence comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments2(CancellationToken cancellationToken = default) => Task.FromResult(SequenceComments);

    /// <summary>
    /// Retrieves all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of synonym comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments2(CancellationToken cancellationToken = default) => Task.FromResult(SynonymComments);

    /// <summary>
    /// Retrieves all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of table comments.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments2(CancellationToken cancellationToken = default) => Task.FromResult(TableComments);

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments2(CancellationToken cancellationToken = default) => Task.FromResult(ViewComments);

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

        var routineNames = IdentifierResolver
            .GetResolutionOrder(routineName)
            .Select(QualifyObjectName);

        return routineNames
            .Select(name =>
            {
                var routineComments = RoutineComments.FirstOrDefault(o => QualifyObjectName(o.RoutineName) == name);
                return routineComments != null
                    ? Option<IDatabaseRoutineComments>.Some(routineComments)
                    : Option<IDatabaseRoutineComments>.None;
            })
            .FirstSome()
            .ToAsync();
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

        var sequenceNames = IdentifierResolver
            .GetResolutionOrder(sequenceName)
            .Select(QualifyObjectName);

        return sequenceNames
            .Select(name =>
            {
                var sequenceComments = SequenceComments.FirstOrDefault(o => QualifyObjectName(o.SequenceName) == name);
                return sequenceComments != null
                    ? Option<IDatabaseSequenceComments>.Some(sequenceComments)
                    : Option<IDatabaseSequenceComments>.None;
            })
            .FirstSome()
            .ToAsync();
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

        var synonymNames = IdentifierResolver
            .GetResolutionOrder(synonymName)
            .Select(QualifyObjectName);

        return synonymNames
            .Select(name =>
            {
                var synonymComments = SynonymComments.FirstOrDefault(o => QualifyObjectName(o.SynonymName) == name);
                return synonymComments != null
                    ? Option<IDatabaseSynonymComments>.Some(synonymComments)
                    : Option<IDatabaseSynonymComments>.None;
            })
            .FirstSome()
            .ToAsync();
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

        var tableNames = IdentifierResolver
            .GetResolutionOrder(tableName)
            .Select(QualifyObjectName);

        return tableNames
            .Select(name =>
            {
                var tableComments = TableComments.FirstOrDefault(o => QualifyObjectName(o.TableName) == name);
                return tableComments != null
                    ? Option<IRelationalDatabaseTableComments>.Some(tableComments)
                    : Option<IRelationalDatabaseTableComments>.None;
            })
            .FirstSome()
            .ToAsync();
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

        var viewNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyObjectName);

        return viewNames
            .Select(name =>
            {
                var viewComments = ViewComments.FirstOrDefault(o => QualifyObjectName(o.ViewName) == name);
                return viewComments != null
                    ? Option<IDatabaseViewComments>.Some(viewComments)
                    : Option<IDatabaseViewComments>.None;
            })
            .FirstSome()
            .ToAsync();
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

        var schema = objectName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, objectName.LocalName);
    }
}