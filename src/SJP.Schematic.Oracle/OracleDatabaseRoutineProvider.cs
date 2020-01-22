using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var simpleRoutinesTask = SimpleRoutineProvider.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var packagesTask = PackageProvider.GetAllPackages(cancellationToken).ToListAsync(cancellationToken).AsTask();
            await Task.WhenAll(simpleRoutinesTask, packagesTask).ConfigureAwait(false);

            var simpleRoutines = await simpleRoutinesTask.ConfigureAwait(false);
            var packages = await packagesTask.ConfigureAwait(false);

            var routines = simpleRoutines
                .Concat(packages)
                .OrderBy(r => r.Name.Schema)
                .ThenBy(r => r.Name.LocalName);

            foreach (var routine in routines)
                yield return routine;
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
