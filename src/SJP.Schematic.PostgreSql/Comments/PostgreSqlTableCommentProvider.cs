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
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql.Comments
{
    /// <summary>
    /// A database table comment provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseTableCommentProvider" />
    public class PostgreSqlTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTableCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlTableCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
            var queryResult = await Connection.QueryAsync<GetTableNamesQueryResult>(TablesQuery, cancellationToken).ConfigureAwait(false);
            var tableNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
                .Select(QualifyTableName);

            foreach (var tableName in tableNames)
                yield return await LoadTableCommentsAsyncCore(tableName, cancellationToken).ConfigureAwait(false);
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
select schemaname as ""{ nameof(GetTableNameQueryResult.SchemaName) }"", tablename as ""{ nameof(GetTableNameQueryResult.TableName) }""
from pg_catalog.pg_tables
where schemaname = @{ nameof(GetTableNameQuery.SchemaName) } and tablename = @{ nameof(GetTableNameQuery.TableName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

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
            var result = await Connection.QueryAsync<GetTableCommentsQueryResult>(
                TableCommentsQuery,
                new GetTableCommentsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var commentData = result.Select(r => new CommentData
            {
                ObjectName = r.ObjectName,
                Comment = r.Comment,
                ObjectType = r.ObjectType
            }).ToList();

            var tableComment = GetFirstCommentByType(commentData, Constants.Table);
            var primaryKeyComment = GetFirstCommentByType(commentData, Constants.Primary);

            var columnComments = GetCommentLookupByType(commentData, Constants.Column);
            var checkComments = GetCommentLookupByType(commentData, Constants.Check);
            var foreignKeyComments = GetCommentLookupByType(commentData, Constants.ForeignKey);
            var uniqueKeyComments = GetCommentLookupByType(commentData, Constants.Unique);
            var indexComments = GetCommentLookupByType(commentData, Constants.Index)
                .Where(kv => !uniqueKeyComments.ContainsKey(kv.Key))
                .ToReadOnlyDictionary(IdentifierComparer.Ordinal);
            var triggerComments = GetCommentLookupByType(commentData, Constants.Trigger);

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
        /// A SQL query that retrieves the names of all tables in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TablesQuery => TablesQuerySql;

        private static readonly string TablesQuerySql = @$"
select
    schemaname as ""{ nameof(GetTableNamesQueryResult.SchemaName) }"",
    tablename as ""{ nameof(GetTableNamesQueryResult.TableName) }""
from pg_catalog.pg_tables
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, tablename";

        /// <summary>
        /// A SQL query definition which retrieves all comment information for a particular table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string TableCommentsQuery => TableCommentsQuerySql;

        private static readonly string TableCommentsQuerySql = @$"
-- table
select
    'TABLE' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    t.relname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
left join pg_catalog.pg_description d on d.objoid = t.oid and d.objsubid = 0
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- columns
select
    'COLUMN' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    a.attname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_attribute a on a.attrelid = t.oid
left join pg_description d on t.oid = d.objoid and a.attnum = d.objsubid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and a.attnum > 0 and not a.attisdropped

union

-- checks
select
    'CHECK' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    c.conname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'c'

union

-- foreign keys
select
    'FOREIGN KEY' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    c.conname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'f'

union

-- unique keys
select
    'UNIQUE' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    c.conname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'u'

union

-- primary key
select
    'PRIMARY' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    c.conname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
    and c.conrelid > 0 and c.contype = 'p'

union

-- indexes
select
    'INDEX' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    ci.relname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_index i on i.indrelid = t.oid and i.indisprimary = false
inner join pg_catalog.pg_class ci on i.indexrelid = ci.oid and ci.relkind = 'i'
left join pg_catalog.pg_description d on d.objoid = i.indexrelid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')

union

-- triggers
select
    'TRIGGER' as ""{ nameof(GetTableCommentsQueryResult.ObjectType) }"",
    tr.tgname as ""{ nameof(GetTableCommentsQueryResult.ObjectName) }"",
    d.description as ""{ nameof(GetTableCommentsQueryResult.Comment) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on t.relnamespace = ns.oid
inner join pg_catalog.pg_trigger tr on tr.tgrelid = t.oid and tr.tgisinternal = false
left join pg_catalog.pg_description d on d.objoid = tr.oid
where t.relkind = 'r' and ns.nspname = @{ nameof(GetTableCommentsQuery.SchemaName) } and t.relname = @{ nameof(GetTableCommentsQuery.TableName) }
    and ns.nspname not in ('pg_catalog', 'information_schema')
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<CommentData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(c => new KeyValuePair<Identifier, Option<string>>(
                    Identifier.CreateQualifiedIdentifier(c.ObjectName),
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

            public const string Check = "CHECK";

            public const string ForeignKey = "FOREIGN KEY";

            public const string Unique = "UNIQUE";

            public const string Primary = "PRIMARY";

            public const string Index = "INDEX";

            public const string Trigger = "TRIGGER";
        }

        private record CommentData
        {
            public string? ObjectName { get; init; }

            public string? ObjectType { get; init; }

            public string? Comment { get; init; }
        }
    }
}
