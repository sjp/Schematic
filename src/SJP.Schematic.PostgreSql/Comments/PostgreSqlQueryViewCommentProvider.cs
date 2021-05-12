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
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql.Comments
{
    /// <summary>
    /// A database view comment provider for PostgreSQL. Does not provide comments for materialized views.
    /// </summary>
    /// <seealso cref="IDatabaseViewCommentProvider" />
    public class PostgreSqlQueryViewCommentProvider : IDatabaseViewCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlQueryViewCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlQueryViewCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
                    var qualifiedName = QualifyViewName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.ViewName));

                    var commentData = g.Select(r => new CommentData
                    {
                        ObjectName = r.ObjectName,
                        Comment = r.Comment,
                        ObjectType = r.ObjectType
                    }).ToList();

                    var viewComment = GetFirstCommentByType(commentData, Constants.View);
                    var columnComments = GetCommentLookupByType(commentData, Constants.Column);

                    return new DatabaseViewComments(qualifiedName, viewComment, columnComments);
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
select schemaname as ""{ nameof(GetViewNameQueryResult.SchemaName) }"", viewname as ""{ nameof(GetViewNameQueryResult.ViewName) }""
from pg_catalog.pg_views
where schemaname = @{ nameof(GetViewNameQuery.SchemaName) } and viewname = @{ nameof(GetViewNameQuery.ViewName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

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
            var result = await Connection.QueryAsync<GetViewCommentsQueryResult>(
                ViewCommentsQuery,
                new GetViewCommentsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ObjectName = r.ObjectName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var viewComment = GetFirstCommentByType(commentData, Constants.View);
            var columnComments = GetCommentLookupByType(commentData, Constants.Column);

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
    n.nspname as ""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"",
    c.relname as ""{ nameof(GetAllViewCommentsQueryResult.ViewName) }"",
    'VIEW' as ""{ nameof(GetAllViewCommentsQueryResult.ObjectType) }"",
    c.relname as ""{ nameof(GetAllViewCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetAllViewCommentsQueryResult.Comment) }""
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select
    n.nspname as ""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"",
    c.relname as ""{ nameof(GetAllViewCommentsQueryResult.ViewName) }"",
    'COLUMN' as ""{ nameof(GetAllViewCommentsQueryResult.ObjectType) }"",
    a.attname as ""{ nameof(GetAllViewCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetAllViewCommentsQueryResult.Comment) }""
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped
) wrapped order by wrapped.""{ nameof(GetAllViewCommentsQueryResult.SchemaName) }"", wrapped.""{ nameof(GetAllViewCommentsQueryResult.ViewName) }""
";

        /// <summary>
        /// Gets a query that retrieves view comments for a single view.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private static readonly string ViewCommentsQuerySql = @$"
-- view
select
    'VIEW' as ""{ nameof(GetViewCommentsQueryResult.ObjectType) }"",
    c.relname as ""{ nameof(GetViewCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetViewCommentsQueryResult.Comment) }""
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where n.nspname = @{ nameof(GetViewCommentsQuery.SchemaName) } and c.relname = @{ nameof(GetViewCommentsQuery.ViewName) }
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select
    'COLUMN' as ""{ nameof(GetViewCommentsQueryResult.ObjectType) }"",
    a.attname as ""{ nameof(GetViewCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetViewCommentsQueryResult.Comment) }""
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where n.nspname = @{ nameof(GetViewCommentsQuery.SchemaName) } and c.relname = @{ nameof(GetViewCommentsQuery.ViewName) }
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<CommentData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

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
            public string? ObjectName { get; init; }

            public string? ObjectType { get; init; }

            public string? Comment { get; init; }
        }
    }
}
