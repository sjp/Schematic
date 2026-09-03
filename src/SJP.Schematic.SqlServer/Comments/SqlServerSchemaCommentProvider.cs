using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer.Comments;

/// <summary>
/// A database schema comment provider for SQL Server. Comments are stored as an extended property
/// on the schema itself.
/// </summary>
/// <seealso cref="IDatabaseSchemaCommentProvider" />
public class SqlServerSchemaCommentProvider : IDatabaseSchemaCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerSchemaCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerSchemaCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    protected string CommentProperty { get; } = "MS_Description";

    /// <summary>
    /// Enumerates comments for all database schemas.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database schema comments.</returns>
    public IAsyncEnumerable<IDatabaseSchemaComments> EnumerateAllSchemaComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync(
                Queries.GetAllSchemaComments.Sql,
                new Queries.GetAllSchemaComments.Query { CommentProperty = CommentProperty },
                cancellationToken
            )
            .Select(static row => MapComments(row.SchemaName, row.Comment));
    }

    /// <summary>
    /// Retrieves comments for all database schemas.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database schema comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSchemaComments>> GetAllSchemaComments(CancellationToken cancellationToken = default)
    {
        var comments = await Connection.QueryAsync(
            Queries.GetAllSchemaComments.Sql,
            new Queries.GetAllSchemaComments.Query { CommentProperty = CommentProperty },
            cancellationToken
        );

        return comments.Select(static row => MapComments(row.SchemaName, row.Comment)).ToList();
    }

    /// <summary>
    /// Retrieves comments for a particular database schema.
    /// </summary>
    /// <param name="schemaName">The name of a database schema.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSchemaComments}" /> instance which holds the value of the schema's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSchemaComments> GetSchemaComments(Identifier schemaName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schemaName);

        return Connection.QueryFirstOrNone(
                Queries.GetSchemaComments.Sql,
                new Queries.GetSchemaComments.Query { SchemaName = schemaName.LocalName, CommentProperty = CommentProperty },
                cancellationToken
            )
            .Map(static row => MapComments(row.SchemaName, row.Comment));
    }

    private static IDatabaseSchemaComments MapComments(string schemaName, string? comment)
    {
        var schemaComment = !comment.IsNullOrWhiteSpace()
            ? Option<string>.Some(comment)
            : Option<string>.None;

        return new DatabaseSchemaComments(Identifier.CreateQualifiedIdentifier(schemaName), schemaComment);
    }
}
