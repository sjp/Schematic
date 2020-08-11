using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// An Oracle database package provider.
    /// </summary>
    /// <seealso cref="SJP.Schematic.Oracle.IOracleDatabasePackageProvider" />
    public class OracleDatabasePackageProvider : IOracleDatabasePackageProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabasePackageProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">Database identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
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
        /// Retrieves all database packages.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database packages.</returns>
        public async IAsyncEnumerable<IOracleDatabasePackage> GetAllPackages([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<RoutineData>(
                AllSourcesQuery,
                cancellationToken
            ).ConfigureAwait(false);

            var packages = queryResult
                .GroupBy(r => new { r.SchemaName, r.RoutineName })
                .Select(r =>
                {
                    var name = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, r.Key.SchemaName, r.Key.RoutineName);

                    var specLines = r
                        .Where(p => p.RoutineType == PackageObjectType)
                        .OrderBy(p => p.LineNumber)
                        .Where(p => p.Text != null)
                        .Select(p => p.Text!);
                    var bodyLines = r
                        .Where(p => p.RoutineType == PackageBodyObjectType)
                        .OrderBy(p => p.LineNumber)
                        .Where(p => p.Text != null)
                        .Select(p => p.Text!);

                    var spec = specLines.Join(string.Empty);
                    var body = bodyLines.Any()
                        ? Option<string>.Some(bodyLines.Join(string.Empty))
                        : Option<string>.None;

                    var specification = OracleUnwrapper.Unwrap(spec);
                    var packageBody = body.Map(OracleUnwrapper.Unwrap);

                    return new OracleDatabasePackage(name, specification, packageBody);
                });

            foreach (var package in packages)
                yield return package;
        }

        /// <summary>
        /// Gets a query that retrieves all package sources.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllSourcesQuery => AllSourcesQuerySql;

        private static readonly string AllSourcesQuerySql = @$"
SELECT
    OWNER as ""{ nameof(RoutineData.SchemaName) }"",
    NAME as ""{ nameof(RoutineData.RoutineName) }"",
    TYPE as ""{ nameof(RoutineData.RoutineType) }"",
    LINE as ""{ nameof(RoutineData.LineNumber) }"",
    TEXT as ""{ nameof(RoutineData.Text) }""
FROM SYS.ALL_SOURCE
    WHERE TYPE in ('PACKAGE', 'PACKAGE BODY')
ORDER BY OWNER, NAME, LINE";

        /// <summary>
        /// Retrieves a database package, if available.
        /// </summary>
        /// <param name="packageName">A database package name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database package in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <c>null</c>.</exception>
        public OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var candidatePackageName = QualifyPackageName(packageName);
            return LoadPackage(candidatePackageName, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the package. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="packageName">A package name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A package name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedPackageName(Identifier packageName, CancellationToken cancellationToken = default)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

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
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedPackageNameStrict(Identifier packageName, CancellationToken cancellationToken)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var candidatePackageName = QualifyPackageName(packageName);
            var qualifiedPackageName = Connection.QueryFirstOrNone<QualifiedName>(
                PackageNameQuery,
                new { SchemaName = candidatePackageName.Schema, PackageName = candidatePackageName.LocalName },
                cancellationToken
            );

            return qualifiedPackageName.Map(name => Identifier.CreateQualifiedIdentifier(candidatePackageName.Server, candidatePackageName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that retrieves the resolved name of a package.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PackageNameQuery => PackageNameQuerySql;

        private static readonly string PackageNameQuerySql = @$"
select
    OWNER as ""{ nameof(QualifiedName.SchemaName) }"",
    OBJECT_NAME as ""{ nameof(QualifiedName.ObjectName) }""
from SYS.ALL_OBJECTS
where OWNER = :SchemaName and OBJECT_NAME = :PackageName
    and ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE = 'PACKAGE'";

        /// <summary>
        /// Retrieves a package from the database, if available.
        /// </summary>
        /// <param name="packageName">A package name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database package, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IOracleDatabasePackage> LoadPackage(Identifier packageName, CancellationToken cancellationToken)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var candidatePackageName = QualifyPackageName(packageName);
            return GetResolvedPackageName(candidatePackageName, cancellationToken)
                .MapAsync(name => LoadPackageAsyncCore(name, cancellationToken));
        }

        private async Task<IOracleDatabasePackage> LoadPackageAsyncCore(Identifier packageName, CancellationToken cancellationToken)
        {
            // fast path
            if (packageName.Schema == IdentifierDefaults.Schema)
            {
                var lines = await Connection.QueryAsync<PackageData>(
                    UserPackageDefinitionQuery,
                    new { PackageName = packageName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                var spec = lines
                    .Where(p => p.SourceType == PackageObjectType)
                    .OrderBy(p => p.LineNumber)
                    .Where(p => p.Text != null)
                    .Select(p => p.Text!)
                    .Join(string.Empty);
                var bodyLines = lines
                    .Where(p => p.SourceType == PackageBodyObjectType)
                    .OrderBy(p => p.LineNumber)
                    .Where(p => p.Text != null)
                    .Select(p => p.Text!);

                var body = bodyLines.Any()
                    ? Option<string>.Some(bodyLines.Join(string.Empty))
                    : Option<string>.None;

                var specification = OracleUnwrapper.Unwrap(spec);
                var packageBody = body.Map(OracleUnwrapper.Unwrap);

                return new OracleDatabasePackage(packageName, specification, packageBody);
            }
            else
            {
                var lines = await Connection.QueryAsync<PackageData>(
                    PackageDefinitionQuery,
                    new { SchemaName = packageName.Schema, PackageName = packageName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                var spec = lines
                    .Where(p => p.SourceType == PackageObjectType)
                    .OrderBy(p => p.LineNumber)
                    .Where(p => p.Text != null)
                    .Select(p => p.Text!)
                    .Join(string.Empty);
                var bodyLines = lines
                    .Where(p => p.SourceType == PackageBodyObjectType)
                    .OrderBy(p => p.LineNumber)
                    .Where(p => p.Text != null)
                    .Select(p => p.Text!);

                var body = bodyLines.Any()
                    ? Option<string>.Some(bodyLines.Join(string.Empty))
                    : Option<string>.None;

                var specification = OracleUnwrapper.Unwrap(spec);
                var packageBody = body.Map(OracleUnwrapper.Unwrap);

                return new OracleDatabasePackage(packageName, specification, packageBody);
            }
        }

        /// <summary>
        /// Gets a query that retrieves the package definition for a package in any visible schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string PackageDefinitionQuery => PackageDefinitionQuerySql;

        private static readonly string PackageDefinitionQuerySql = @$"
select
    TYPE as ""{ nameof(PackageData.SourceType) }"",
    LINE as ""{ nameof(PackageData.LineNumber) }"",
    TEXT as ""{ nameof(PackageData.Text) }""
from SYS.ALL_SOURCE
where OWNER = :SchemaName and NAME = :PackageName and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE";

        /// <summary>
        /// Gets a query that retrieves the package definition for a package in the user's schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserPackageDefinitionQuery => UserPackageDefinitionQuerySql;

        private static readonly string UserPackageDefinitionQuerySql = @$"
select
    TYPE as ""{ nameof(PackageData.SourceType) }"",
    LINE as ""{ nameof(PackageData.LineNumber) }"",
    TEXT as ""{ nameof(PackageData.Text) }""
from SYS.USER_SOURCE
where NAME = :PackageName and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE";

        /// <summary>
        /// Qualifies the name of the package, using known identifier defaults.
        /// </summary>
        /// <param name="packageName">A package name to qualify.</param>
        /// <returns>A package name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <c>null</c>.</exception>
        protected Identifier QualifyPackageName(Identifier packageName)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var schema = packageName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, packageName.LocalName);
        }

        private const string PackageObjectType = "PACKAGE";
        private const string PackageBodyObjectType = "PACKAGE BODY";
    }
}
