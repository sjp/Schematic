using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlMaterializedViewCommentProvider : IDatabaseViewCommentProvider
    {
        public PostgreSqlMaterializedViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IDatabaseViewComments>();

            var allCommentsData = await Connection.QueryAsync<ViewCommentsData>(AllViewCommentsQuery, cancellationToken).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.ViewName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.ViewName);
                var qualifiedName = QualifyViewName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var viewComment = GetFirstCommentByType(commentsData, Constants.View);
                var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

                var comments = new DatabaseViewComments(qualifiedName, viewComment, columnComments);
                result.Add(comments);
            }

            return result;
        }

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

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select schemaname as SchemaName, matviewname as ObjectName
from pg_catalog.pg_matviews
where schemaname = @SchemaName and matviewname = @ViewName
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewComments(candidateViewName, cancellationToken);
        }

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
-- view
select n.nspname as SchemaName, c.relname as ViewName, 'VIEW' as ObjectType, c.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where c.relkind = 'm' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select n.nspname as SchemaName, c.relname as ViewName, 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where c.relkind = 'm' and n.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped
";

        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private const string ViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, c.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
left join pg_catalog.pg_description d on c.oid = d.objoid and d.objsubid = 0
where n.nspname = @SchemaName and c.relname = @ViewName
    and c.relkind = 'm' and n.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class c
inner join pg_catalog.pg_namespace n on c.relnamespace = n.oid
inner join pg_catalog.pg_attribute a on a.attrelid = c.oid
left join pg_description d on c.oid = d.objoid and a.attnum = d.objsubid
where n.nspname = @SchemaName and c.relname = @ViewName
    and c.relkind = 'm' and n.nspname not in ('pg_catalog', 'information_schema')
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
                .Select(c => Option<string>.Some(c.Comment))
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
                    Option<string>.Some(c.Comment)
                ))
                .ToDictionary(c => c.Key, c => c.Value);
        }

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
