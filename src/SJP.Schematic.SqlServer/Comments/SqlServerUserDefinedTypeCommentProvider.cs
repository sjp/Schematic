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
/// A database user-defined type comment provider for SQL Server.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeCommentProvider" />
public class SqlServerUserDefinedTypeCommentProvider : IDatabaseUserDefinedTypeCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerUserDefinedTypeCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerUserDefinedTypeCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    /// Enumerates comments for all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database user-defined type comments.</returns>
    public IAsyncEnumerable<IDatabaseUserDefinedTypeComments> EnumerateAllUserDefinedTypeComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllUserDefinedTypeNames.Result>(GetAllUserDefinedTypeNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TypeName))
            .Select(QualifyUserDefinedTypeName)
            .SelectAwait(LoadUserDefinedTypeCommentsAsyncCore);
    }

    /// <summary>
    /// Retrieves comments for all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database user-defined type comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> GetAllUserDefinedTypeComments(CancellationToken cancellationToken = default)
    {
        var typeNames = await Connection.QueryEnumerableAsync<GetAllUserDefinedTypeNames.Result>(GetAllUserDefinedTypeNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TypeName))
            .Select(QualifyUserDefinedTypeName)
            .ToListAsync(cancellationToken);

        return await typeNames
            .Select(typeName => LoadUserDefinedTypeCommentsAsyncCore(typeName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets the resolved name of the user-defined type. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="typeName">A user-defined type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A type name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedUserDefinedTypeName(Identifier typeName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        typeName = QualifyUserDefinedTypeName(typeName);
        var qualifiedTypeName = Connection.QueryFirstOrNone(
            GetUserDefinedTypeName.Sql,
            new GetUserDefinedTypeName.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
            cancellationToken
        );

        return qualifiedTypeName.Map(name => Identifier.CreateQualifiedIdentifier(typeName.Server, typeName.Database, name.SchemaName, name.TypeName));
    }

    /// <summary>
    /// Retrieves comments for a particular database user-defined type.
    /// </summary>
    /// <param name="typeName">The name of a database user-defined type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseUserDefinedTypeComments}" /> instance which holds the value of the type's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseUserDefinedTypeComments> GetUserDefinedTypeComments(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        return LoadUserDefinedTypeComments(candidateTypeName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database user-defined type.
    /// </summary>
    /// <param name="typeName">The name of a database user-defined type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseUserDefinedTypeComments}" /> instance which holds the value of the type's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseUserDefinedTypeComments> LoadUserDefinedTypeComments(Identifier typeName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        return GetResolvedUserDefinedTypeName(candidateTypeName, cancellationToken)
            .MapAsync(name => LoadUserDefinedTypeCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseUserDefinedTypeComments> LoadUserDefinedTypeCommentsAsyncCore(Identifier typeName, CancellationToken cancellationToken)
    {
        var queryResult = await Connection.QueryAsync(
            Queries.GetUserDefinedTypeComments.Sql,
            new GetUserDefinedTypeComments.Query
            {
                SchemaName = typeName.Schema!,
                TypeName = typeName.LocalName,
                CommentProperty = CommentProperty,
            },
            cancellationToken
        );

        var typeComment = queryResult
            .Where(static r => string.Equals(r.ObjectType, Constants.Type, StringComparison.Ordinal))
            .Select(static r => !r.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(r.Comment) : Option<string>.None)
            .FirstOrDefault();

        return new DatabaseUserDefinedTypeComments(typeName, typeComment);
    }

    /// <summary>
    /// Qualifies the name of a user-defined type.
    /// </summary>
    /// <param name="typeName">A user-defined type name.</param>
    /// <returns>A type name that is at least as qualified as the given type name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyUserDefinedTypeName(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var schema = typeName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, typeName.LocalName);
    }

    private static class Constants
    {
        public const string Type = "TYPE";
    }
}
