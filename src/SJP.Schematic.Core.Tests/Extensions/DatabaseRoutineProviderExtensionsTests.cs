using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests.Fakes;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class DatabaseRoutineProviderExtensionsTests
    {
        private static FakeRelationalDatabase GetFakeDatabase()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }

        private static IDatabaseRoutine GetMockRoutine(Identifier routineName)
        {
            var routineMock = new Mock<IDatabaseRoutine>();
            routineMock.SetupGet(s => s.Name).Returns(routineName);

            return routineMock.Object;
        }

        [Test]
        public static void TryGetRoutine_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var routineName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetRoutine(routineName, out var routine));
        }

        [Test]
        public static void TryGetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier routineName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetRoutine(routineName, out var routine));
        }

        [Test]
        public static void TryGetRoutineAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var routineName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetRoutineAsync(routineName));
        }

        [Test]
        public static void TryGetRoutineAsync_GivenNullRoutineName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier routineName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetRoutineAsync(routineName));
        }

        [Test]
        public static void TryGetRoutine_GivenPresentRoutineName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var routineName = new Identifier("A");
            var routine = GetMockRoutine(routineName);

            fakeDb.Routines = new[] { routine };

            Assert.IsTrue(fakeDb.TryGetRoutine(routineName, out var _));
        }

        [Test]
        public static void TryGetRoutine_GivenPresentRoutineName_ReturnsCorrectRoutine()
        {
            var fakeDb = GetFakeDatabase();
            var routineName = new Identifier("A");
            var routine = GetMockRoutine(routineName);
            fakeDb.Routines = new[] { routine };

            fakeDb.TryGetRoutine(routineName, out var routineResult);

            Assert.AreEqual(routine, routineResult);
        }

        [Test]
        public static void TryGetRoutine_GivenMissingRoutineName_ReturnsFalse()
        {
            var database = GetFakeDatabase();
            var routineName = new Identifier("A");

            Assert.IsFalse(database.TryGetRoutine(routineName, out var _));
        }

        [Test]
        public static void TryGetRoutine_GivenMissingRoutineName_ReturnsNullRoutine()
        {
            var database = GetFakeDatabase();
            var routineName = new Identifier("A");

            database.TryGetRoutine(routineName, out var routine);

            Assert.IsNull(routine);
        }

        [Test]
        public static async Task TryGetRoutineAsync_GivenPresentRoutineName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var routineName = new Identifier("A");
            var routine = GetMockRoutine(routineName);
            fakeDb.Routines = new[] { routine };

            var result = await fakeDb.TryGetRoutineAsync(routineName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetRoutineAsync_GivenPresentRoutineName_ReturnsCorrectRoutine()
        {
            var fakeDb = GetFakeDatabase();
            var routineName = new Identifier("A");
            var routine = GetMockRoutine(routineName);
            fakeDb.Routines = new[] { routine };

            var result = await fakeDb.TryGetRoutineAsync(routineName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(routine, result.routine);
        }

        [Test]
        public static async Task TryGetRoutineAsync_GivenMissingRoutineName_ReturnsFalse()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var routineName = new Identifier("A");
            databaseMock.Setup(db => db.GetRoutine(routineName, CancellationToken.None)).Returns(OptionAsync<IDatabaseRoutine>.None);
            var database = databaseMock.Object;

            var (exists, routine) = await database.TryGetRoutineAsync(routineName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetRoutineAsync_GivenMissingRoutineName_ReturnsNullRoutine()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var routineName = new Identifier("A");
            databaseMock.Setup(db => db.GetRoutine(routineName, CancellationToken.None)).Returns(OptionAsync<IDatabaseRoutine>.None);
            var database = databaseMock.Object;

            var (exists, routine) = await database.TryGetRoutineAsync(routineName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(routine);
        }
    }
}
