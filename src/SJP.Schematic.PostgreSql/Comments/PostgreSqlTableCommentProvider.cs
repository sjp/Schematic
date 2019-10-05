using System;
using System.Collections.Generic;
using System.Data;
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
    public class PostgreSqlTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public PostgreSqlTableCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<TableCommentsData>(AllTableCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var tableName = QualifyTableName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var comments = g.ToList();

                    var tableComment = GetFirstCommentByType(comments, Constants.Table);
                    var primaryKeyComment = GetFirstCommentByType(comments, Constants.Primary);

                    var columnComments = GetCommentLookupByType(comments, Constants.Column);
                    var checkComments = GetCommentLookupByType(comments, Constants.Check);
                    var foreignKeyComments = GetCommentLookupByType(comments, Constants.ForeignKey);
                    var uniqueKeyComments = GetCommentLookupByType(comments, Constants.Unique);
                    var indexComments = GetCommentLookupByType(comments, Constants.Index);
                    var triggerComments = GetCommentLookupByType(comments, Constants.Trigger);

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
                });

            foreach (var comment in comments)
                yield return comment;
        }

        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(tableName)
                .Select(QualifyTableName);

            return resolvedNames
                .Select(name => GetResolvedTableNameStrict(name, cancellationToken))
                .FirstSome(cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = candidateTableName.Schema, TableName = candidateTableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select schemaname as SchemaName, tablename as ObjectName
from pg_catalog.pg_tables
where schemaname = @SchemaName and tablename = @TableName
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
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
            var primaryKeyComment = GetFirstCommentByType(commentsData, Constants.Primary);

            var columnComments = GetCommentLookupByType(commentsData, Constants.Column);
            var checkComments = GetCommentLookupByType(commentsData, Constants.Check);
            var foreignKeyComments = GetCommentLookupByType(commentsData, Constants.ForeignKey);
            var uniqueKeyComments = GetCommentLookupByType(commentsData, Constants.Unique);
            var indexComments = GetCommentLookupByType(commentsData, Constants.Index);
            var triggerComments = GetCommentLookupByType(commentsData, Constants.Trigger);

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
select wrapped.* from (
-- table
select ns.nspname as SchemaName, t.relname as TableName, 'TABLE' as ObjectType, t.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
left join pg_catalog.pg_description d on d.objoid = t.oid and d.objsubid = 0
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select ns.nspname as SchemaName, t.relname as TableName, 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid
left join pg_description d on t.oid = d.objoid and a.attnum = d.objsubid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped

union

-- checks
select ns.nspname as SchemaName, t.relname as TableName, 'CHECK' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'c'

union

-- foreign keys
select ns.nspname as SchemaName, t.relname as TableName, 'FOREIGN KEY' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'f'

union

-- unique keys
select ns.nspname as SchemaName, t.relname as TableName, 'UNIQUE' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'u'

union

-- primary key
select ns.nspname as SchemaName, t.relname as TableName, 'PRIMARY' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'p'

union

-- indexes
select ns.nspname as SchemaName, t.relname as TableName, 'INDEX' as ObjectType, ci.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_index i on i.indrelid = t.oid
inner join pg_catalog.pg_class ci on i.indexrelid = ci.oid and ci.relkind = 'i'
left join pg_catalog.pg_description d on d.objoid = i.indexrelid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- triggers
select ns.nspname as SchemaName, t.relname as TableName, 'TRIGGER' as ObjectType, tr.tgname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = tr.oid
where t.relkind = 'r' and ns.nspname not in ('pg_catalog', 'information_schema')
) wrapped order by wrapped.SchemaName, wrapped.TableName
";

        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private const string TableCommentsQuerySql = @"
-- table
select 'TABLE' as ObjectType, t.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
left join pg_catalog.pg_description d on d.objoid = t.oid and d.objsubid = 0
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select 'COLUMN' as ObjectType, a.attname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid
left join pg_description d on t.oid = d.objoid and a.attnum = d.objsubid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped

union

-- checks
select 'CHECK' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'c'

union

-- foreign keys
select 'FOREIGN KEY' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'f'

union

-- unique keys
select 'UNIQUE' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'u'

union

-- primary key
select 'PRIMARY' as ObjectType, c.conname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'p'

union

-- indexes
select 'INDEX' as ObjectType, ci.relname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_index i on i.indrelid = t.oid
inner join pg_catalog.pg_class ci on i.indexrelid = ci.oid and ci.relkind = 'i'
left join pg_catalog.pg_description d on d.objoid = i.indexrelid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- triggers
select 'TRIGGER' as ObjectType, tr.tgname as ObjectName, d.description as Comment
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = tr.oid
where t.relkind = 'r' and ns.nspname = @SchemaName and t.relname = @TableName
    and ns.nspname not in ('pg_catalog', 'information_schema')
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

            public const string Check = "CHECK";

            public const string ForeignKey = "FOREIGN KEY";

            public const string Unique = "UNIQUE";

            public const string Primary = "PRIMARY";

            public const string Index = "INDEX";

            public const string Trigger = "TRIGGER";
        }
    }
}
