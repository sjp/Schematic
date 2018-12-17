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
    // NOTE: these test the various object provider extension methods via a database instead of separate providers
    [TestFixture]
    internal static class RelationalDatabaseExtensionsTests
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

        private static IDatabaseView GetMockView(Identifier viewName)
        {
            var viewMock = new Mock<IDatabaseView>();
            viewMock.SetupGet(v => v.Name).Returns(viewName);

            return viewMock.Object;
        }

        private static IDatabaseSequence GetMockSequence(Identifier sequenceName)
        {
            var sequenceMock = new Mock<IDatabaseSequence>();
            sequenceMock.SetupGet(s => s.Name).Returns(sequenceName);

            return sequenceMock.Object;
        }

        private static IDatabaseSynonym GetMockSynonym(Identifier synonymName)
        {
            var synonymMock = new Mock<IDatabaseSynonym>();
            synonymMock.SetupGet(s => s.Name).Returns(synonymName);

            return synonymMock.Object;
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

        [Test]
        public static void TryGetView_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var viewName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetView(viewName, out var view));
        }

        [Test]
        public static void TryGetView_GivenNullViewName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier viewName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetView(viewName, out var view));
        }

        [Test]
        public static void TryGetViewAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var viewName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetViewAsync(viewName));
        }

        [Test]
        public static void TryGetViewAsync_GivenNullViewName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier viewName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetViewAsync(viewName));
        }

        [Test]
        public static void TryGetView_GivenPresentViewName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            Assert.IsTrue(fakeDb.TryGetView(viewName, out var _));
        }

        [Test]
        public static void TryGetView_GivenPresentViewName_ReturnsCorrectView()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            fakeDb.TryGetView(viewName, out var viewResult);

            Assert.AreEqual(view, viewResult);
        }

        [Test]
        public static void TryGetView_GivenMissingViewName_ReturnsFalse()
        {
            var database = GetFakeDatabase();
            var viewName = new Identifier("A");

            Assert.IsFalse(database.TryGetView(viewName, out var _));
        }

        [Test]
        public static void TryGetView_GivenMissingViewName_ReturnsNullView()
        {
            var viewName = new Identifier("A");
            var fakeDb = GetFakeDatabase();

            fakeDb.TryGetView(viewName, out var view);

            Assert.IsNull(view);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            var result = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsCorrectView()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");
            var view = GetMockView(viewName);
            fakeDb.Views = new[] { view };

            var result = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(view, result.view);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsFalse()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");

            var (exists, view) = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsNullView()
        {
            var fakeDb = GetFakeDatabase();
            var viewName = new Identifier("A");

            var (exists, view) = await fakeDb.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(view);
        }

        [Test]
        public static void TryGetSequence_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var sequenceName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetSequence(sequenceName, out var sequence));
        }

        [Test]
        public static void TryGetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier sequenceName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSequence(sequenceName, out var sequence));
        }

        [Test]
        public static void TryGetSequenceAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var sequenceName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetSequenceAsync(sequenceName));
        }

        [Test]
        public static void TryGetSequenceAsync_GivenNullSequenceName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier sequenceName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSequenceAsync(sequenceName));
        }

        [Test]
        public static void TryGetSequence_GivenPresentSequenceName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var sequenceName = new Identifier("A");
            var sequence = GetMockSequence(sequenceName);
            fakeDb.Sequences = new[] { sequence };

            Assert.IsTrue(fakeDb.TryGetSequence(sequenceName, out var _));
        }

        [Test]
        public static void TryGetSequence_GivenPresentSequenceName_ReturnsCorrectSequence()
        {
            var fakeDb = GetFakeDatabase();
            var sequenceName = new Identifier("A");
            var sequence = GetMockSequence(sequenceName);
            fakeDb.Sequences = new[] { sequence };

            fakeDb.TryGetSequence(sequenceName, out var sequenceResult);

            Assert.AreEqual(sequence, sequenceResult);
        }

        [Test]
        public static void TryGetSequence_GivenMissingSequenceName_ReturnsFalse()
        {
            var database = GetFakeDatabase();
            var sequenceName = new Identifier("A");

            Assert.IsFalse(database.TryGetSequence(sequenceName, out var _));
        }

        [Test]
        public static void TryGetSequence_GivenMissingSequenceName_ReturnsNullSequence()
        {
            var database = GetFakeDatabase();
            var sequenceName = new Identifier("A");

            database.TryGetSequence(sequenceName, out var sequence);

            Assert.IsNull(sequence);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenPresentSequenceName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var sequenceName = new Identifier("A");
            var sequence = GetMockSequence(sequenceName);
            fakeDb.Sequences = new[] { sequence };

            var result = await fakeDb.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenPresentSequenceName_ReturnsCorrectSequence()
        {
            var fakeDb = GetFakeDatabase();
            var sequenceName = new Identifier("A");
            var sequence = GetMockSequence(sequenceName);
            fakeDb.Sequences = new[] { sequence };

            var result = await fakeDb.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(sequence, result.sequence);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenMissingSequenceName_ReturnsFalse()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequence(sequenceName, CancellationToken.None)).Returns(OptionAsync<IDatabaseSequence>.None);
            var database = databaseMock.Object;

            var (exists, sequence) = await database.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenMissingSequenceName_ReturnsNullSequence()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequence(sequenceName, CancellationToken.None)).Returns(OptionAsync<IDatabaseSequence>.None);
            var database = databaseMock.Object;

            var (exists, sequence) = await database.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(sequence);
        }

        [Test]
        public static void TryGetSynonym_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var synonymName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetSynonym(synonymName, out var synonym));
        }

        [Test]
        public static void TryGetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier synonymName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSynonym(synonymName, out var synonym));
        }

        [Test]
        public static void TryGetSynonymAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            IRelationalDatabase database = null;
            var synonymName = new Identifier("A");

            Assert.Throws<ArgumentNullException>(() => database.TryGetSynonymAsync(synonymName));
        }

        [Test]
        public static void TryGetSynonymAsync_GivenNullSynonymName_ThrowsArgumentNullException()
        {
            var database = GetFakeDatabase();
            Identifier synonymName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSynonymAsync(synonymName));
        }

        [Test]
        public static void TryGetSynonym_GivenPresentSynonymName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var synonymName = new Identifier("A");
            var synonym = GetMockSynonym(synonymName);

            fakeDb.Synonyms = new[] { synonym };

            Assert.IsTrue(fakeDb.TryGetSynonym(synonymName, out var _));
        }

        [Test]
        public static void TryGetSynonym_GivenPresentSynonymName_ReturnsCorrectSynonym()
        {
            var fakeDb = GetFakeDatabase();
            var synonymName = new Identifier("A");
            var synonym = GetMockSynonym(synonymName);
            fakeDb.Synonyms = new[] { synonym };

            fakeDb.TryGetSynonym(synonymName, out var synonymResult);

            Assert.AreEqual(synonym, synonymResult);
        }

        [Test]
        public static void TryGetSynonym_GivenMissingSynonymName_ReturnsFalse()
        {
            var database = GetFakeDatabase();
            var synonymName = new Identifier("A");

            Assert.IsFalse(database.TryGetSynonym(synonymName, out var _));
        }

        [Test]
        public static void TryGetSynonym_GivenMissingSynonymName_ReturnsNullSynonym()
        {
            var database = GetFakeDatabase();
            var synonymName = new Identifier("A");

            database.TryGetSynonym(synonymName, out var synonym);

            Assert.IsNull(synonym);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenPresentSynonymName_ReturnsTrue()
        {
            var fakeDb = GetFakeDatabase();
            var synonymName = new Identifier("A");
            var synonym = GetMockSynonym(synonymName);
            fakeDb.Synonyms = new[] { synonym };

            var result = await fakeDb.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenPresentSynonymName_ReturnsCorrectSynonym()
        {
            var fakeDb = GetFakeDatabase();
            var synonymName = new Identifier("A");
            var synonym = GetMockSynonym(synonymName);
            fakeDb.Synonyms = new[] { synonym };

            var result = await fakeDb.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(synonym, result.synonym);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenMissingSynonymName_ReturnsFalse()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonym(synonymName, CancellationToken.None)).Returns(OptionAsync<IDatabaseSynonym>.None);
            var database = databaseMock.Object;

            var (exists, synonym) = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenMissingSynonymName_ReturnsNullSynonym()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonym(synonymName, CancellationToken.None)).Returns(OptionAsync<IDatabaseSynonym>.None);
            var database = databaseMock.Object;

            var (exists, synonym) = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(synonym);
        }
    }
}
