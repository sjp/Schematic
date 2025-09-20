using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An Oracle database package provider.
/// </summary>
/// <seealso cref="IOracleDatabasePackageProvider" />
public class OracleDatabasePackageProvider : IOracleDatabasePackageProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabasePackageProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">Database identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <see langword="null" />.</exception>
    public OracleDatabasePackageProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
    /// Enumerates all database packages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database packages.</returns>
    public IAsyncEnumerable<IOracleDatabasePackage> GetAllPackages(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllPackageNames.Result>(GetAllPackageNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.PackageName))
            .Select(QualifyPackageName)
            .SelectAwait(packageName => LoadPackageAsyncCore(packageName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Retrieves all database packages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database packages.</returns>
    public async Task<IReadOnlyCollection<IOracleDatabasePackage>> GetAllPackages2(CancellationToken cancellationToken = default)
    {
        var packageNames = await Connection.QueryEnumerableAsync<GetAllPackageNames.Result>(GetAllPackageNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.PackageName))
            .Select(QualifyPackageName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await packageNames
            .Select(packageName => LoadPackageAsyncCore(packageName, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a database package, if available.
    /// </summary>
    /// <param name="packageName">A database package name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database package in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null" />.</exception>
    public OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageName);

        var candidatePackageName = QualifyPackageName(packageName);
        return LoadPackage(candidatePackageName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the package. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="packageName">A package name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A package name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedPackageName(Identifier packageName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(packageName)
            .Select(QualifyPackageName);

        return resolvedNames
            .Select(name => GetResolvedPackageNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the package without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="packageName">A package name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A package name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedPackageNameStrict(Identifier packageName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packageName);

        var candidatePackageName = QualifyPackageName(packageName);
        var qualifiedPackageName = Connection.QueryFirstOrNone(
            GetPackageName.Sql,
            new GetPackageName.Query { SchemaName = candidatePackageName.Schema!, PackageName = candidatePackageName.LocalName },
            cancellationToken
        );

        return qualifiedPackageName.Map(name => Identifier.CreateQualifiedIdentifier(candidatePackageName.Server, candidatePackageName.Database, name.SchemaName, name.PackageName));
    }

    /// <summary>
    /// Retrieves a package from the database, if available.
    /// </summary>
    /// <param name="packageName">A package name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database package, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IOracleDatabasePackage> LoadPackage(Identifier packageName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packageName);

        var candidatePackageName = QualifyPackageName(packageName);
        return GetResolvedPackageName(candidatePackageName, cancellationToken)
            .MapAsync(name => LoadPackageAsyncCore(name, cancellationToken));
    }

    private async Task<IOracleDatabasePackage> LoadPackageAsyncCore(Identifier packageName, CancellationToken cancellationToken)
    {
        if (string.Equals(packageName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)) // fast path
            return await LoadUserPackageAsyncCore(packageName, cancellationToken).ConfigureAwait(false);

        var lines = await Connection.QueryAsync(
            GetPackageDefinition.Sql,
            new GetPackageDefinition.Query { SchemaName = packageName.Schema!, PackageName = packageName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var spec = lines
            .Where(static p => string.Equals(p.RoutineType, PackageObjectType, StringComparison.Ordinal) && p.Text != null)
            .OrderBy(static p => p.LineNumber)
            .Select(static p => p.Text!)
            .Join(string.Empty);
        var bodyLines = lines
            .Where(static p => string.Equals(p.RoutineType, PackageBodyObjectType, StringComparison.Ordinal) && p.Text != null);
        var orderedBodyText = bodyLines
            .OrderBy(static p => p.LineNumber)
            .Select(static p => p.Text!);

        var body = bodyLines.Any()
            ? Option<string>.Some(orderedBodyText.Join(string.Empty))
            : Option<string>.None;

        var specification = OracleUnwrapper.Unwrap(spec);
        var packageBody = body.Map(OracleUnwrapper.Unwrap);

        return new OracleDatabasePackage(packageName, specification, packageBody);
    }

    private async Task<IOracleDatabasePackage> LoadUserPackageAsyncCore(Identifier packageName, CancellationToken cancellationToken)
    {
        var lines = await Connection.QueryAsync(
            GetUserPackageDefinition.Sql,
            new GetUserPackageDefinition.Query { PackageName = packageName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var spec = lines
            .Where(static p => string.Equals(p.RoutineType, PackageObjectType, StringComparison.Ordinal) && p.Text != null)
            .OrderBy(static p => p.LineNumber)
            .Select(static p => p.Text!)
            .Join(string.Empty);
        var bodyLines = lines
            .Where(static p => string.Equals(p.RoutineType, PackageBodyObjectType, StringComparison.Ordinal) && p.Text != null);
        var orderedBodyText = bodyLines
            .OrderBy(static p => p.LineNumber)
            .Select(static p => p.Text!);

        var body = bodyLines.Any()
            ? Option<string>.Some(orderedBodyText.Join(string.Empty))
            : Option<string>.None;

        var specification = OracleUnwrapper.Unwrap(spec);
        var packageBody = body.Map(OracleUnwrapper.Unwrap);

        return new OracleDatabasePackage(packageName, specification, packageBody);
    }

    /// <summary>
    /// Qualifies the name of the package, using known identifier defaults.
    /// </summary>
    /// <param name="packageName">A package name to qualify.</param>
    /// <returns>A package name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyPackageName(Identifier packageName)
    {
        ArgumentNullException.ThrowIfNull(packageName);

        var schema = packageName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, packageName.LocalName);
    }

    private const string PackageObjectType = "PACKAGE";
    private const string PackageBodyObjectType = "PACKAGE BODY";
}