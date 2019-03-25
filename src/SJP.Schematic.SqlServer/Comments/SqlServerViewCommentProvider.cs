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
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
    public class SqlServerViewCommentProvider : IDatabaseViewCommentProvider
    {
        public SqlServerViewCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected virtual string CommentProperty { get; } = "MS_Description";

        public async Task<IReadOnlyCollection<IDatabaseViewComments>> GetAllViewComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IDatabaseViewComments>();

            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllViewCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.TableName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.TableName);
                var qualifiedName = QualifyViewName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var viewComment = GetFirstCommentByType(commentsData, Constants.View);
                var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

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
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.views
where schema_id = schema_id(@SchemaName) and name = @ViewName
    and is_ms_shipped = 0";

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
            var commentsData = await Connection.QueryAsync<CommentsData>(
                ViewCommentsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var viewComment = GetFirstCommentByType(commentsData, Constants.View);
            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);

            return new DatabaseViewComments(viewName, viewComment, columnComments);
        }

        protected virtual string AllViewCommentsQuery => AllViewCommentsQuerySql;

        private const string AllViewCommentsQuerySql = @"
-- view
select SCHEMA_NAME(v.schema_id) as SchemaName, v.name as TableName, 'VIEW' as ObjectType, v.name as ObjectName, ep.value as Comment
from sys.views v
left join sys.extended_properties ep on v.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where v.is_ms_shipped = 0

union

-- columns
select SCHEMA_NAME(v.schema_id) as SchemaName, v.name as TableName, 'COLUMN' as ObjectType, c.name as ObjectName, ep.value as Comment
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.extended_properties ep on v.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @CommentProperty
where v.is_ms_shipped = 0
";

        protected virtual string ViewCommentsQuery => ViewCommentsQuerySql;

        private const string ViewCommentsQuerySql = @"
-- view
select 'VIEW' as ObjectType, v.name as ObjectName, ep.value as Comment
from sys.views v
left join sys.extended_properties ep on v.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where v.schema_id = SCHEMA_ID(@SchemaName) and v.name = @ViewName and v.is_ms_shipped = 0

union

-- columns
select 'COLUMN' as ObjectType, c.name as ObjectName, ep.value as Comment
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
                .Select(c => Option<string>.Some(c.Comment))
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
