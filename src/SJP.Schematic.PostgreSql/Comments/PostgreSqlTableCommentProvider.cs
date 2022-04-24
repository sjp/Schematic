﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql.Comments;

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
        var queryResult = await Connection.QueryAsync<GetAllTableNames.Result>(TablesQuery, cancellationToken).ConfigureAwait(false);
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
        var qualifiedTableName = Connection.QueryFirstOrNone<GetTableName.Result>(
            TableNameQuery,
            new GetTableName.Query { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
    }

    /// <summary>
    /// A SQL query definition that resolves a table name for the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableNameQuery => GetTableName.Sql;

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
        var result = await Connection.QueryAsync<GetTableComments.Result>(
            TableCommentsQuery,
            new GetTableComments.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
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
    protected virtual string TablesQuery => GetAllTableNames.Sql;

    /// <summary>
    /// A SQL query definition which retrieves all comment information for a particular table.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string TableCommentsQuery => Queries.GetTableComments.Sql;

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

    private sealed record CommentData
    {
        public string? ObjectName { get; init; }

        public string? ObjectType { get; init; }

        public string? Comment { get; init; }
    }
}