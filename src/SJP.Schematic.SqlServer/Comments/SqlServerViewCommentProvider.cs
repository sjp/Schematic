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
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
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
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
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
        protected virtual string CommentProperty { get; } = "MS_Description";

        /// <summary>
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllViewCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var viewName = QualifyViewName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var comments = g.ToList();

                    var viewComment = GetFirstCommentByType(comments, Constants.View);
                    var columnComments = GetCommentLookupByType(comments, Constants.Column);

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

            viewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved name of a view in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private static readonly string ViewNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(QualifiedName.SchemaName) }], name as [{ nameof(QualifiedName.ObjectName) }]
from sys.views
where schema_id = schema_id(@SchemaName) and name = @ViewName
    and is_ms_shipped = 0";

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
            var commentsData = await Connection.QueryAsync<CommentsData>(
                ViewCommentsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var viewComment = GetFirstCommentByType(commentsData, Constants.View);
            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

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
    SCHEMA_NAME(v.schema_id) as [{ nameof(CommentsData.SchemaName) }],
    v.name as [{ nameof(CommentsData.TableName) }],
    'VIEW' as [{ nameof(CommentsData.ObjectType) }],
    v.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.views v
left join sys.extended_properties ep on v.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where v.is_ms_shipped = 0

union

-- columns
select
    SCHEMA_NAME(v.schema_id) as [{ nameof(CommentsData.SchemaName) }],
    v.name as [{ nameof(CommentsData.TableName) }],
    'COLUMN' as [{ nameof(CommentsData.ObjectType) }],
    c.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.extended_properties ep on v.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @CommentProperty
where v.is_ms_shipped = 0
) wrapped order by wrapped.SchemaName, wrapped.TableName
";

        /// <summary>
        /// Gets a query that retrieves view comments for a single view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private static readonly string ViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as [{ nameof(CommentsData.ObjectType) }],
    v.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.views v
left join sys.extended_properties ep on v.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where v.schema_id = SCHEMA_ID(@SchemaName) and v.name = @ViewName and v.is_ms_shipped = 0

union

-- columns
select
    'COLUMN' as [{ nameof(CommentsData.ObjectType) }],
    c.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.extended_properties ep on v.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @CommentProperty
where v.schema_id = SCHEMA_ID(@SchemaName) and v.name = @ViewName and v.is_ms_shipped = 0
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => c.ObjectType == objectType)
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => c.ObjectType == objectType)
                .Select(c => new KeyValuePair<Identifier, Option<string>>(
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
