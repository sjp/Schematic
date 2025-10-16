using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseRoutineProviderTests : PostgreSqlTest
{
    private IDatabaseRoutineProvider RoutineProvider => new PostgreSqlDatabaseRoutineProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"CREATE FUNCTION db_test_routine_1(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop function db_test_routine_1(integer)", CancellationToken.None);
    }

    private Task<IDatabaseRoutine> GetRoutineAsync(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return GetRoutineAsyncCore(routineName);
    }

    private async Task<IDatabaseRoutine> GetRoutineAsyncCore(Identifier routineName)
    {
        using (await _lock.LockAsync())
        {
            if (!_routinesCache.TryGetValue(routineName, out var lazyRoutine))
            {
                lazyRoutine = new AsyncLazy<IDatabaseRoutine>(() => RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync());
                _routinesCache[routineName] = lazyRoutine;
            }

            return await lazyRoutine;
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>> _routinesCache = [];

    [Test]
    public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutine()
    {
        var routineIsSome = await RoutineProvider.GetRoutine("db_test_routine_1").IsSome;
        Assert.That(routineIsSome, Is.True);
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
    {
        const string routineName = "db_test_routine_1";
        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name.LocalName, Is.EqualTo(routineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(routineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutineMissing_ReturnsNone()
    {
        var routineIsNone = await RoutineProvider.GetRoutine("routine_that_doesnt_exist").IsNone;
        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenEnumerated_ContainsRoutines()
    {
        var hasRoutines = await RoutineProvider.EnumerateAllRoutines().AnyAsync();

        Assert.That(hasRoutines, Is.True);
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenEnumerated_ContainsTestRoutine()
    {
        var containsTestRoutine = await RoutineProvider.EnumerateAllRoutines()
            .AnyAsync(r => string.Equals(r.Name.LocalName, "db_test_routine_1", StringComparison.Ordinal));

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task GetAllRoutines_WhenRetrieved_ContainsRoutines()
    {
        var routines = await RoutineProvider.GetAllRoutines();

        Assert.That(routines, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllRoutines_WhenRetrieved_ContainsTestRoutine()
    {
        var routines = await RoutineProvider.GetAllRoutines();
        var containsTestRoutine = routines.Any(r => string.Equals(r.Name.LocalName, "db_test_routine_1", StringComparison.Ordinal));

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task Definition_ForFunction_ReturnsCorrectDefinition()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        const string expectedDefinition = @"
BEGIN
    RETURN val + 1;
END; ";

        Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
    }
}