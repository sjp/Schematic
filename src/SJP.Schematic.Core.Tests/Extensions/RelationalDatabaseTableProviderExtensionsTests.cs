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
    internal static class RelationalDatabaseTableProviderExtensionsTests
    {
        private static FakeRelationalDatabase GetFakeDatabase()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }

        private static IRelationalDatabaseTable GetMockTable(Identifier tableName)
        {
            var tableMock = new Mock<IRelationalDatabaseTable>();
            tableMock.SetupGet(t => t.Name).Returns(tableName);

            return tableMock.Object;
        }

        [Test]
        public static void TryGetTable_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var tableName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetTable(tableName, out var table));
        }

        [Test]
        public static void TryGetTable_GivenNullTableName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier tableName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetTable(tableName, out var table));
        }

        [Test]
        public static void TryGetTableAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var tableName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetTableAsync(tableName));
        }

        [Test]
        public static void TryGetTableAsync_GivenNullTableName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier tableName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetTableAsync(tableName));
        }

        [Test]
        public static void TryGetTable_GivenPresentTableName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var tableName = new Identifier("A");
            var table = GetMockTable(tableName);
            fakeDb.Tables = new[] { table };

            Assert.IsTrue(fakeDb.TryGetTable(tableName, out var _));
        }

        [Test]
        public static void TryGetTable_GivenPresentTableName_ReturnsCorrectTable()
        {
            var fakeDb = GetFakeDatabase();
            var tableName = new Identifier("A");
            var table = GetMockTable(tableName);
            fakeDb.Tables = new[] { table };

            fakeDb.TryGetTable(tableName, out var tableResult);

            Assert.AreEqual(table, tableResult);
        }

        [Test]
        public static void TryGetTable_GivenMissingTableName_ReturnsFalse()
        {
            var database = GetFakeDatabase();
            var tableName = new Identifier("A");

            Assert.IsFalse(database.TryGetTable(tableName, out var _));
        }

        [Test]
        public static void TryGetTable_GivenMissingTableName_ReturnsNullTable()
        {
            var database = GetFakeDatabase();
            var tableName = new Identifier("A");

            database.TryGetTable(tableName, out var table);

            Assert.IsNull(table);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenPresentTableName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var tableName = new Identifier("A");
            var table = GetMockTable(tableName);
            fakeDb.Tables = new[] { table };

            var result = await fakeDb.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenPresentTableName_ReturnsCorrectTable()
        {
            var fakeDb = GetFakeDatabase();
            var tableName = new Identifier("A");
            var table = GetMockTable(tableName);
            fakeDb.Tables = new[] { table };

            var result = await fakeDb.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(table, result.table);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenMissingTableName_ReturnsFalse()
        {
            var tableName = new Identifier("A");
            var fakeDb = GetFakeDatabase();

            var (exists, table) = await fakeDb.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenMissingTableName_ReturnsNullTable()
        {
            var tableName = new Identifier("A");
            var fakeDb = GetFakeDatabase();

            var (exists, table) = await fakeDb.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(table);
        }
    }
}
