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
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql.Comments
{
    public class MySqlTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public MySqlTableCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IRelationalDatabaseTableComments>();

            var allCommentsData = await Connection.QueryAsync<TableCommentsData>(
                AllTableCommentsQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.TableName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.TableName);
                var qualifiedName = QualifyTableName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var tableComment = GetFirstCommentByType(commentsData, Constants.Table);
                var primaryKeyComment = Option<string>.None;

                var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
                var checkComments = Empty.CommentLookup;
                var foreignKeyComments = Empty.CommentLookup;
                var uniqueKeyComments = Empty.CommentLookup;
                var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
                var triggerComments = Empty.CommentLookup;

                var comments = new RelationalDatabaseTableComments(
                    qualifiedName,
                    tableComment,
                    primaryKeyComment,
                    columnComments,
                    checkComments,
                    uniqueKeyComments,
                    foreignKeyComments,
                    indexComments,
                    triggerComments
                );

                result.Add(comments);
            }

            return result
                .OrderBy(c => c.TableName.Schema)
                .ThenBy(c => c.TableName.LocalName)
                .ToList();
        }

        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select table_schema as SchemaName, table_name as ObjectName
from information_schema.tables
where table_schema = @SchemaName and table_name = @TableName
limit 1";

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTableComments(candidateTableName, cancellationToken);
        }

        protected virtual OptionAsync<IRelationalDatabaseTableComments> LoadTableComments(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IRelationalDatabaseTableComments> LoadTableCommentsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<TableCommentsData>(
                TableCommentsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var tableComment = GetFirstCommentByType(commentsData, Constants.Table);
            var primaryKeyComment = Option<string>.None;

            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
            var checkComments = Empty.CommentLookup;
            var foreignKeyComments = Empty.CommentLookup;
            var uniqueKeyComments = Empty.CommentLookup;
            var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
            var triggerComments = Empty.CommentLookup;

            return new RelationalDatabaseTableComments(
                tableName,
                tableComment,
                primaryKeyComment,
                columnComments,
                checkComments,
                uniqueKeyComments,
                foreignKeyComments,
                indexComments,
                triggerComments
            );
        }

        protected virtual string AllTableCommentsQuery => AllTableCommentsQuerySql;

        private const string AllTableCommentsQuerySql = @"
-- table
select TABLE_SCHEMA as SchemaName, TABLE_NAME as TableName, 'TABLE' as ObjectType, TABLE_NAME as ObjectName, TABLE_COMMENT as Comment
from INFORMATION_SCHEMA.TABLES
where TABLE_SCHEMA = @SchemaName

union

-- columns
select c.TABLE_SCHEMA as SchemaName, c.TABLE_NAME as ObjectName, 'COLUMN' as ObjectType, c.COLUMN_NAME as ObjectName, c.COLUMN_COMMENT as Comment
from INFORMATION_SCHEMA.COLUMNS c
inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_NAME = t.TABLE_NAME
where c.TABLE_SCHEMA = @SchemaName

union

-- indexes
select s.TABLE_SCHEMA as SchemaName, s.TABLE_NAME as ObjectName, 'INDEX' as ObjectType, s.INDEX_NAME as ObjectName, s.INDEX_COMMENT as Comment
from INFORMATION_SCHEMA.STATISTICS s
inner join INFORMATION_SCHEMA.TABLES t on s.TABLE_SCHEMA = t.TABLE_SCHEMA and s.TABLE_NAME = t.TABLE_NAME
where s.TABLE_SCHEMA = @SchemaName
";

        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private const string TableCommentsQuerySql = @"
-- table
select 'TABLE' as ObjectType, TABLE_NAME as ObjectName, TABLE_COMMENT as Comment
from INFORMATION_SCHEMA.TABLES
where TABLE_SCHEMA = @SchemaName and TABLE_NAME = @TableName

union

-- columns
select 'COLUMN' as ObjectType, c.COLUMN_NAME as ObjectName, c.COLUMN_COMMENT as Comment
from INFORMATION_SCHEMA.COLUMNS c
inner join INFORMATION_SCHEMA.TABLES t on c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_NAME = t.TABLE_NAME
where c.TABLE_SCHEMA = @SchemaName and c.TABLE_NAME = @TableName

union

-- indexes
select 'INDEX' as ObjectType, s.INDEX_NAME as ObjectName, s.INDEX_COMMENT as Comment
from INFORMATION_SCHEMA.STATISTICS s
inner join INFORMATION_SCHEMA.TABLES t on s.TABLE_SCHEMA = t.TABLE_SCHEMA and s.TABLE_NAME = t.TABLE_NAME
where s.TABLE_SCHEMA = @SchemaName and s.TABLE_NAME = @TableName
";

        private static Option<string> GetFirstCommentByType(IEnumerable<TableCommentsData> commentsData, string objectType)
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

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<TableCommentsData> commentsData, string objectType)
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

        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
        }

        private static class Constants
        {
            public const string Table = "TABLE";

            public const string Column = "COLUMN";

            public const string Index = "INDEX";
        }
    }
}
