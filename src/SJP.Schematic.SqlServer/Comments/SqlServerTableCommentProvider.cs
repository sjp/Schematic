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
    public class SqlServerTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public SqlServerTableCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected virtual string CommentProperty { get; } = "MS_Description";

        public async Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IRelationalDatabaseTableComments>();

            var allCommentsData = await Connection.QueryAsync<TableCommentsData>(
                AllTableCommentsSql,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.TableName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.TableName);
                var qualifiedName = QualifyTableName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var tableComment = GetFirstCommentByType(commentsData, Constants.Table);
                var primaryKeyComment = GetFirstCommentByType(commentsData, Constants.Primary);

                var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
                var checkComments = GetCommentLookupByType(commentsData, Constants.Check);
                var foreignKeyComments = GetCommentLookupByType(commentsData, Constants.ForeignKey);
                var uniqueKeyComments = GetCommentLookupByType(commentsData, Constants.Unique);
                var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
                var triggerComments = GetCommentLookupByType(commentsData, Constants.Trigger);

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

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select schema_name(schema_id) as SchemaName, name as ObjectName
from sys.tables
where is_ms_shipped = 0
order by schema_name(schema_id), name";

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
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.tables
where schema_id = schema_id(@SchemaName) and name = @TableName
    and is_ms_shipped = 0";

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
            return LoadTableCommentsAsyncCore(candidateTableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTableComments>> LoadTableCommentsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var candidateTableName = QualifyTableName(tableName);
            var resolvedTableNameOption = GetResolvedTableName(candidateTableName, cancellationToken);
            var resolvedTableNameOptionIsNone = await resolvedTableNameOption.IsNone.ConfigureAwait(false);
            if (resolvedTableNameOptionIsNone)
                return Option<IRelationalDatabaseTableComments>.None;

            var resolvedTableName = await resolvedTableNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var commentsData = await Connection.QueryAsync<TableCommentsData>(
                TableCommentsSql,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var tableComment = GetFirstCommentByType(commentsData, Constants.Table);
            var primaryKeyComment = GetFirstCommentByType(commentsData, Constants.Primary);

            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
            var checkComments = GetCommentLookupByType(commentsData, Constants.Check);
            var foreignKeyComments = GetCommentLookupByType(commentsData, Constants.ForeignKey);
            var uniqueKeyComments = GetCommentLookupByType(commentsData, Constants.Unique);
            var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
            var triggerComments = GetCommentLookupByType(commentsData, Constants.Trigger);

            var comments = new RelationalDatabaseTableComments(
                resolvedTableName,
                tableComment,
                primaryKeyComment,
                columnComments,
                checkComments,
                uniqueKeyComments,
                foreignKeyComments,
                indexComments,
                triggerComments
            );

            return Option<IRelationalDatabaseTableComments>.Some(comments);
        }

        private const string AllTableCommentsSql = @"
-- table
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'TABLE' as ObjectType, t.name as ObjectName, ep.value as Comment
from sys.tables t
left join sys.extended_properties ep on t.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where t.is_ms_shipped = 0

union

-- columns
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'COLUMN' as ObjectType, c.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0

union

-- checks
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'CHECK' as ObjectType, cc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
left join sys.extended_properties ep on cc.object_id = ep.major_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0

union

-- foreign keys
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'FOREIGN KEY' as ObjectType, fk.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.foreign_keys fk on t.object_id = fk.parent_object_id
left join sys.extended_properties ep on fk.object_id = ep.major_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0

union

-- unique keys
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'UNIQUE' as ObjectType, kc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0 and kc.type = 'UQ'

union

-- primary key
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'PRIMARY' as ObjectType, kc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0 and kc.type = 'PK'

union

-- indexes
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'INDEX' as ObjectType, i.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and i.index_id = ep.minor_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0 and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore

union

-- triggers
select SCHEMA_NAME(t.schema_id) as SchemaName, t.name as TableName, 'TRIGGER' as ObjectType, tr.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.triggers tr on t.object_id = tr.parent_id
left join sys.extended_properties ep on t.object_id = ep.major_id and tr.object_id = ep.minor_id and ep.name = @CommentProperty
where t.is_ms_shipped = 0
";

        private const string TableCommentsSql = @"
-- table
select 'TABLE' as ObjectType, t.name as ObjectName, ep.value as Comment
from sys.tables t
left join sys.extended_properties ep on t.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0

union

-- columns
select 'COLUMN' as ObjectType, c.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0

union

-- checks
select 'CHECK' as ObjectType, cc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
left join sys.extended_properties ep on cc.object_id = ep.major_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0

union

-- foreign keys
select 'FOREIGN KEY' as ObjectType, fk.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.foreign_keys fk on t.object_id = fk.parent_object_id
left join sys.extended_properties ep on fk.object_id = ep.major_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0

union

-- unique keys
select 'UNIQUE' as ObjectType, kc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName
    and t.is_ms_shipped = 0 and kc.type = 'UQ'

union

-- primary key
select 'PRIMARY' as ObjectType, kc.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName
    and t.is_ms_shipped = 0and kc.type = 'PK'

union

-- indexes
select 'INDEX' as ObjectType, i.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and i.index_id = ep.minor_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore

union

-- triggers
select 'TRIGGER' as ObjectType, tr.name as ObjectName, ep.value as Comment
from sys.tables t
inner join sys.triggers tr on t.object_id = tr.parent_id
left join sys.extended_properties ep on t.object_id = ep.major_id and tr.object_id = ep.minor_id and ep.name = @CommentProperty
where t.schema_id = SCHEMA_ID(@SchemaName) and t.name = @TableName and t.is_ms_shipped = 0
";

        private static Option<string> GetFirstCommentByType(IEnumerable<TableCommentsData> commentsData, string objectType)
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
                    Option<string>.Some(c.Comment)
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

            public const string Check = "CHECK";

            public const string ForeignKey = "FOREIGN KEY";

            public const string Unique = "UNIQUE";

            public const string Primary = "PRIMARY";

            public const string Index = "INDEX";

            public const string Trigger = "TRIGGER";
        }
    }
}
