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

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal sealed class SqlServerDatabaseRoutineProviderTests : SqlServerTest
    {
        private IDatabaseRoutineProvider RoutineProvider => new SqlServerDatabaseRoutineProvider(DbConnection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            // UDF
            await DbConnection.ExecuteAsync(@"
CREATE FUNCTION dbo.db_test_routine_1()
RETURNS int
AS
BEGIN
     DECLARE @tmp int;
     RETURN(@tmp);
END", CancellationToken.None).ConfigureAwait(false);
            // IF
            await DbConnection.ExecuteAsync(@"
CREATE FUNCTION dbo.db_test_routine_2()
RETURNS TABLE
AS
RETURN
(
    SELECT TOP 10 1 AS test_col
)", CancellationToken.None).ConfigureAwait(false);
            // TF
            await DbConnection.ExecuteAsync(@"
CREATE FUNCTION dbo.db_test_routine_3()
RETURNS @ret TABLE
(
    test_col int NOT NULL
)
AS
BEGIN
   INSERT INTO @ret (test_col) VALUES (1);
   RETURN
END", CancellationToken.None).ConfigureAwait(false);
            // P
            await DbConnection.ExecuteAsync(@"CREATE PROCEDURE db_test_routine_4
AS
SELECT DB_NAME() AS ThisDB", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop function db_test_routine_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop function db_test_routine_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop function db_test_routine_3", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure db_test_routine_4", CancellationToken.None).ConfigureAwait(false);
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

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>> _routinesCache = new Dictionary<Identifier, AsyncLazy<IDatabaseRoutine>>();

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutine()
        {
            var routineIsSome = await RoutineProvider.GetRoutine("db_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.That(routineIsSome, Is.True);
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "db_test_routine_1";
            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name.LocalName, Is.EqualTo(routineName));
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
        public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
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

            var routine = await RoutineProvider.GetRoutine(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routine.Name, Is.EqualTo(routineName));
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
        public async Task GetRoutine_WhenRoutinePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("db_test_routine_1");
            var routine = await RoutineProvider.GetRoutine(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, routine.Name.LocalName);
            Assert.That(equalNames, Is.True);
        }

        [Test]
        public async Task GetRoutine_WhenRoutinePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "db_test_routine_1");
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
                .AnyAsync(r => r.Name.LocalName == "db_test_routine_1")
                .ConfigureAwait(false);

            Assert.That(containsTestRoutine, Is.True);
        }

        [Test]
        public async Task Definition_GivenScalarFunction_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("db_test_routine_1").ConfigureAwait(false);
            const string expectedDefinition = @"
CREATE FUNCTION dbo.db_test_routine_1()
RETURNS int
AS
BEGIN
     DECLARE @tmp int;
     RETURN(@tmp);
END";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Definition_GivenInlineTableFunction_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("db_test_routine_2").ConfigureAwait(false);
            const string expectedDefinition = @"
CREATE FUNCTION dbo.db_test_routine_2()
RETURNS TABLE
AS
RETURN
(
    SELECT TOP 10 1 AS test_col
)";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Definition_GivenTableValuedFunction_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("db_test_routine_3").ConfigureAwait(false);
            const string expectedDefinition = @"
CREATE FUNCTION dbo.db_test_routine_3()
RETURNS @ret TABLE
(
    test_col int NOT NULL
)
AS
BEGIN
   INSERT INTO @ret (test_col) VALUES (1);
   RETURN
END";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }

        [Test]
        public async Task Definition_GivenStoredProcedure_ReturnsCorrectDefinition()
        {
            var routine = await GetRoutineAsync("db_test_routine_4").ConfigureAwait(false);
            const string expectedDefinition = @"CREATE PROCEDURE db_test_routine_4
AS
SELECT DB_NAME() AS ThisDB";

            Assert.That(routine.Definition, Is.EqualTo(expectedDefinition));
        }
    }
}
