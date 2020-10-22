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

namespace SJP.Schematic.Oracle.Comments
{
    /// <summary>
    /// A materialized view comment provider for Oracle databases.
    /// </summary>
    /// <seealso cref="IDatabaseViewCommentProvider" />
    public class OracleMaterializedViewCommentProvider : IDatabaseViewCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleMaterializedViewCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleMaterializedViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        /// Retrieves all materialized view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of materialized view comments.</returns>
        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<TableCommentsData>(AllViewCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.ObjectName })
                .Select(g =>
                {
                    var viewName = QualifyViewName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.ObjectName));
                    var comments = g.ToList();

                    var viewComment = GetViewComment(comments);
                    var columnComments = GetColumnComments(comments);

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
        /// Gets the resolved name of the materialized view without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="viewName">A materialized view name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a materialized view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private static readonly string ViewNameQuerySql = @$"
select mv.OWNER as ""{ nameof(QualifiedName.SchemaName) }"", mv.MVIEW_NAME as ""{ nameof(QualifiedName.ObjectName) }""
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where mv.OWNER = :SchemaName and mv.MVIEW_NAME = :ViewName
    and o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'";

        /// <summary>
        /// Retrieves comments for a particular materialized view.
        /// </summary>
        /// <param name="viewName">The name of a materialized view.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="T:LanguageExt.OptionAsync`1" /> instance which holds the value of the materialized view's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewComments(candidateViewName, cancellationToken);
        }

        /// <summary>
        /// Retrieves a materialized view's comments.
        /// </summary>
        /// <param name="viewName">A materialized view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for a materialized view, if available.</returns>
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
            IEnumerable<TableCommentsData> commentsData;
            if (string.Equals(viewName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)) // fast path
            {
                commentsData = await Connection.QueryAsync<TableCommentsData>(
                    UserViewCommentsQuery,
                    new { ViewName = viewName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                commentsData = await Connection.QueryAsync<TableCommentsData>(
                    ViewCommentsQuery,
                    new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);
            }

            var viewComment = GetViewComment(commentsData);
            var columnComments = GetColumnComments(commentsData);

            return new DatabaseViewComments(viewName, viewComment, columnComments);
        }

        /// <summary>
        /// Gets a query that retrieves comments for all materialized views.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllViewCommentsQuery => AllViewCommentsQuerySql;

        private static readonly string AllViewCommentsQuerySql = @$"
select wrapped.* from (
-- view
select
    v.OWNER as ""{ nameof(TableCommentsData.SchemaName) }"",
    v.MVIEW_NAME as ""{ nameof(TableCommentsData.ObjectName) }"",
    'VIEW' as ""{ nameof(TableCommentsData.ObjectType) }"",
    NULL as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEW_COMMENTS c on v.OWNER = c.OWNER and v.MVIEW_NAME = c.MVIEW_NAME
where o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select
    v.OWNER as ""{ nameof(TableCommentsData.SchemaName) }"",
    v.MVIEW_NAME as ""{ nameof(TableCommentsData.ObjectName) }"",
    'COLUMN' as ""{ nameof(TableCommentsData.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
) wrapped order by wrapped.""{ nameof(TableCommentsData.SchemaName) }"", wrapped.""{ nameof(TableCommentsData.ObjectName) }""
";

        /// <summary>
        /// Gets a query that retrieves comments for a single materialized view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private static readonly string ViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as ""{ nameof(TableCommentsData.ObjectType) }"",
    NULL as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEW_COMMENTS c on v.OWNER = c.OWNER and v.MVIEW_NAME = c.MVIEW_NAME
where v.OWNER = :SchemaName and v.MVIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select
    'COLUMN' as ""{ nameof(TableCommentsData.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.OWNER = :SchemaName and v.MVIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'
";

        /// <summary>
        /// Gets a query that retrieves comments for a single materialized view in the user's schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserViewCommentsQuery => UserViewCommentsQuerySql;

        private static readonly string UserViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as ""{ nameof(TableCommentsData.ObjectType) }"",
    NULL as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.USER_MVIEWS v
left join SYS.USER_MVIEW_COMMENTS c on v.MVIEW_NAME = c.MVIEW_NAME
where v.MVIEW_NAME = :ViewName

union

-- columns
select
    'COLUMN' as ""{ nameof(TableCommentsData.ObjectType) }"",
    vc.COLUMN_NAME as ""{ nameof(TableCommentsData.ColumnName) }"",
    c.COMMENTS as ""{ nameof(TableCommentsData.Comment) }""
from SYS.USER_MVIEWS v
inner join SYS.USER_TAB_COLS vc on vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.MVIEW_NAME = :ViewName
";

        private static Option<string> GetViewComment(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, Constants.View, StringComparison.Ordinal))
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetColumnComments(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, Constants.Column, StringComparison.Ordinal))
                .Select(c => new KeyValuePair<Identifier, Option<string>>(
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
    }
}
