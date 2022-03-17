using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database routine provider for Oracle databases.
/// </summary>
/// <seealso cref="IDatabaseRoutineProvider" />
public class OracleDatabaseRoutineProvider : IDatabaseRoutineProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseRoutineProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
    public OracleDatabaseRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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

    /// <summary>
    /// Gets a simple routine provider. Retrieves only database functions and procedures, not packages.
    /// </summary>
    /// <value>A routine provider.</value>
    protected IDatabaseRoutineProvider SimpleRoutineProvider { get; }

    /// <summary>
    /// Gets a package object provider. Retrieves only database packages.
    /// </summary>
    /// <value>A package provider.</value>
    protected IOracleDatabasePackageProvider PackageProvider { get; }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var simpleRoutinesTask = SimpleRoutineProvider.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask();
        var packagesTask = PackageProvider.GetAllPackages(cancellationToken).ToListAsync(cancellationToken).AsTask();

        await Task.WhenAll(simpleRoutinesTask, packagesTask).ConfigureAwait(false);

        var simpleRoutines = await simpleRoutinesTask.ConfigureAwait(false);
        var packages = await packagesTask.ConfigureAwait(false);

        var routines = simpleRoutines
            .Concat(packages)
            .OrderBy(static r => r.Name.Schema)
            .ThenBy(static r => r.Name.LocalName);

        foreach (var routine in routines)
            yield return routine;
    }

    /// <summary>
    /// Gets a database routine.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        if (routineName == null)
            throw new ArgumentNullException(nameof(routineName));

        return SimpleRoutineProvider.GetRoutine(routineName, cancellationToken)
             | PackageProvider.GetPackage(routineName, cancellationToken).Map<IDatabaseRoutine>(static p => p);
    }
}
