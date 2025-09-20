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
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle.Comments;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
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
    /// Enumerates comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .SelectAwait(tableName => LoadTableCommentsAsyncCore(tableName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Retrieves comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments2(CancellationToken cancellationToken = default)
    {
        var tableNames = await Connection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var tableCommentTasks = tableNames
            .Select(tableName => LoadTableCommentsAsyncCore(tableName, cancellationToken))
            .ToArray();

        await Task.WhenAll(tableCommentTasks).ConfigureAwait(false);

        return tableCommentTasks
            .Select(t => t.Result)
            .ToArray();
    }

    /// <summary>
    /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

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
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        var qualifiedTableName = Connection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
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
        if (string.Equals(tableName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)) // fast path
            return await LoadUserTableCommentsAsyncCore(tableName, cancellationToken).ConfigureAwait(false);

        var result = await Connection.QueryAsync(
            Queries.GetTableComments.Sql,
            new GetTableComments.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var commentData = result.Select(r => new CommentData
        {
            ColumnName = r.ColumnName,
            Comment = r.Comment,
            ObjectType = r.ObjectType,
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
        var result = await Connection.QueryAsync(
            GetUserTableComments.Sql,
            new GetUserTableComments.Query { TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var commentData = result.Select(r => new CommentData
        {
            ColumnName = r.ColumnName,
            Comment = r.Comment,
            ObjectType = r.ObjectType,
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

    private static Option<string> GetTableComment(IEnumerable<CommentData> commentsData)
    {
        ArgumentNullException.ThrowIfNull(commentsData);

        return commentsData
            .Where(static c => string.Equals(c.ObjectType, Constants.Table, StringComparison.Ordinal))
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
            .FirstOrDefault();
    }

    private static IReadOnlyDictionary<Identifier, Option<string>> GetColumnComments(IEnumerable<CommentData> commentsData)
    {
        ArgumentNullException.ThrowIfNull(commentsData);

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
    }

    private sealed record CommentData
    {
        public string? ColumnName { get; init; }

        public string? ObjectType { get; init; }

        public string? Comment { get; init; }
    }
}