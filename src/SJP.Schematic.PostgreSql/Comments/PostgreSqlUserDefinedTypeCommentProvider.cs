using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql.Comments;

/// <summary>
/// A database user-defined type comment provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeCommentProvider" />
public class PostgreSqlUserDefinedTypeCommentProvider : IDatabaseUserDefinedTypeCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlUserDefinedTypeCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlUserDefinedTypeCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
    /// Enumerates comments for all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database user-defined type comments.</returns>
    public IAsyncEnumerable<IDatabaseUserDefinedTypeComments> EnumerateAllUserDefinedTypeComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllUserDefinedTypeComments.Result>(Queries.GetAllUserDefinedTypeComments.Sql, cancellationToken)
            .Select(BuildComments);
    }

    /// <summary>
    /// Retrieves comments for all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database user-defined type comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> GetAllUserDefinedTypeComments(CancellationToken cancellationToken = default)
    {
        return await Connection.QueryEnumerableAsync<GetAllUserDefinedTypeComments.Result>(Queries.GetAllUserDefinedTypeComments.Sql, cancellationToken)
            .Select(BuildComments)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the user-defined type. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="typeName">A user-defined type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A type name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedUserDefinedTypeName(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(typeName)
            .Select(QualifyUserDefinedTypeName);

        return resolvedNames
            .Select(name => GetResolvedUserDefinedTypeNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the user-defined type without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="typeName">A user-defined type name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A type name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedUserDefinedTypeNameStrict(Identifier typeName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        var qualifiedTypeName = Connection.QueryFirstOrNone(
            GetUserDefinedTypeName.Sql,
            new GetUserDefinedTypeName.Query { SchemaName = candidateTypeName.Schema!, TypeName = candidateTypeName.LocalName },
            cancellationToken
        );

        return qualifiedTypeName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTypeName.Server, candidateTypeName.Database, name.SchemaName, name.TypeName));
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
            .Bind(name => Connection.QueryFirstOrNone(
                Queries.GetUserDefinedTypeComments.Sql,
                new GetUserDefinedTypeComments.Query { SchemaName = name.Schema!, TypeName = name.LocalName },
                cancellationToken
            ).Map<IDatabaseUserDefinedTypeComments>(c =>
            {
                var comment = !c.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(c.Comment)
                    : Option<string>.None;
                return new DatabaseUserDefinedTypeComments(name, comment);
            }));
    }

    private IDatabaseUserDefinedTypeComments BuildComments(GetAllUserDefinedTypeComments.Result commentData)
    {
        var qualifiedName = QualifyUserDefinedTypeName(Identifier.CreateQualifiedIdentifier(commentData.SchemaName, commentData.TypeName));
        var comment = !commentData.Comment.IsNullOrWhiteSpace()
            ? Option<string>.Some(commentData.Comment)
            : Option<string>.None;

        return new DatabaseUserDefinedTypeComments(qualifiedName, comment);
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
}
