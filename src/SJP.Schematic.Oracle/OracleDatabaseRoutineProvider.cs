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

        _connectionFactory = connection;
        _simpleRoutineProvider = new OracleDatabaseSimpleRoutineProvider(connection, identifierDefaults, identifierResolver);
        _packageProvider = new OracleDatabasePackageProvider(connection, identifierDefaults, identifierResolver);
        SimpleRoutineProvider = _simpleRoutineProvider;
        PackageProvider = _packageProvider;
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
        var (simpleRoutineNames, packageNames) = await (
            _simpleRoutineProvider.GetAllRoutineNamesAsync(cancellationToken),
            _packageProvider.GetAllPackageNamesAsync(cancellationToken)
        ).WhenAll();

        // order the names rather than the loaded routines, so that each routine can be returned as soon as it is loaded
        var routineNames = simpleRoutineNames
            .Select(static name => (Name: name, IsPackage: false))
            .Concat(packageNames.Select(static name => (Name: name, IsPackage: true)))
            .OrderBy(static r => r.Name.Schema, StringComparer.Ordinal)
            .ThenBy(static r => r.Name.LocalName, StringComparer.Ordinal);

        var routines = routineNames.SelectOrderedPrefetchAsync(LoadRoutineAsyncCore, Math.Max(1, _connectionFactory.MaxConcurrentQueries), cancellationToken);

        await foreach (var routine in routines.WithCancellation(cancellationToken))
            yield return routine;
    }

    /// <summary>
    /// Gets all database routines.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database routines.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default)
    {
        var (simpleRoutines, packages) = await (
            SimpleRoutineProvider.GetAllRoutines(cancellationToken),
            PackageProvider.GetAllPackages(cancellationToken)
        ).WhenAll();

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
            .OrElse(() => PackageProvider.GetPackage(routineName, cancellationToken).Map<IDatabaseRoutine>(static p => p));
    }

    private async Task<IDatabaseRoutine> LoadRoutineAsyncCore((Identifier Name, bool IsPackage) routine, CancellationToken cancellationToken)
    {
        return routine.IsPackage
            ? await _packageProvider.LoadPackageAsyncCore(routine.Name, cancellationToken)
            : await _simpleRoutineProvider.LoadRoutineAsyncCore(routine.Name, cancellationToken);
    }

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly OracleDatabaseSimpleRoutineProvider _simpleRoutineProvider;
    private readonly OracleDatabasePackageProvider _packageProvider;
}