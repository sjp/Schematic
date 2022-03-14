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

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseSimpleRoutineProviderTests : OracleTest
    {
        private IDatabaseRoutineProvider RoutineProvider => new OracleDatabaseSimpleRoutineProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync(@"
create or replace FUNCTION db_test_routine_1()
   RETURN NUMBER(1)
   IS test_col NUMBER(1);
   BEGIN
      SELECT 1 as dummy
      INTO test_col
      FROM dual;
      RETURN(test_col);
END db_test_routine_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"CREATE PROCEDURE db_test_routine_2
IS
BEGIN
    DBMS_OUTPUT.PUT_LINE('test');
END;", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop function db_test_routine_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure db_test_routine_2", CancellationToken.None).ConfigureAwait(false);
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

                return await lazyRoutine.ConfigureAwait(false);
            }
        }

        private readonly AsyncLock _lock = new();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>> _routinesCache = new();

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutine()
        {
            var routineIsSome = await RoutineProvider.GetRoutine("db_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.That(routineIsSome, Is.True);
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "DB_TEST_ROUTINE_1";
            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name.LocalName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_ROUTINE_1");

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
        public async Task GetRoutine_WhenRoutinePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_ROUTINE_1");
            var routine = await RoutineProvider.GetRoutine(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, routine.Name.LocalName);
            Assert.That(equalNames, Is.True);
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            // lower-case the first and last letters only
            var schemaName = IdentifierDefaults.Schema[0].ToLowerInvariant()
                + IdentifierDefaults.Schema[1..^1].ToUpperInvariant()
                + IdentifierDefaults.Schema[^1].ToLowerInvariant();

            var inputName = new Identifier(schemaName, "db_test_ROUTINE_1");
            var routine = await RoutineProvider.GetRoutine(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, routine.Name.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, routine.Name.LocalName);
            Assert.That(equalNames, Is.True);
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
                .AnyAsync(r => string.Equals(r.Name.LocalName, "DB_TEST_ROUTINE_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestRoutine, Is.True);
        }

        [Test]
        public async Task Definition_GivenFunction_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("DB_TEST_ROUTINE_1").ConfigureAwait(false);
            const string expectedDefinition = @"FUNCTION db_test_routine_1()
   RETURN NUMBER(1)
   IS test_col NUMBER(1);
   BEGIN
      SELECT 1 as dummy
      INTO test_col
      FROM dual;
      RETURN(test_col);
END db_test_routine_1";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Definition_GivenStoredProcedure_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("DB_TEST_ROUTINE_2").ConfigureAwait(false);
            const string expectedDefinition = @"PROCEDURE db_test_routine_2
IS
BEGIN
    DBMS_OUTPUT.PUT_LINE('test');
END;";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }
    }
}
