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
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Oracle.QueryResult;

namespace SJP.Schematic.Oracle.Comments
{
    /// <summary>
    /// A database table comment provider for Oracle databases.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseTableCommentProvider" />
    public class OracleTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTableCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleTableCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        /// Retrieves comments for all database tables.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database table comments, where available.</returns>
        public async IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<GetAllTableCommentsQueryResult>(AllTableCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(static row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var tableName = QualifyTableName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));

                    var commentData = g.Select(r => new CommentData
                    {
                        ColumnName = r.ColumnName,
                        Comment = r.Comment,
                        ObjectType = r.ObjectType
                    }).ToList();

                    var tableComment = GetTableComment(commentData);
                    var primaryKeyComment = Option<string>.None;
                    var columnComments = GetColumnComments(commentData);
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
                });

            foreach (var comment in comments)
                yield return comment;
        }

        /// <summary>
        /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="tableName">A table name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Gets the resolved name of the table without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="tableName">A table name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<GetTableNameQueryResult>(
                TableNameQuery,
                new GetTableNameQuery { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
        }

        /// <summary>
        /// A SQL query definition that resolves a table name for the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableNameQuery => TableNameQuerySql;

        private static readonly string TableNameQuerySql = @$"
select t.OWNER as ""{ nameof(GetTableNameQueryResult.SchemaName) }"", t.TABLE_NAME as ""{ nameof(GetTableNameQueryResult.TableName) }""
from SYS.ALL_TABLES t
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
where
    t.OWNER = :{ nameof(GetTableNameQuery.SchemaName) } and t.TABLE_NAME = :{ nameof(GetTableNameQuery.TableName) }
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null";

        /// <summary>
        /// Retrieves comments for a database table, if available.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTableComments(candidateTableName, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's comments.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for a table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
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
            if (string.Equals(tableName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)) // fast path
                return await LoadUserTableCommentsAsyncCore(tableName, cancellationToken);

            var result = await Connection.QueryAsync<GetTableCommentsQueryResult>(
                TableCommentsQuery,
                new GetTableCommentsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ColumnName = r.ColumnName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var tableComment = GetTableComment(commentData);
            var primaryKeyComment = Option<string>.None;

            var columnComments = GetColumnComments(commentData);
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

        private async Task<IRelationalDatabaseTableComments> LoadUserTableCommentsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var result = await Connection.QueryAsync<GetUserTableCommentsQueryResult>(
                UserTableCommentsQuery,
                new GetUserTableCommentsQuery { TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ColumnName = r.ColumnName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var tableComment = GetTableComment(commentData);
            var primaryKeyComment = Option<string>.None;

            var columnComments = GetColumnComments(commentData);
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

        /// <summary>
        /// A SQL query definition which retrieves all comment information for all tables.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllTableCommentsQuery => AllTableCommentsQuerySql;

        private static readonly string AllTableCommentsQuerySql = @$"
select wrapped.* from (
-- table
select
    t.OWNER as ""{ nameof(GetAllTableCommentsQueryResult.SchemaName) }"",
    t.TABLE_NAME as ""{ nameof(GetAllTableCommentsQueryResult.TableName) }"",
    'TABLE' as ""{ nameof(GetAllTableCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetAllTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetAllTableCommentsQueryResult.Comment) }""
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
select
    t.OWNER as ""{ nameof(GetAllTableCommentsQueryResult.SchemaName) }"",
    t.TABLE_NAME as ""{ nameof(GetAllTableCommentsQueryResult.TableName) }"",
    'COLUMN' as ""{ nameof(GetAllTableCommentsQueryResult.ObjectType) }"",
    tc.COLUMN_NAME as ""{ nameof(GetAllTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetAllTableCommentsQueryResult.Comment) }""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS tc on tc.OWNER = t.OWNER and tc.TABLE_NAME = t.TABLE_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = tc.OWNER and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
) wrapped order by wrapped.""{ nameof(GetAllTableCommentsQueryResult.SchemaName) }"", wrapped.""{ nameof(GetAllTableCommentsQueryResult.TableName) }""";

        /// <summary>
        /// A SQL query definition which retrieves all comment information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private static readonly string TableCommentsQuerySql = @$"
-- table
select
    'TABLE' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on t.OWNER = c.OWNER and t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.OWNER = :{ nameof(GetTableCommentsQuery.SchemaName) } and t.TABLE_NAME = :{ nameof(GetTableCommentsQuery.TableName) }
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null

union

-- columns
select
    'COLUMN' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    tc.COLUMN_NAME as ""{ nameof(GetTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from SYS.ALL_TABLES t
left join SYS.ALL_MVIEWS mv on t.OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS tc on tc.OWNER = t.OWNER and tc.TABLE_NAME = t.TABLE_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = tc.OWNER and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.OWNER = :{ nameof(GetTableCommentsQuery.SchemaName) } and t.TABLE_NAME = :{ nameof(GetTableCommentsQuery.TableName) }
    and o.ORACLE_MAINTAINED <> 'Y'
    and o.GENERATED <> 'Y'
    and o.SECONDARY <> 'Y'
    and mv.MVIEW_NAME is null
";

        /// <summary>
        /// A SQL query definition which retrieves all comment information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserTableCommentsQuery => UserTableCommentsQuerySql;

        private static readonly string UserTableCommentsQuerySql = @$"
-- table
select
    'TABLE' as ""{ nameof(GetUserTableCommentsQueryResult.ObjectType) }"",
    NULL as ""{ nameof(GetUserTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetUserTableCommentsQueryResult.Comment) }""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
left join SYS.USER_TAB_COMMENTS c on t.TABLE_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'TABLE'
where t.TABLE_NAME = :{ nameof(GetUserTableCommentsQuery.TableName) } and mv.MVIEW_NAME is null

union

-- columns
select
    'COLUMN' as ""{ nameof(GetUserTableCommentsQueryResult.ObjectType) }"",
    tc.COLUMN_NAME as ""{ nameof(GetUserTableCommentsQueryResult.ColumnName) }"",
    c.COMMENTS as ""{ nameof(GetUserTableCommentsQueryResult.Comment) }""
from SYS.USER_TABLES t
left join SYS.USER_MVIEWS mv on t.TABLE_NAME = mv.MVIEW_NAME
inner join SYS.USER_TAB_COLS tc on tc.TABLE_NAME = t.TABLE_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
where t.TABLE_NAME = :{ nameof(GetUserTableCommentsQuery.TableName) } and mv.MVIEW_NAME is null
";

        private static Option<string> GetTableComment(IEnumerable<CommentData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(static c => string.Equals(c.ObjectType, Constants.Table, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetColumnComments(IEnumerable<CommentData> commentsData)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));

            return commentsData
                .Where(static c => string.Equals(c.ObjectType, Constants.Column, StringComparison.Ordinal))
                .Select(static c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ColumnName),
                    !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None
                ))
                .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
        }

        /// <summary>
        /// Qualifies the name of a table, using known identifier defaults.
        /// </summary>
        /// <param name="tableName">A table name to qualify.</param>
        /// <returns>A table name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
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

        private record CommentData
        {
            public string? ColumnName { get; init; }

            public string? ObjectType { get; init; }

            public string? Comment { get; init; }
        }
    }
}
