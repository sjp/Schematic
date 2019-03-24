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
        public OracleMaterializedViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default(CancellationToken))
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

            viewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select mv.OWNER as SchemaName, mv.MVIEW_NAME as ObjectName
from SYS.ALL_MVIEWS mv
inner join SYS.ALL_OBJECTS o on mv.OWNER = o.OWNER and mv.MVIEW_NAME = o.OBJECT_NAME
where mv.OWNER = :SchemaName and mv.MVIEW_NAME = :ViewName
    and o.ORACLE_MAINTAINED <> 'Y' and o.OBJECT_TYPE <> 'TABLE'";

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
            return LoadViewCommentsAsyncCore(candidateViewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseViewComments>> LoadViewCommentsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewName(candidateViewName, cancellationToken);
            var resolvedViewNameOptionIsNone = await resolvedViewNameOption.IsNone.ConfigureAwait(false);
            if (resolvedViewNameOptionIsNone)
                return Option<IDatabaseViewComments>.None;

            var resolvedViewName = await resolvedViewNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var commentsData = await Connection.QueryAsync<TableCommentsData>(
                ViewCommentsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var viewComment = GetViewComment(commentsData);
            var columnComments = GetColumnComments(commentsData);

            var comments = new DatabaseViewComments(resolvedViewName, viewComment, columnComments);
            return Option<IDatabaseViewComments>.Some(comments);
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

        private static Option<string> GetViewComment(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => c.ObjectType == Constants.View)
                .Select(c => Option<string>.Some(c.Comment))
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
