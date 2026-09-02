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
        await DbConnection.ExecuteAsync(@"CREATE PROCEDURE db_test_routine_2(IN val integer, OUT doubled integer)
AS $$
BEGIN
    doubled := val * 2;
END; $$
LANGUAGE PLPGSQL", CancellationToken.None);
        // two signatures under one name, which only PostgreSQL permits
        await DbConnection.ExecuteAsync(@"CREATE FUNCTION db_test_routine_3(val integer)
RETURNS integer AS $$ SELECT val $$
LANGUAGE SQL", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"CREATE FUNCTION db_test_routine_3(val text)
RETURNS text AS $$ SELECT val $$
LANGUAGE SQL", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"CREATE FUNCTION db_test_routine_4(val integer DEFAULT 42)
RETURNS integer AS $$ SELECT val $$
LANGUAGE SQL", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop function db_test_routine_1(integer)", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop procedure db_test_routine_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop function db_test_routine_3(integer)", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop function db_test_routine_3(text)", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop function db_test_routine_4", CancellationToken.None);
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

        Assert.Multiple(() =>
        {
            Assert.That(routine.Definition, Does.StartWith("CREATE OR REPLACE FUNCTION public.db_test_routine_1(val integer)"));
            Assert.That(routine.Definition, Does.Contain("RETURN val + 1;"));
        });
    }

    [TestCase("db_test_routine_1", RoutineType.Function)]
    [TestCase("db_test_routine_2", RoutineType.Procedure)]
    public async Task RoutineType_GivenRoutine_ReturnsCorrectType(string routineName, RoutineType expectedType)
    {
        var routine = await GetRoutineAsync(routineName);

        Assert.That(routine.RoutineType, Is.EqualTo(expectedType));
    }

    [Test]
    public async Task Language_GivenRoutine_ReturnsDeclaredLanguage()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        Assert.That(routine.Language.UnwrapSome(), Is.EqualTo("plpgsql"));
    }

    [Test]
    public async Task ReturnType_ForFunction_ReturnsFunctionReturnType()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        Assert.That(routine.ReturnType.UnwrapSome().TypeName.LocalName, Is.EqualTo("int4"));
    }

    [Test]
    public async Task ReturnType_ForStoredProcedure_ReturnsNone()
    {
        var routine = await GetRoutineAsync("db_test_routine_2");

        Assert.That(routine.ReturnType, OptionIs.None);
    }

    [Test]
    public async Task Parameters_GivenFunctionWithParameter_ReturnsParameter()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");
        var parameters = routine.Parameters;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameters, Has.Count.EqualTo(1));
            Assert.That(parameters[0].Name.UnwrapSome().LocalName, Is.EqualTo("val"));
            Assert.That(parameters[0].Ordinal, Is.EqualTo(1));
            Assert.That(parameters[0].Type.TypeName.LocalName, Is.EqualTo("int4"));
            Assert.That(parameters[0].Direction, Is.EqualTo(RoutineParameterDirection.Input));
            Assert.That(parameters[0].DefaultValue, OptionIs.None);
        }
    }

    [Test]
    public async Task Parameters_GivenProcedureWithOutParameter_ReturnsDirections()
    {
        var routine = await GetRoutineAsync("db_test_routine_2");
        var parameters = routine.Parameters;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameters, Has.Count.EqualTo(2));
            Assert.That(parameters[0].Direction, Is.EqualTo(RoutineParameterDirection.Input));
            Assert.That(parameters[1].Name.UnwrapSome().LocalName, Is.EqualTo("doubled"));
            Assert.That(parameters[1].Direction, Is.EqualTo(RoutineParameterDirection.Output));
        }
    }

    [Test]
    public async Task Parameters_GivenFunctionWithDefaultedParameter_ReturnsDefaultValue()
    {
        var routine = await GetRoutineAsync("db_test_routine_4");

        Assert.That(routine.Parameters.Single().DefaultValue.UnwrapSome(), Is.EqualTo("42"));
    }

    [Test]
    public async Task Overloads_GivenOverloadedFunction_ReturnsEverySignature()
    {
        var routine = await GetRoutineAsync("db_test_routine_3");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.Overloads, Has.Count.EqualTo(2));
            Assert.That(routine.Overloads.Select(static o => o.Parameters.Single().Type.TypeName.LocalName), Is.EquivalentTo(new[] { "int4", "text" }));
            // the routine's own signature is the first overload's
            Assert.That(routine.Parameters, Is.EqualTo(routine.Overloads[0].Parameters));
            Assert.That(routine.Definition, Does.Contain(routine.Overloads[0].Definition));
            Assert.That(routine.Definition, Does.Contain(routine.Overloads[1].Definition));
        }
    }

    [Test]
    public async Task Overloads_GivenRoutineWithOneSignature_ReturnsEmpty()
    {
        var routine = await GetRoutineAsync("db_test_routine_1");

        Assert.That(routine.Overloads, Is.Empty);
    }
}