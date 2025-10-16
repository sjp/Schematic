using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer.Comments;

/// <summary>
/// A comment provider for SQL Server database views.
/// </summary>
/// <seealso cref="IDatabaseViewCommentProvider" />
public class SqlServerViewCommentProvider : IDatabaseViewCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerViewCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Retrieves the extended property name used to store comments on an object.
    /// </summary>
    /// <value>The comment property name.</value>
    protected string CommentProperty { get; } = "MS_Description";

    /// <summary>
    /// Enumerates all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> EnumerateAllViewComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllViewNames.Result>(GetAllViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .SelectAwait(viewName => LoadViewCommentsAsyncCore(viewName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
    {
        var viewNames = await Connection.QueryEnumerableAsync<GetAllViewNames.Result>(GetAllViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .ToListAsync(cancellationToken);

        return await viewNames
            .Select(viewName => LoadViewCommentsAsyncCore(viewName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        viewName = QualifyViewName(viewName);
        var qualifiedViewName = Connection.QueryFirstOrNone(
            GetViewName.Sql,
            new GetViewName.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ViewName));
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

        var candidateViewName = QualifyViewName(viewName);
        return LoadViewComments(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Retrieves a view's comments.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for a view, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseViewComments> LoadViewComments(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseViewComments> LoadViewCommentsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var queryResult = await Connection.QueryAsync(
            Queries.GetViewComments.Sql,
            new GetViewComments.Query
            {
                SchemaName = viewName.Schema!,
                ViewName = viewName.LocalName,
                CommentProperty = CommentProperty,
            },
            cancellationToken
        );

        var commentData = queryResult.Select(r => new CommentData
        {
            ObjectName = r.ObjectName,
            ObjectType = r.ObjectType,
            Comment = r.Comment,
        }).ToList();

        var viewComment = GetFirstCommentByType(commentData, Constants.View);
        var columnComments = GetCommentLookupByType(commentData, Constants.Column);

        return new DatabaseViewComments(viewName, viewComment, columnComments);
    }

    private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        return commentsData
            .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
            .FirstOrDefault();
    }

    private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        return commentsData
            .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
            .Select(static c => new KeyValuePair<Identifier, Option<string>>(
                Identifier.CreateQualifiedIdentifier(c.ObjectName),
                !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
            ))
            .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
    }

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }

    private static class Constants
    {
        public const string View = "VIEW";

        public const string Column = "COLUMN";
    }

    private sealed record CommentData
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }
}