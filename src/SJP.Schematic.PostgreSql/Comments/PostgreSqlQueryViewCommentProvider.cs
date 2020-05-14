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

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlQueryViewCommentProvider : IDatabaseViewCommentProvider
    {
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

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// Retrieves all database view comments defined within a database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of view comments.</returns>
        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<ViewCommentsData>(AllViewCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.ViewName })
                .Select(g =>
                {
                    var qualifiedName = QualifyViewName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.ViewName));
                    var commentsData = g.ToList();

                    var viewComment = GetFirstCommentByType(commentsData, Constants.View);
                    var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

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
select schemaname as SchemaName, viewname as ObjectName
from pg_catalog.pg_views
where schemaname = @SchemaName and viewname = @ViewName
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
            var commentsData = await Connection.QueryAsync<ViewCommentsData>(
                ViewCommentsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var viewComment = GetFirstCommentByType(commentsData, Constants.View);
            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

            return new DatabaseViewComments(viewName, viewComment, columnComments);
        }

        protected virtual string AllViewCommentsQuery => AllViewCommentsQuerySql;

        private const string AllViewCommentsQuerySql = @"
select wrapped.* from (
-- view
select n.nspname as SchemaName, c.relname as ViewName, 'VIEW' as ObjectType, c.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select n.nspname as SchemaName, c.relname as ViewName, 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped
) wrapped order by wrapped.SchemaName, wrapped.ViewName
";

        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private const string ViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, c.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where n.nspname = @SchemaName and c.relname = @ViewName
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where n.nspname = @SchemaName and c.relname = @ViewName
    and c.relkind = 'v' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped
";

        private static Option<string> GetFirstCommentByType(IEnumerable<ViewCommentsData> commentsData, string objectType)
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

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<ViewCommentsData> commentsData, string objectType)
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
