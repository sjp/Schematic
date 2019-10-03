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
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle.Comments
{
    public class OracleTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public OracleTableCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
                .GroupBy(row => new { row.SchemaName, row.ObjectName })
                .Select(g => new
                {
                    Name = QualifyTableName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.ObjectName)),
                    Comments = g.ToList()
                })
                .OrderBy(g => g.Name.Schema)
                .ThenBy(g => g.Name.LocalName)
                .Select(table =>
                {
                    var tableComment = GetTableComment(table.Comments);
                    var primaryKeyComment = Option<string>.None;
                    var columnComments = GetColumnComments(table.Comments);
                    var checkComments = Empty.CommentLookup;
                    var foreignKeyComments = Empty.CommentLookup;
                    var uniqueKeyComments = Empty.CommentLookup;
                    var indexComments = Empty.CommentLookup;
                    var triggerComments = Empty.CommentLookup;

                    return new RelationalDatabaseTableComments(
                        table.Name,
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
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
where
    t.OWNER = :SchemaName and t.TABLE_NAME = :TableName
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null";

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
            IEnumerable<TableCommentsData> commentsData;
            if (tableName.Schema == IdentifierDefaults.Schema) // fast path
            {
                commentsData = await Connection.QueryAsync<TableCommentsData>(
                    UserTableCommentsQuery,
                    new { TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                commentsData = await Connection.QueryAsync<TableCommentsData>(
                    TableCommentsQuery,
                    new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);
            }

            var tableComment = GetTableComment(commentsData);
            var primaryKeyComment = Option<string>.None;

            var columnComments = GetColumnComments(commentsData);
            var checkComments = Empty.CommentLookup;
            var foreignKeyComments = Empty.CommentLookup;
            var uniqueKeyComments = Empty.CommentLookup;
            var indexComments = Empty.CommentLookup;
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
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName, 'TABLE' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on t.OWNER = c.OWNER and t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null

union

-- columns
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName, 'COLUMN' as ObjectType, tc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS tc on tc.OWNER = t.OWNER and tc.TABLE_NAME = t.TABLE_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = tc.OWNER and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
";

        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private const string TableCommentsQuerySql = @"
-- table
select 'TABLE' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on t.OWNER = c.OWNER and t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.OWNER = :SchemaName and t.TABLE_NAME = :TableName
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null

union

-- columns
select 'COLUMN' as ObjectType, tc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS tc on tc.OWNER = t.OWNER and tc.TABLE_NAME = t.TABLE_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = tc.OWNER and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.OWNER = :SchemaName and t.TABLE_NAME = :TableName
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
";

        protected virtual string UserTableCommentsQuery => UserTableCommentsQuerySql;

        private const string UserTableCommentsQuerySql = @"
-- table
select 'TABLE' as ObjectType, NULL as ColumnName, c.COMMENTS as ""Comment""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
left join SYS.USER_TAB_COMMENTS c on t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.TABLE_NAME = :TableName and mv.MVIEW_NAME is null

union

-- columns
select 'COLUMN' as ObjectType, tc.COLUMN_NAME as ColumnName, c.COMMENTS as ""Comment""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.USER_TAB_COLS tc on tc.TABLE_NAME = t.TABLE_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.TABLE_NAME = :TableName and mv.MVIEW_NAME is null
";

        private static Option<string> GetTableComment(IEnumerable<TableCommentsData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(c => c.ObjectType == Constants.Table)
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
        }
    }
}
