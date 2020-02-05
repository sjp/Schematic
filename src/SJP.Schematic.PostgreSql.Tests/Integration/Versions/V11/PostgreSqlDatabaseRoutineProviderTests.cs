using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal sealed class PostgreSqlDatabaseRoutineProviderTests : PostgreSql11Test
    {
        private IDatabaseRoutineProvider RoutineProvider => new PostgreSqlDatabaseRoutineProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            // func
            await Connection.ExecuteAsync(@"CREATE FUNCTION v11_db_test_routine_1(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL").ConfigureAwait(false);
            // stored proc
            await Connection.ExecuteAsync(@"CREATE PROCEDURE v11_db_test_routine_2()
LANGUAGE PLPGSQL
AS $$
BEGIN
    COMMIT;
END $$").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop function v11_db_test_routine_1(integer)").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop procedure v11_db_test_routine_2()").ConfigureAwait(false);
        }

        private Task<IDatabaseRoutine> GetRoutineAsync(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

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

                return await lazyRoutine;
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>> _routinesCache = new Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>>();

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutine()
        {
            var routineIsSome = await RoutineProvider.GetRoutine("v11_db_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.That(routineIsSome, Is.True);
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "v11_db_test_routine_1";
            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name.LocalName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("v11_db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "v11_db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "v11_db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_db_test_routine_1");

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
        public async Task GetAllRoutines_WhenEnumerated_ContainsRoutines()
        {
            var hasRoutines = await RoutineProvider.GetAllRoutines()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasRoutines, Is.True);
        }

        [Test]
        public async Task GetAllRoutines_WhenEnumerated_ContainsTestRoutine()
        {
            var containsTestRoutine = await RoutineProvider.GetAllRoutines()
                .AnyAsync(r => r.Name.LocalName == "v11_db_test_routine_1")
                .ConfigureAwait(false);

            Assert.That(containsTestRoutine, Is.True);
        }

        [Test]
        public async Task Definition_ForFunction_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("v11_db_test_routine_1").ConfigureAwait(false);

            const string expectedDefinition = @"
BEGIN
    RETURN val + 1;
END; ";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Definition_ForStoredProcedure_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("v11_db_test_routine_2").ConfigureAwait(false);

            const string expectedDefinition = @"
BEGIN
    COMMIT;
END ";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }
    }
}
