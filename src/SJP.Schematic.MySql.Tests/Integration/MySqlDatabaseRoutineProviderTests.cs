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

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed class MySqlDatabaseRoutineProviderTests : MySqlTest
{
    private IDatabaseRoutineProvider RoutineProvider => new MySqlDatabaseRoutineProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
CREATE FUNCTION db_test_routine_1()
  RETURNS TEXT
  LANGUAGE SQL
  DETERMINISTIC
BEGIN
  RETURN 'test';
END;", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
CREATE PROCEDURE db_test_routine_2()
DETERMINISTIC
BEGIN
   COMMIT;
END", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop function db_test_routine_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop procedure db_test_routine_2", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IDatabaseRoutine> GetRoutineAsync(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return GetRoutineAsyncCore(routineName);
    }

    private async Task<IDatabaseRoutine> GetRoutineAsyncCore(Identifier routineName)
    {
        using (await _lock.LockAsync().ConfigureAwait(false))
        {
            if (!_routinesCache.TryGetValue(routineName, out var lazyRoutine))
            {
                lazyRoutine = new AsyncLazy<IDatabaseRoutine>(() => RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync());
                _routinesCache[routineName] = lazyRoutine;
            }

            return await lazyRoutine.ConfigureAwait(false);
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>> _routinesCache = [];

    [Test]
    public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutine()
    {
        var routineIsSome = await RoutineProvider.GetRoutine("db_test_routine_1").IsSome.ConfigureAwait(false);

        Assert.That(routineIsSome, Is.True);
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
    {
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(routineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_GivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutine_WhenRoutineMissing_ReturnsNone()
    {
        var routineIsNone = await RoutineProvider.GetRoutine("routine_that_doesnt_exist").IsNone.ConfigureAwait(false);

        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenEnumerated_ContainsRoutines()
    {
        var hasRoutines = await RoutineProvider.EnumerateAllRoutines()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasRoutines, Is.True);
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenEnumerated_ContainsTestRoutine()
    {
        const string routineName = "db_test_routine_1";
        var containsTestRoutine = await RoutineProvider.EnumerateAllRoutines()
            .AnyAsync(r => string.Equals(r.Name.LocalName, routineName, StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task GetAllRoutines2_WhenRetrieved_ContainsRoutines()
    {
        var routines = await RoutineProvider.GetAllRoutines2().ConfigureAwait(false);

        Assert.That(routines, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllRoutines2_WhenRetrieved_ContainsTestRoutine()
    {
        const string routineName = "db_test_routine_1";
        var routines = await RoutineProvider.GetAllRoutines2().ConfigureAwait(false);
        var containsTestRoutine = routines.Any(r => string.Equals(r.Name.LocalName, routineName, StringComparison.Ordinal));

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task Definition_ForFunction_ReturnsCorrectDefinition()
    {
        var routine = await GetRoutineAsync("db_test_routine_1").ConfigureAwait(false);

        var definition = routine.Definition;
        const string expectedDefinition = @"BEGIN
  RETURN 'test';
END";

        Assert.That(definition, Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Definition_ForStoredProcedure_ReturnsCorrectDefinition()
    {
        var routine = await GetRoutineAsync("db_test_routine_2").ConfigureAwait(false);

        var definition = routine.Definition;
        const string expectedDefinition = @"BEGIN
   COMMIT;
END";

        Assert.That(definition, Is.EqualTo(expectedDefinition));
    }
}