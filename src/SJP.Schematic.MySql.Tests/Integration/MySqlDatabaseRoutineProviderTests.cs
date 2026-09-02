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
END;", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
CREATE PROCEDURE db_test_routine_2()
DETERMINISTIC
BEGIN
   COMMIT;
END", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
CREATE PROCEDURE db_test_routine_3(IN first_arg INT, OUT second_arg VARCHAR(50))
DETERMINISTIC
BEGIN
   SET second_arg = 'test';
END", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop function db_test_routine_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop procedure db_test_routine_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop procedure db_test_routine_3", CancellationToken.None);
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
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(routineName));
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
    public async Task GetRoutine_GivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
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
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");

        var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync();

        Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
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
        const string routineName = "db_test_routine_1";
        var containsTestRoutine = await RoutineProvider.EnumerateAllRoutines()
            .AnyAsync(r => string.Equals(r.Name.LocalName, routineName, StringComparison.Ordinal));

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
        const string routineName = "db_test_routine_1";
        var routines = await RoutineProvider.GetAllRoutines();
        var containsTestRoutine = routines.Any(r => string.Equals(r.Name.LocalName, routineName, StringComparison.Ordinal));

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task Definition_ForFunction_ReturnsCorrectDefinition()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        var definition = routine.Definition;
        const string expectedDefinition = @"BEGIN
  RETURN 'test';
END";

        Assert.That(definition, Is.EqualTo(expectedDefinition));
    }

    [Test]
    public async Task Definition_ForStoredProcedure_ReturnsCorrectDefinition()
    {
        var routine = await GetRoutineAsync("db_test_routine_2");

        var definition = routine.Definition;
        const string expectedDefinition = @"BEGIN
   COMMIT;
END";

        Assert.That(definition, Is.EqualTo(expectedDefinition));
    }

    [TestCase("db_test_routine_1", RoutineType.Function)]
    [TestCase("db_test_routine_2", RoutineType.Procedure)]
    public async Task RoutineType_GivenRoutine_ReturnsCorrectType(string routineName, RoutineType expectedType)
    {
        var routine = await GetRoutineAsync(routineName);

        Assert.That(routine.RoutineType, Is.EqualTo(expectedType));
    }

    [Test]
    public async Task Language_GivenRoutine_ReturnsSql()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        Assert.That(routine.Language.UnwrapSome(), Is.EqualTo("SQL"));
    }

    [Test]
    public async Task ReturnType_ForFunction_ReturnsFunctionReturnType()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        Assert.That(routine.ReturnType.UnwrapSome().TypeName.LocalName, Is.EqualTo("text"));
    }

    [Test]
    public async Task ReturnType_ForStoredProcedure_ReturnsNone()
    {
        var routine = await GetRoutineAsync("db_test_routine_2");

        Assert.That(routine.ReturnType, OptionIs.None);
    }

    [Test]
    public async Task Parameters_GivenRoutineWithoutParameters_ReturnsEmpty()
    {
        var routine = await GetRoutineAsync("db_test_routine_2");

        Assert.That(routine.Parameters, Is.Empty);
    }

    [Test]
    public async Task Parameters_GivenRoutineWithParameters_ReturnsParametersInOrder()
    {
        var routine = await GetRoutineAsync("db_test_routine_3");
        var parameters = routine.Parameters;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameters, Has.Count.EqualTo(2));
            Assert.That(parameters[0].Name.UnwrapSome().LocalName, Is.EqualTo("first_arg"));
            Assert.That(parameters[0].Ordinal, Is.EqualTo(1));
            Assert.That(parameters[0].Direction, Is.EqualTo(RoutineParameterDirection.Input));
            Assert.That(parameters[1].Name.UnwrapSome().LocalName, Is.EqualTo("second_arg"));
            Assert.That(parameters[1].Ordinal, Is.EqualTo(2));
            Assert.That(parameters[1].Direction, Is.EqualTo(RoutineParameterDirection.Output));
        }
    }

    [Test]
    public async Task Overloads_GivenRoutine_ReturnsEmpty()
    {
        var routine = await GetRoutineAsync("db_test_routine_3");

        Assert.That(routine.Overloads, Is.Empty);
    }
}