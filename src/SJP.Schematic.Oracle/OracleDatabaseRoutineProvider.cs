using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleDatabaseRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(identifierDefaults);
        ArgumentNullException.ThrowIfNull(identifierResolver);

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
    /// Enumerates all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (simpleRoutines, packages) = await (
            SimpleRoutineProvider.EnumerateAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask(),
            PackageProvider.GetAllPackages(cancellationToken).ToListAsync(cancellationToken).AsTask()
        ).WhenAll().ConfigureAwait(false);

        var routines = simpleRoutines
            .Concat(packages)
            .OrderBy(static r => r.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static r => r.Name.LocalName, StringComparer.Ordinal);

        foreach (var routine in routines)
            yield return routine;
    }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines2(CancellationToken cancellationToken = default)
    {
        var (simpleRoutines, packages) = await (
            SimpleRoutineProvider.GetAllRoutines2(cancellationToken),
            PackageProvider.GetAllPackages2(cancellationToken)
        ).WhenAll().ConfigureAwait(false);

        return simpleRoutines
            .Concat(packages)
            .OrderBy(static r => r.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static r => r.Name.LocalName, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Gets a database routine.
    /// </summary>
    /// <param name="routineName">A database routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return SimpleRoutineProvider.GetRoutine(routineName, cancellationToken)
             | PackageProvider.GetPackage(routineName, cancellationToken).Map<IDatabaseRoutine>(static p => p);
    }
}