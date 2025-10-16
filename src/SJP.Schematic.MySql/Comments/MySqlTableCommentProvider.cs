using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql.Comments;

/// <summary>
/// A table comment provider for MySQL databases.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableCommentProvider" />
public class MySqlTableCommentProvider : IRelationalDatabaseTableCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlTableCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the given database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public MySqlTableCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Enumerates comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> EnumerateAllTableComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync(
                GetAllTableNames.Sql,
                new GetAllTableNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .SelectAwait(tableName => LoadTableCommentsAsyncCore(tableName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Retrieves comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default)
    {
        var tableNames = await Connection.QueryEnumerableAsync(
                GetAllTableNames.Sql,
                new GetAllTableNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken);

        return await tableNames
            .Select(tableName => LoadTableCommentsAsyncCore(tableName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        tableName = QualifyTableName(tableName);
        var qualifiedTableName = Connection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName));
    }

    /// <summary>
    /// Retrieves comments for a database table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        return LoadTableComments(candidateTableName, cancellationToken);
    }

    /// <summary>
    /// Retrieves a table's comments.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for a table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTableComments> LoadTableComments(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IRelationalDatabaseTableComments> LoadTableCommentsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var commentsData = await Connection.QueryAsync(
            Queries.GetTableComments.Sql,
            new GetTableComments.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

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

    private static Option<string> GetFirstCommentByType(IEnumerable<GetTableComments.Result> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        return commentsData
            .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
            .FirstOrDefault();
    }

    private static IReadOnlyDictionary<Identifier, Option<string>> GetCommentLookupByType(IEnumerable<GetTableComments.Result> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

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
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

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