using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Comments;

/// <summary>
/// A view comment provider for Oracle.
/// </summary>
/// <seealso cref="IDatabaseViewCommentProvider" />
public class OracleViewCommentProvider : IDatabaseViewCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleViewCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(identifierDefaults);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        _connectionFactory = connection;
        _queryViewCommentProvider = new OracleQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);
        _materializedViewCommentProvider = new OracleMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);
        QueryViewCommentProvider = _queryViewCommentProvider;
        MaterializedViewCommentProvider = _materializedViewCommentProvider;
    }

    /// <summary>
    /// Gets a query view comment provider that does not return any materialized view comments.
    /// </summary>
    /// <value>A query view comment provider.</value>
    protected IDatabaseViewCommentProvider QueryViewCommentProvider { get; }

    /// <summary>
    /// Gets a materialized view comment provider, that does not return any simple query view comments.
    /// </summary>
    /// <value>A materialized view comment provider.</value>
    protected IDatabaseViewCommentProvider MaterializedViewCommentProvider { get; }

    /// <summary>
    /// Enumerates all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public async IAsyncEnumerable<IDatabaseViewComments> EnumerateAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (queryViewNames, materializedViewNames) = await (
            _queryViewCommentProvider.GetAllViewNamesAsync(cancellationToken),
            _materializedViewCommentProvider.GetAllViewNamesAsync(cancellationToken)
        ).WhenAll();

        // order the names rather than the loaded comments, so that each view's comments can be returned as soon as they are loaded
        var viewNames = queryViewNames
            .Select(static name => (Name: name, IsMaterialized: false))
            .Concat(materializedViewNames.Select(static name => (Name: name, IsMaterialized: true)))
            .OrderBy(static v => v.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static v => v.Name.LocalName, StringComparer.Ordinal);

        var comments = viewNames.SelectOrderedPrefetchAsync(LoadViewCommentsAsyncCore, Math.Max(1, _connectionFactory.MaxConcurrentQueries), cancellationToken);

        await foreach (var comment in comments.WithCancellation(cancellationToken))
            yield return comment;
    }

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
    {
        var (queryViewComments, materializedViewComments) = await (
            QueryViewCommentProvider.GetAllViewComments(cancellationToken),
            MaterializedViewCommentProvider.GetAllViewComments(cancellationToken)
        ).WhenAll();

        return queryViewComments
            .Concat(materializedViewComments)
            .OrderBy(static v => v.ViewName.Schema, StringComparer.Ordinal)
            .ThenBy(static v => v.ViewName.LocalName, StringComparer.Ordinal)
            .ToList();
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

        return QueryViewCommentProvider.GetViewComments(viewName, cancellationToken)
            .OrElse(() => MaterializedViewCommentProvider.GetViewComments(viewName, cancellationToken));
    }

    private Task<IDatabaseViewComments> LoadViewCommentsAsyncCore((Identifier Name, bool IsMaterialized) view, CancellationToken cancellationToken)
    {
        return view.IsMaterialized
            ? _materializedViewCommentProvider.LoadViewCommentsAsyncCore(view.Name, cancellationToken)
            : _queryViewCommentProvider.LoadViewCommentsAsyncCore(view.Name, cancellationToken);
    }

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly OracleQueryViewCommentProvider _queryViewCommentProvider;
    private readonly OracleMaterializedViewCommentProvider _materializedViewCommentProvider;
}