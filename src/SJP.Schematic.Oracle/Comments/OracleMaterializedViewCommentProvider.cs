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
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleMaterializedViewCommentProvider : IDatabaseViewCommentProvider
    {
        public OracleMaterializedViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<TableCommentsData>(AllViewCommentsQuery, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseViewComments>();

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.ObjectName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.ObjectName);
                var qualifiedName = QualifyViewName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var viewComment = GetViewComment(commentsData);
                var columnComments = GetColumnComments(commentsData);

                var comments = new DatabaseViewComments(qualifiedName, viewComment, columnComments);
                result.Add(comments);
            }

            return result
                .OrderBy(c => c.ViewName.Schema)
                .ThenBy(c => c.ViewName.LocalName)
                .ToList();
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
select mv.OWNER as SchemaName, mv.MVIEW_NAME as ObjectName
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where mv.OWNER = :SchemaName and mv.MVIEW_NAME = :ViewName
    and o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'";

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
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
-- view
select v.OWNER as SchemaName, v.MVIEW_NAME as ObjectName, 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEW_COMMENTS c on v.OWNER = c.OWNER and v.MVIEW_NAME = c.MVIEW_NAME
where o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select v.OWNER as SchemaName, v.MVIEW_NAME as ObjectName, 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
";

        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private const string ViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEW_COMMENTS c on v.OWNER = c.OWNER and v.MVIEW_NAME = c.MVIEW_NAME
where v.OWNER = :SchemaName and v.MVIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_MVIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.MVIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.OWNER = :SchemaName and v.MVIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'
";

        protected virtual string UserViewCommentsQuery => UserViewCommentsQuerySql;

        private const string UserViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.USER_MVIEWS v
left join SYS.USER_MVIEW_COMMENTS c on v.MVIEW_NAME = c.MVIEW_NAME
where v.MVIEW_NAME = :ViewName

union

-- columns
select 'COLUMN' as ObjectType, vc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
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
