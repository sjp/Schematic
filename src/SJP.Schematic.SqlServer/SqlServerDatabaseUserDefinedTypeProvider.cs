using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A user-defined type provider for SQL Server databases. Alias types, table types and assembly
/// (CLR) types are all declared in <c>sys.types</c> and are reported here.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeProvider" />
public class SqlServerDatabaseUserDefinedTypeProvider : IDatabaseUserDefinedTypeProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseUserDefinedTypeProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqlServerDatabaseUserDefinedTypeProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    /// A type provider used to describe the type an alias type is defined over.
    /// </summary>
    /// <value>A type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new SqlServerDbTypeProvider();

    /// <summary>
    /// Enumerates all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database user-defined types.</returns>
    public async IAsyncEnumerable<IDatabaseUserDefinedType> EnumerateAllUserDefinedTypes([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // a table type's attributes and checks are read in bulk, so there is nothing to stream
        var types = await GetAllUserDefinedTypes(cancellationToken);
        foreach (var type in types)
            yield return type;
    }

    /// <summary>
    /// Gets all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database user-defined types.</returns>
    public async Task<IReadOnlyCollection<IDatabaseUserDefinedType>> GetAllUserDefinedTypes(CancellationToken cancellationToken = default)
    {
        var (definitions, attributes, checks) = await (
            Connection.QueryAsync<GetAllUserDefinedTypeDefinitions.Result>(GetAllUserDefinedTypeDefinitions.Sql, cancellationToken),
            Connection.QueryAsync<GetAllUserDefinedTypeAttributes.Result>(GetAllUserDefinedTypeAttributes.Sql, cancellationToken),
            Connection.QueryAsync<GetAllUserDefinedTypeChecks.Result>(GetAllUserDefinedTypeChecks.Sql, cancellationToken)
        ).WhenAll();

        var attributesByType = attributes
            .GroupAsDictionary(static row => Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName));
        var checksByType = checks
            .Where(static row => row.ConstraintName != null && row.Definition != null)
            .GroupAsDictionary(static row => Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName));

        return definitions
            .Select(row =>
            {
                var unqualifiedName = Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName);
                var typeAttributes = attributesByType.TryGetValue(unqualifiedName, out var attributeRows)
                    ? SqlServerUserDefinedTypeMapper.MapAttributes(attributeRows, TypeProvider)
                    : [];
                var typeChecks = checksByType.TryGetValue(unqualifiedName, out var checkRows)
                    ? SqlServerUserDefinedTypeMapper.MapChecks(checkRows)
                    : [];

                return SqlServerUserDefinedTypeMapper.MapType(QualifyUserDefinedTypeName(unqualifiedName), row, typeAttributes, typeChecks, TypeProvider);
            })
            .ToList();
    }

    /// <summary>
    /// Gets a database user-defined type.
    /// </summary>
    /// <param name="typeName">A database user-defined type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database user-defined type in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseUserDefinedType> GetUserDefinedType(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        return LoadUserDefinedType(candidateTypeName, cancellationToken);
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

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        var qualifiedTypeName = Connection.QueryFirstOrNone(
            GetUserDefinedTypeName.Sql,
            new GetUserDefinedTypeName.Query { SchemaName = candidateTypeName.Schema!, TypeName = candidateTypeName.LocalName },
            cancellationToken
        );

        return qualifiedTypeName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTypeName.Server, candidateTypeName.Database, name.SchemaName, name.TypeName));
    }

    /// <summary>
    /// Retrieves database user-defined type information.
    /// </summary>
    /// <param name="typeName">A database user-defined type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database user-defined type in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseUserDefinedType> LoadUserDefinedType(Identifier typeName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var candidateTypeName = QualifyUserDefinedTypeName(typeName);
        return GetResolvedUserDefinedTypeName(candidateTypeName, cancellationToken)
            .Bind(name => LoadUserDefinedTypeData(name, cancellationToken));
    }

    private OptionAsync<IDatabaseUserDefinedType> LoadUserDefinedTypeData(Identifier typeName, CancellationToken cancellationToken)
    {
        return Connection.QueryFirstOrNone(
            GetUserDefinedTypeDefinition.Sql,
            new GetUserDefinedTypeDefinition.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
            cancellationToken
        ).MapAsync(row => BuildUserDefinedTypeAsync(typeName, row, cancellationToken));
    }

    private async Task<IDatabaseUserDefinedType> BuildUserDefinedTypeAsync(Identifier typeName, GetUserDefinedTypeDefinition.Result row, CancellationToken cancellationToken)
    {
        // only a table type carries attributes and checks; everything else is described by its own row
        if (!row.IsTableType)
            return SqlServerUserDefinedTypeMapper.MapType(typeName, row, [], [], TypeProvider);

        var (attributeRows, checkRows) = await (
            Connection.QueryAsync(
                GetUserDefinedTypeAttributes.Sql,
                new GetUserDefinedTypeAttributes.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
                cancellationToken),
            Connection.QueryAsync(
                GetUserDefinedTypeChecks.Sql,
                new GetUserDefinedTypeChecks.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
                cancellationToken)
        ).WhenAll();

        var attributes = SqlServerUserDefinedTypeMapper.MapAttributes(attributeRows, TypeProvider);
        var checks = SqlServerUserDefinedTypeMapper.MapChecks(checkRows.Where(static checkRow => checkRow.ConstraintName != null && checkRow.Definition != null));

        return SqlServerUserDefinedTypeMapper.MapType(typeName, row, attributes, checks, TypeProvider);
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
