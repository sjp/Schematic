using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A user-defined type provider for Oracle databases. Object types and collection types (varrays
/// and nested tables) are reported; Oracle declares neither domains nor enumerations.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeProvider" />
public class OracleDatabaseUserDefinedTypeProvider : IDatabaseUserDefinedTypeProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseUserDefinedTypeProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public OracleDatabaseUserDefinedTypeProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    /// A type provider used to describe the element type of a collection type.
    /// </summary>
    /// <value>A type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new OracleDbTypeProvider();

    /// <summary>
    /// Enumerates all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database user-defined types.</returns>
    public async IAsyncEnumerable<IDatabaseUserDefinedType> EnumerateAllUserDefinedTypes([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // attributes and specifications are read in bulk for every type at once, so there is nothing to stream
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
        var (definitions, attributes, specifications) = await (
            Connection.QueryAsync<GetAllUserDefinedTypes.Result>(Queries.GetAllUserDefinedTypes.Sql, cancellationToken),
            Connection.QueryAsync<GetAllUserDefinedTypeAttributes.Result>(GetAllUserDefinedTypeAttributes.Sql, cancellationToken),
            Connection.QueryAsync<GetAllUserDefinedTypeSpecifications.Result>(GetAllUserDefinedTypeSpecifications.Sql, cancellationToken)
        ).WhenAll();

        var attributesByType = attributes.GroupAsDictionary(static row => Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName));
        var specificationsByType = specifications.GroupAsDictionary(static row => Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName));

        return definitions
            .Select(row =>
            {
                var unqualifiedName = Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TypeName);
                var typeAttributes = attributesByType.TryGetValue(unqualifiedName, out var attributeRows)
                    ? OracleUserDefinedTypeMapper.MapAttributes(attributeRows, TypeProvider)
                    : [];
                var specification = specificationsByType.TryGetValue(unqualifiedName, out var specificationRows)
                    ? BuildSpecification(specificationRows.OrderBy(static line => line.LineNumber).Select(static line => line.Definition))
                    : Option<string>.None;

                return OracleUserDefinedTypeMapper.MapType(QualifyUserDefinedTypeName(unqualifiedName), row, typeAttributes, specification, TypeProvider);
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
        var (attributeRows, specificationLines) = await (
            Connection.QueryAsync(
                GetUserDefinedTypeAttributes.Sql,
                new GetUserDefinedTypeAttributes.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
                cancellationToken),
            Connection.QueryAsync(
                GetUserDefinedTypeSpecification.Sql,
                new GetUserDefinedTypeSpecification.Query { SchemaName = typeName.Schema!, TypeName = typeName.LocalName },
                cancellationToken)
        ).WhenAll();

        var attributes = OracleUserDefinedTypeMapper.MapAttributes(attributeRows, TypeProvider);
        var specification = BuildSpecification(specificationLines.Select(static line => line.Definition));

        return OracleUserDefinedTypeMapper.MapType(typeName, row, attributes, specification, TypeProvider);
    }

    // ALL_SOURCE stores a type's specification one line at a time, so the lines are rejoined here.
    private static Option<string> BuildSpecification(IEnumerable<string?> lines)
    {
        var builder = StringBuilderCache.Acquire();
        foreach (var line in lines)
            builder.Append(line);

        var definition = builder.GetStringAndRelease();
        return !definition.IsNullOrWhiteSpace()
            ? Option<string>.Some(definition)
            : Option<string>.None;
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
