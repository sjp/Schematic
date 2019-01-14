using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
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

        public async Task<IReadOnlyCollection<IOracleDatabasePackage>> GetAllPackages(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(
                PackagesQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            var packageNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var packages = new List<IOracleDatabasePackage>();

            foreach (var packageName in packageNames)
            {
                var package = LoadPackage(packageName, cancellationToken);
                await package.IfSome(p => packages.Add(p)).ConfigureAwait(false);
            }

            return packages;
        }

        protected virtual string PackagesQuery => PackagesQuerySql;

        private const string PackagesQuerySql = @"
select
    OWNER as SchemaName,
    OBJECT_NAME as ObjectName
from SYS.ALL_OBJECTS
where ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE = 'PACKAGE'
order by OWNER, OBJECT_NAME";

        public OptionAsync<IOracleDatabasePackage> GetPackage(Identifier packageName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var candidatePackageName = QualifyPackageName(packageName);
            return LoadPackage(candidatePackageName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedPackageName(Identifier packageName, CancellationToken cancellationToken = default(CancellationToken))
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
            return LoadPackageAsyncCore(candidatePackageName, cancellationToken).ToAsync();
        }

        private async Task<Option<IOracleDatabasePackage>> LoadPackageAsyncCore(Identifier packageName, CancellationToken cancellationToken)
        {
            var candidatePackageName = QualifyPackageName(packageName);
            var resolvedPackageNameOption = GetResolvedPackageName(candidatePackageName, cancellationToken);
            var resolvedPackageNameOptionIsNone = await resolvedPackageNameOption.IsNone.ConfigureAwait(false);
            if (resolvedPackageNameOptionIsNone)
                return Option<IOracleDatabasePackage>.None;

            var resolvedPackageName = await resolvedPackageNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var packageSpec = await LoadPackageSpecificationAsync(resolvedPackageName, cancellationToken).ConfigureAwait(false);
            var packageBodyOption = LoadPackageBody(resolvedPackageName, cancellationToken);
            var packageBody = await packageBodyOption.ToOption().ConfigureAwait(false);

            var package = new OracleDatabasePackage(resolvedPackageName, packageSpec, packageBody);
            return Option<IOracleDatabasePackage>.Some(package);
        }

        protected virtual Task<string> LoadPackageSpecificationAsync(Identifier packageName, CancellationToken cancellationToken)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            return LoadPackageSpecificationAsyncCore(packageName, cancellationToken);
        }

        private async Task<string> LoadPackageSpecificationAsyncCore(Identifier packageName, CancellationToken cancellationToken)
        {
            // fast path
            if (packageName.Schema == IdentifierDefaults.Schema)
            {
                var userLines = await Connection.QueryAsync<string>(
                    UserPackageSpecificationQuery,
                    new { PackageName = packageName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                return !userLines.Empty()
                    ? userLines.Join(string.Empty)
                    : null;
            }

            var lines = await Connection.QueryAsync<string>(
                PackageSpecificationQuery,
                new { SchemaName = packageName.Schema, PackageName = packageName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            return !lines.Empty()
                ? lines.Join(string.Empty)
                : null;
        }

        protected virtual OptionAsync<string> LoadPackageBody(Identifier packageName, CancellationToken cancellationToken)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            return LoadPackageBodyAsyncCore(packageName, cancellationToken).ToAsync();
        }

        private async Task<Option<string>> LoadPackageBodyAsyncCore(Identifier packageName, CancellationToken cancellationToken)
        {
            // fast path
            if (packageName.Schema == IdentifierDefaults.Schema)
            {
                var userLines = await Connection.QueryAsync<string>(
                    UserPackageBodyQuery,
                    new { PackageName = packageName.LocalName },
                    cancellationToken
                ).ConfigureAwait(false);

                return !userLines.Empty()
                    ? Option<string>.Some(userLines.Join(string.Empty))
                    : Option<string>.None;
            }

            var lines = await Connection.QueryAsync<string>(
                PackageBodyQuery,
                new { SchemaName = packageName.Schema, PackageName = packageName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            return !lines.Empty()
                ? Option<string>.Some(lines.Join(string.Empty))
                : Option<string>.None;
        }

        protected virtual string PackageSpecificationQuery => PackageSpecificationQuerySql;

        private const string PackageSpecificationQuerySql = @"
select TEXT
from SYS.ALL_SOURCE
where OWNER = :SchemaName and NAME = :PackageName AND TYPE = 'PACKAGE'
order by LINE";

        protected virtual string PackageBodyQuery => PackageBodyQuerySql;

        private const string PackageBodyQuerySql = @"
select TEXT
from SYS.ALL_SOURCE
where OWNER = :SchemaName and NAME = :PackageName AND TYPE = 'PACKAGE BODY'
order by LINE";

        protected virtual string UserPackageSpecificationQuery => UserPackageSpecificationQuerySql;

        private const string UserPackageSpecificationQuerySql = @"
select TEXT
from SYS.USER_SOURCE
where NAME = :PackageName AND TYPE = 'PACKAGE'
order by LINE";

        protected virtual string UserPackageBodyQuery => UserPackageBodyQuerySql;

        private const string UserPackageBodyQuerySql = @"
select TEXT
from SYS.USER_SOURCE
where NAME = :PackageName AND TYPE = 'PACKAGE BODY'
order by LINE";

        protected Identifier QualifyPackageName(Identifier packageName)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            var schema = packageName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, packageName.LocalName);
        }
    }
}
