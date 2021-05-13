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
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Oracle.QueryResult;

namespace SJP.Schematic.Oracle.Comments
{
    /// <summary>
    /// A view comment provider for Oracle databases. Does not provide comments for materialized views.
    /// </summary>
    /// <seealso cref="IDatabaseViewCommentProvider" />
    public class OracleQueryViewCommentProvider : IDatabaseViewCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleQueryViewCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleQueryViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
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
        /// Gets an identifier resolver that enables more relaxed matching against database object names.
        /// </summary>
        /// <value>An identifier resolver.</value>
        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<GetAllViewCommentsQueryResult>(AllViewCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(static row => new { row.SchemaName, row.ViewName })
                .Select(g =>
                {
                    var viewName = QualifyViewName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.ViewName));

                    var commentData = g.Select(r => new CommentData
                    {
                        ColumnName = r.ColumnName,
                        Comment = r.Comment,
                        ObjectType = r.ObjectType
                    }).ToList();

                    var viewComment = GetViewComment(commentData);
                    var columnComments = GetColumnComments(commentData);

                    return new DatabaseViewComments(viewName, viewComment, columnComments);
                });

            foreach (var comment in comments)
                yield return comment;
        }

        /// <summary>
        /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(viewName)
                .Select(QualifyViewName);

            return resolvedNames
                .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
                .FirstSome(cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the view without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="viewName">A view name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<GetViewNameQueryResult>(
                ViewNameQuery,
                new GetViewNameQuery { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private static readonly string ViewNameQuerySql = @$"
select v.OWNER as ""{ nameof(GetViewNameQueryResult.SchemaName) }"", v.VIEW_NAME as ""{ nameof(GetViewNameQueryResult.ViewName) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :{ nameof(GetViewNameQuery.SchemaName) } and v.VIEW_NAME = :{ nameof(GetViewNameQuery.ViewName) } and o.ORACLE_MAINTAINED <> 'Y'";

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

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewComments(candidateViewName, cancellationToken);
        }

        /// <summary>
        /// Retrieves a view's comments.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for a view, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseViewComments> LoadViewComments(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return GetResolvedViewName(candidateViewName, cancellationToken)
                .MapAsync(name => LoadViewCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseViewComments> LoadViewCommentsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            if (string.Equals(viewName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)) // fast path
                return await LoadUserViewCommentsAsyncCore(viewName, cancellationToken).ConfigureAwait(false);

            var result = await Connection.QueryAsync<GetViewCommentsQueryResult>(
                ViewCommentsQuery,
                new GetViewCommentsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ColumnName = r.ColumnName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var viewComment = GetViewComment(commentData);
            var columnComments = GetColumnComments(commentData);

            return new DatabaseViewComments(viewName, viewComment, columnComments);
        }

        private async Task<IDatabaseViewComments> LoadUserViewCommentsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var result = await Connection.QueryAsync<GetUserViewCommentsQueryResult>(
                UserViewCommentsQuery,
                new GetUserViewCommentsQuery { ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ColumnName = r.ColumnName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var viewComment = GetViewComment(commentData);
            var columnComments = GetColumnComments(commentData);

            return new DatabaseViewComments(viewName, viewComment, columnComments);
        }

        /// <summary>
        /// Gets a query that retrieves view comments for all views.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllViewCommentsQuery => AllViewCommentsQuerySql;

        private static readonly string AllViewCommentsQuerySql = @$"
select wrapped.* from (
-- view
select
    v.OWNER as ""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"",
    v.VIEW_NAME as ""{ nameof(GetAllViewCommentsQueryResult.ViewName) }"",
    'VIEW' as ""{ nameof(GetAllViewCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetAllViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetAllViewCommentsQueryResult.Comment) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on v.OWNER = c.OWNER and v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select
    v.OWNER as ""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"",
    v.VIEW_NAME as ""{ nameof(GetAllViewCommentsQueryResult.ViewName) }"",
    'COLUMN' as ""{ nameof(GetAllViewCommentsQueryResult.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(GetAllViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetAllViewCommentsQueryResult.Comment) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.VIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
) wrapped order by wrapped.""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"", wrapped.""{ nameof(GetAllViewCommentsQueryResult.ViewName) }""";

        /// <summary>
        /// Gets a query that retrieves view comments for a single view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private static readonly string ViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as ""{ nameof(GetViewCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetViewCommentsQueryResult.Comment) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on v.OWNER = c.OWNER and v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.OWNER = :{ nameof(GetViewCommentsQuery.SchemaName) } and v.VIEW_NAME = :{ nameof(GetViewCommentsQuery.ViewName) } and o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select
    'COLUMN' as ""{ nameof(GetViewCommentsQueryResult.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(GetViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetViewCommentsQueryResult.Comment) }""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.VIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.OWNER = :{ nameof(GetViewCommentsQuery.SchemaName) } and v.VIEW_NAME = :{ nameof(GetViewCommentsQuery.ViewName) } and o.ORACLE_MAINTAINED <> 'Y'
";

        /// <summary>
        /// Gets a query that retrieves view comments for a single view in the user's schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserViewCommentsQuery => UserViewCommentsQuerySql;

        private static readonly string UserViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as ""{ nameof(GetUserViewCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetUserViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetUserViewCommentsQueryResult.Comment) }""
from SYS.USER_VIEWS v
left join SYS.USER_TAB_COMMENTS c on v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.VIEW_NAME = :{ nameof(GetUserViewCommentsQuery.ViewName) }
union

-- columns
select
    'COLUMN' as ""{ nameof(GetUserViewCommentsQueryResult.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(GetUserViewCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetUserViewCommentsQueryResult.Comment) }""
from SYS.USER_VIEWS v
inner join SYS.USER_TAB_COLS vc on vc.TABLE_NAME = v.VIEW_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.VIEW_NAME = :{ nameof(GetUserViewCommentsQuery.ViewName) }
";

        private static Option<string> GetViewComment(IEnumerable<CommentData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(static c => string.Equals(c.ObjectType, Constants.View, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetColumnComments(IEnumerable<CommentData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(static c => string.Equals(c.ObjectType, Constants.Column, StringComparison.Ordinal))
                .Select(static c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ColumnName),
                    !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
                ))
                .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
        }

        /// <summary>
        /// Qualifies the name of the view.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <returns>A view name is at least as qualified as the given view name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
        }

        private static class Constants
        {
            public const string View = "VIEW";

            public const string Column = "COLUMN";
        }

        private record CommentData
        {
            public string? ColumnName { get; init; }

            public string? ObjectType { get; init; }

            public string? Comment { get; init; }
        }
    }
}
