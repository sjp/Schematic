using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// A database object comment provider that always returns no results.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public sealed class EmptyRelationalDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyRelationalDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public EmptyRelationalDatabaseCommentProvider(IIdentifierDefaults identifierDefaults)
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// Default values for identifiers in a database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database sequence comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database synonym comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of synonym comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
    {
        return _synonymCommentProvider.GetAllSynonymComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database table comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of table comments.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
    {
        return _tableCommentProvider.GetAllTableComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An empty collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default)
    {
        return _viewCommentProvider.GetAllViewComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database routine.
    /// </summary>
    /// <param name="routineName">The name of a database routine.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database synonym.
    /// </summary>
    /// <param name="synonymName">The name of a database synonym.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return _synonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database table.
    /// </summary>
    /// <param name="tableName">The name of a database table.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which is always none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
    }

    private static readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider = new EmptyRelationalDatabaseTableCommentProvider();
    private static readonly IDatabaseViewCommentProvider _viewCommentProvider = new EmptyDatabaseViewCommentProvider();
    private static readonly IDatabaseSequenceCommentProvider _sequenceCommentProvider = new EmptyDatabaseSequenceCommentProvider();
    private static readonly IDatabaseSynonymCommentProvider _synonymCommentProvider = new EmptyDatabaseSynonymCommentProvider();
    private static readonly IDatabaseRoutineCommentProvider _routineCommentProvider = new EmptyDatabaseRoutineCommentProvider();
}