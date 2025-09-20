using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Oracle.Comments;

/// <summary>
/// A comment provider for Oracle database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public class OracleDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <see langword="null" />.</exception>
    public OracleDatabaseCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        _tableCommentProvider = new OracleTableCommentProvider(connection, identifierDefaults, identifierResolver);
        _viewCommentProvider = new OracleViewCommentProvider(connection, identifierDefaults, identifierResolver);
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
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
    {
        return _tableCommentProvider.GetAllTableComments(cancellationToken);
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
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseViewComments}" /> instance which holds the value of the view's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database views.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An collection of database view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default)
    {
        return _viewCommentProvider.GetAllViewComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database views.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An collection of database view comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments2(CancellationToken cancellationToken = default)
    {
        return _viewCommentProvider.GetAllViewComments2(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSequenceComments}" /> instance which holds the value of the sequence's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return SequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> EnumerateAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return SequenceCommentProvider.EnumerateAllSequenceComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments2(CancellationToken cancellationToken = default)
    {
        return SequenceCommentProvider.GetAllSequenceComments2(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database synonym.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comments object result in the some state, if found, none otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return SynonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
    }

    /// <summary>
    /// Enumerates comments for all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database synonyms comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> EnumerateAllSynonymComments(CancellationToken cancellationToken = default)
    {
        return SynonymCommentProvider.EnumerateAllSynonymComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database synonyms comments.</returns>
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

        return RoutineCommentProvider.GetRoutineComments(routineName, cancellationToken);
    }

    /// <summary>
    /// Enumerates all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> EnumerateAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return RoutineCommentProvider.EnumerateAllRoutineComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments2(CancellationToken cancellationToken = default)
    {
        return RoutineCommentProvider.GetAllRoutineComments2(cancellationToken);
    }

    private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
    private readonly IDatabaseViewCommentProvider _viewCommentProvider;

    private static readonly IDatabaseSequenceCommentProvider SequenceCommentProvider = new EmptyDatabaseSequenceCommentProvider();
    private static readonly IDatabaseSynonymCommentProvider SynonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
    private static readonly IDatabaseRoutineCommentProvider RoutineCommentProvider = new EmptyDatabaseRoutineCommentProvider();
}