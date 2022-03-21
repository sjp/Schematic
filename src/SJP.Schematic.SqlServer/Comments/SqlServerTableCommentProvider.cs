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
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.SqlServer.QueryResult;

namespace SJP.Schematic.SqlServer.Comments;

/// <summary>
/// A comment provider for SQL Server database tables.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableCommentProvider" />
public class SqlServerTableCommentProvider : IRelationalDatabaseTableCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerTableCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
    public SqlServerTableCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
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
    /// Retrieves the extended property name used to store comments on an object.
    /// </summary>
    /// <value>The comment property name.</value>
    protected virtual string CommentProperty { get; } = "MS_Description";

    /// <summary>
    /// Retrieves comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public async IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResults = await Connection.QueryAsync<GetAllTableNamesQueryResult>(TablesQuery, cancellationToken).ConfigureAwait(false);
        var tableNames = queryResults
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
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
    protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        tableName = QualifyTableName(tableName);
        var qualifiedTableName = Connection.QueryFirstOrNone<GetTableNameQueryResult>(
            TableNameQuery,
            new GetTableNameQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName));
    }

    /// <summary>
    /// A SQL query definition that resolves a table name for the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableNameQuery => TableNameQuerySql;

    private const string TableNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(GetTableNameQueryResult.SchemaName) }], name as [{ nameof(GetTableNameQueryResult.TableName) }]
from sys.tables
where schema_id = schema_id(@{ nameof(GetTableNameQuery.SchemaName) }) and name = @{ nameof(GetTableNameQuery.TableName) }
    and is_ms_shipped = 0";

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
        var queryResult = await Connection.QueryAsync<GetTableCommentsQueryResult>(
            TableCommentsQuery,
            new GetTableCommentsQuery
            {
                SchemaName = tableName.Schema!,
                TableName = tableName.LocalName,
                CommentProperty = CommentProperty
            },
            cancellationToken
        ).ConfigureAwait(false);

        var commentData = queryResult.Select(r => new CommentData
        {
            ObjectName = r.ObjectName,
            ObjectType = r.ObjectType,
            Comment = r.Comment
        }).ToList();

        var tableComment = GetFirstCommentByType(commentData, Constants.Table);
        var primaryKeyComment = GetFirstCommentByType(commentData, Constants.Primary);

        var columnComments = GetCommentLookupByType(commentData, Constants.Column);
        var checkComments = GetCommentLookupByType(commentData, Constants.Check);
        var foreignKeyComments = GetCommentLookupByType(commentData, Constants.ForeignKey);
        var uniqueKeyComments = GetCommentLookupByType(commentData, Constants.Unique);
        var indexComments = GetCommentLookupByType(commentData, Constants.Index);
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

    private const string TablesQuerySql = @$"
select schema_name(schema_id) as [{ nameof(GetAllTableNamesQueryResult.SchemaName) }], name as [{ nameof(GetAllTableNamesQueryResult.TableName) }]
from sys.tables
where is_ms_shipped = 0
order by schema_name(schema_id), name";

    /// <summary>
    /// A SQL query definition which retrieves all comment information for a particular table.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableCommentsQuery => TableCommentsQuerySql;

    private const string TableCommentsQuerySql = @$"
-- table
select
    'TABLE' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    t.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
left join sys.extended_properties ep on t.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) } and ep.minor_id = 0
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0

union

-- columns
select
    'COLUMN' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    c.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0

union

-- checks
select
    'CHECK' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    cc.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
left join sys.extended_properties ep on cc.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0

union

-- foreign keys
select
    'FOREIGN KEY' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    fk.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.foreign_keys fk on t.object_id = fk.parent_object_id
left join sys.extended_properties ep on fk.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0

union

-- unique keys
select
    'UNIQUE' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    kc.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) }
    and t.is_ms_shipped = 0 and kc.type = 'UQ'

union

-- primary key
select
    'PRIMARY' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    kc.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) }
    and t.is_ms_shipped = 0and kc.type = 'PK'

union

-- indexes
select
    'INDEX' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    i.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and i.index_id = ep.minor_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore

union

-- triggers
select
    'TRIGGER' as [{ nameof(GetTableCommentsQueryResult.ObjectType) }],
    tr.name as [{ nameof(GetTableCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetTableCommentsQueryResult.Comment) }]
from sys.tables t
inner join sys.triggers tr on t.object_id = tr.parent_id
left join sys.extended_properties ep on tr.object_id = ep.major_id and ep.name = @{ nameof(GetTableCommentsQuery.CommentProperty) }
where t.schema_id = SCHEMA_ID(@{ nameof(GetTableCommentsQuery.SchemaName) }) and t.name = @{ nameof(GetTableCommentsQuery.TableName) } and t.is_ms_shipped = 0
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
            .Select(static c => new KeyValuePair<Identifier, Option<string>>(
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

    private sealed record CommentData
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}