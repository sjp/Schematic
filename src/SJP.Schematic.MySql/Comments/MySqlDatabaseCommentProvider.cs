using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.MySql.Comments;

/// <summary>
/// A comment provider for MySQL database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public class MySqlDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public MySqlDatabaseCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        ArgumentNullException.ThrowIfNull(connection);

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        _tableCommentProvider = new MySqlTableCommentProvider(connection, identifierDefaults);
        _routineCommentProvider = new MySqlRoutineCommentProvider(connection, identifierDefaults);
    }

    /// <summary>
    /// Default values for identifiers in a database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Retrieves comments for a database table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> EnumerateAllTableComments(CancellationToken cancellationToken = default)
    {
        return _tableCommentProvider.EnumerateAllTableComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments2(CancellationToken cancellationToken = default)
    {
        return _tableCommentProvider.GetAllTableComments2(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database view. Always none.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comments object result in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return ViewCommentProvider.GetViewComments(viewName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database views. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> EnumerateAllViewComments(CancellationToken cancellationToken = default)
    {
        return ViewCommentProvider.EnumerateAllViewComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database views. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database view comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments2(CancellationToken cancellationToken = default)
    {
        return ViewCommentProvider.GetAllViewComments2(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database sequence. Always none.
    /// </summary>
    /// <param name="sequenceName">A sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comments object result in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return SequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database sequences. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> EnumerateAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return SequenceCommentProvider.EnumerateAllSequenceComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database sequences. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database sequence comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return SequenceCommentProvider.GetAllSequenceComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database synonym,.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comments object result in the none state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return SynonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database synonyms. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database synonym comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> EnumerateAllSynonymComments(CancellationToken cancellationToken = default)
    {
        return SynonymCommentProvider.EnumerateAllSynonymComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database synonyms. Always empty.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of database synonym comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments2(CancellationToken cancellationToken = default)
    {
        return SynonymCommentProvider.GetAllSynonymComments2(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database routine, if available.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database routine, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database routines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database routine comments, where available.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> EnumerateAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return _routineCommentProvider.EnumerateAllRoutineComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database routines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database routine comments, where available.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
    }

    private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
    private readonly IDatabaseRoutineCommentProvider _routineCommentProvider;

    private static readonly IDatabaseViewCommentProvider ViewCommentProvider = new EmptyDatabaseViewCommentProvider();
    private static readonly IDatabaseSequenceCommentProvider SequenceCommentProvider = new EmptyDatabaseSequenceCommentProvider();
    private static readonly IDatabaseSynonymCommentProvider SynonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
}