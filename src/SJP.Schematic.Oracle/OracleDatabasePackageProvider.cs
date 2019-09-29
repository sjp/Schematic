using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabasePackageProvider : IOracleDatabasePackageProvider
    {
        public OracleDatabasePackageProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IOracleDatabasePackage>> GetAllPackages(CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<RoutineData>(
                AllSourcesQuery,
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IOracleDatabasePackage>();

            var packages = new List<IOracleDatabasePackage>();

            var namedPackages = queryResult.GroupBy(r => new { r.SchemaName, r.RoutineName });
            foreach (var namedPackage in namedPackages)
            {
                var name = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, namedPackage.Key.SchemaName, namedPackage.Key.RoutineName);

                var specLines = namedPackage
                    .Where(p => p.RoutineType == PackageObjectType)
                    .OrderBy(p => p.LineNumber)
                    .Where(p => p.Text != null)
                    .Select(p => p.Text!);
                var bodyLines = namedPackage
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

                var package = new OracleDatabasePackage(name, specification, packageBody);
                packages.Add(package);
            }

            return packages;
        }

        protected virtual string AllSourcesQuery => AllSourcesQuerySql;

        private const string AllSourcesQuerySql = @"
SELECT
    OWNER as SchemaName,
    NAME as RoutineName,
    TYPE as RoutineType,
    LINE as LineNumber,
    TEXT as Text
FROM SYS.ALL_SOURCE
    WHERE TYPE in ('PACKAGE', 'PACKAGE BODY')
ORDER BY OWNER, NAME, LINE";

        public OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var candidatePackageName = QualifyPackageName(packageName);
            return LoadPackage(candidatePackageName, cancellationToken);
        }

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

        protected virtual string PackageNameQuery => PackageNameQuerySql;

        private const string PackageNameQuerySql = @"
select
    OWNER as SchemaName,
    OBJECT_NAME as ObjectName
from SYS.ALL_OBJECTS
where OWNER = :SchemaName and OBJECT_NAME = :PackageName
    and ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE = 'PACKAGE'";

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

        protected virtual string PackageDefinitionQuery => PackageDefinitionQuerySql;

        private const string PackageDefinitionQuerySql = @"
select
    TYPE as SourceType,
    LINE as LineNumber,
    TEXT as Text
from SYS.ALL_SOURCE
where OWNER = :SchemaName and NAME = :PackageName and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE";

        protected virtual string UserPackageDefinitionQuery => UserPackageDefinitionQuerySql;

        private const string UserPackageDefinitionQuerySql = @"
select
    TYPE as SourceType,
    LINE as LineNumber,
    TEXT as Text
from SYS.USER_SOURCE
where NAME = :PackageName and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE";

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
