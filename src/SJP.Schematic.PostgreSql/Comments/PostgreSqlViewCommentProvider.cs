using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.PostgreSql.Comments;

/// <summary>
/// A database view comment provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseViewCommentProvider" />
public class PostgreSqlViewCommentProvider : IDatabaseViewCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlViewCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
    public PostgreSqlViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));
        if (identifierDefaults == null)
            throw new ArgumentNullException(nameof(identifierDefaults));
        if (identifierResolver == null)
            throw new ArgumentNullException(nameof(identifierResolver));

        QueryViewCommentProvider = new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);
        MaterializedViewCommentProvider = new PostgreSqlMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);
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
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryViewsTask = QueryViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();
        var materializedViewsTask = MaterializedViewCommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).AsTask();

        await Task.WhenAll(queryViewsTask, materializedViewsTask).ConfigureAwait(false);

        var queryViews = await queryViewsTask.ConfigureAwait(false);
        var materializedViews = await materializedViewsTask.ConfigureAwait(false);

        var viewComments = queryViews
            .Concat(materializedViews)
            .OrderBy(static v => v.ViewName.Schema)
            .ThenBy(static v => v.ViewName.LocalName);

        foreach (var comment in viewComments)
            yield return comment;
    }

    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="T:LanguageExt.OptionAsync`1" /> instance which holds the value of the view's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return QueryViewCommentProvider.GetViewComments(viewName, cancellationToken)
            | MaterializedViewCommentProvider.GetViewComments(viewName, cancellationToken);
    }
}