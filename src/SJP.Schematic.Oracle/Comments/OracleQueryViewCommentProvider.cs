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
    public class OracleQueryViewCommentProvider : IDatabaseViewCommentProvider
    {
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

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
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
        /// A SQL query that retrieves the resolved name of a view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

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
            IEnumerable<TableCommentsData> commentsData;
            if (viewName.Schema == IdentifierDefaults.Schema) // fast path
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

        protected virtual string AllViewCommentsQuery => AllViewCommentsQuerySql;

        private const string AllViewCommentsQuerySql = @"
select wrapped.* from (
-- view
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName, 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on v.OWNER = c.OWNER and v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName, 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.VIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
) wrapped order by wrapped.SchemaName, wrapped.ObjectName";

        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private const string ViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on v.OWNER = c.OWNER and v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.VIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'
";

        protected virtual string UserViewCommentsQuery => UserViewCommentsQuerySql;

        private const string UserViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.USER_VIEWS v
left join SYS.USER_TAB_COMMENTS c on v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.VIEW_NAME = :ViewName
union

-- columns
select 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.USER_VIEWS v
inner join SYS.USER_TAB_COLS vc on vc.TABLE_NAME = v.VIEW_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.VIEW_NAME = :ViewName
";

        private static Option<string> GetViewComment(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => c.ObjectType == Constants.View)
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetColumnComments(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => c.ObjectType == Constants.Column)
                .Select(c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ColumnName),
                    !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
                ))
                .ToDictionary(c => c.Key, c => c.Value);
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
