using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        public OracleDatabaseRoutineProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            SimpleRoutineProvider = new OracleDatabaseSimpleRoutineProvider(connection, identifierDefaults, identifierResolver);
            PackageProvider = new OracleDatabasePackageProvider(connection, identifierDefaults, identifierResolver);
        }

        protected IDatabaseRoutineProvider SimpleRoutineProvider { get; }

        protected IOracleDatabasePackageProvider PackageProvider { get; }

        public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
        {
            var simpleRoutinesTask = SimpleRoutineProvider.GetAllRoutines(cancellationToken);
            var packagesTask = PackageProvider.GetAllPackages(cancellationToken);
            await Task.WhenAll(simpleRoutinesTask, packagesTask).ConfigureAwait(false);

            var simpleRoutines = simpleRoutinesTask.Result;
            var packages = packagesTask.Result;

            return simpleRoutines
                .Concat(packages)
                .OrderBy(r => r.Name.Schema)
                .ThenBy(r => r.Name.LocalName)
                .ToList();
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return SimpleRoutineProvider.GetRoutine(routineName, cancellationToken)
                 | PackageProvider.GetPackage(routineName, cancellationToken).Map<IDatabaseRoutine>(p => p);
        }
    }
}
