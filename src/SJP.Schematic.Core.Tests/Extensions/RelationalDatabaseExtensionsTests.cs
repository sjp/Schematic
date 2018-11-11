using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class RelationalDatabaseExtensionsTests
    {
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
            var database = Mock.Of<IRelationalDatabase>();
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
            var database = Mock.Of<IRelationalDatabase>();
            Identifier tableName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetTableAsync(tableName));
        }

        [Test]
        public static void TryGetTable_GivenPresentTableName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var table = Mock.Of<IRelationalDatabaseTable>();
            var tableName = new Identifier("A");
            databaseMock.Setup(db => db.GetTable(tableName)).Returns(Option<IRelationalDatabaseTable>.Some(table));
            var database = databaseMock.Object;

            Assert.IsTrue(database.TryGetTable(tableName, out var _));
        }

        [Test]
        public static void TryGetTable_GivenPresentTableName_ReturnsCorrectTable()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var table = Mock.Of<IRelationalDatabaseTable>();
            var tableName = new Identifier("A");
            databaseMock.Setup(db => db.GetTable(tableName)).Returns(Option<IRelationalDatabaseTable>.Some(table));
            var database = databaseMock.Object;

            database.TryGetTable(tableName, out var tableResult);

            Assert.AreEqual(table, tableResult);
        }

        [Test]
        public static void TryGetTable_GivenMissingTableName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("A");

            Assert.IsFalse(database.TryGetTable(tableName, out var _));
        }

        [Test]
        public static void TryGetTable_GivenMissingTableName_ReturnsNullTable()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("A");

            database.TryGetTable(tableName, out var table);

            Assert.IsNull(table);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenPresentTableName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var table = Mock.Of<IRelationalDatabaseTable>();
            var tableName = new Identifier("A");
            databaseMock.Setup(db => db.GetTableAsync(tableName, CancellationToken.None)).Returns(Task.FromResult(Option<IRelationalDatabaseTable>.Some(table)));
            var database = databaseMock.Object;

            var result = await database.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenPresentTableName_ReturnsCorrectTable()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var table = Mock.Of<IRelationalDatabaseTable>();
            var tableName = new Identifier("A");
            databaseMock.Setup(db => db.GetTableAsync(tableName, CancellationToken.None)).Returns(Task.FromResult(Option<IRelationalDatabaseTable>.Some(table)));
            var database = databaseMock.Object;

            var result = await database.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(table, result.table);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenMissingTableName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("A");

            var (exists, table) = await database.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetTableAsync_GivenMissingTableName_ReturnsNullTable()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("A");

            var (exists, table) = await database.TryGetTableAsync(tableName, CancellationToken.None).ConfigureAwait(false);
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
            var database = Mock.Of<IRelationalDatabase>();
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
            var database = Mock.Of<IRelationalDatabase>();
            Identifier viewName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetViewAsync(viewName));
        }

        [Test]
        public static void TryGetView_GivenPresentViewName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var view = Mock.Of<IRelationalDatabaseView>();
            var viewName = new Identifier("A");
            databaseMock.Setup(db => db.GetView(viewName)).Returns(Option<IRelationalDatabaseView>.Some(view));
            var database = databaseMock.Object;

            Assert.IsTrue(database.TryGetView(viewName, out var _));
        }

        [Test]
        public static void TryGetView_GivenPresentViewName_ReturnsCorrectView()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var view = Mock.Of<IRelationalDatabaseView>();
            var viewName = new Identifier("A");
            databaseMock.Setup(db => db.GetView(viewName)).Returns(Option<IRelationalDatabaseView>.Some(view));
            var database = databaseMock.Object;

            database.TryGetView(viewName, out var viewResult);

            Assert.AreEqual(view, viewResult);
        }

        [Test]
        public static void TryGetView_GivenMissingViewName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var viewName = new Identifier("A");

            Assert.IsFalse(database.TryGetView(viewName, out var _));
        }

        [Test]
        public static void TryGetView_GivenMissingViewName_ReturnsNullView()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var viewName = new Identifier("A");

            database.TryGetView(viewName, out var view);

            Assert.IsNull(view);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var view = Mock.Of<IRelationalDatabaseView>();
            var viewName = new Identifier("A");
            databaseMock.Setup(db => db.GetViewAsync(viewName, CancellationToken.None)).Returns(Task.FromResult(Option<IRelationalDatabaseView>.Some(view)));
            var database = databaseMock.Object;

            var result = await database.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenPresentViewName_ReturnsCorrectView()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var view = Mock.Of<IRelationalDatabaseView>();
            var viewName = new Identifier("A");
            databaseMock.Setup(db => db.GetViewAsync(viewName, CancellationToken.None)).Returns(Task.FromResult(Option<IRelationalDatabaseView>.Some(view)));
            var database = databaseMock.Object;

            var result = await database.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(view, result.view);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var viewName = new Identifier("A");

            var (exists, view) = await database.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetViewAsync_GivenMissingViewName_ReturnsNullView()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var viewName = new Identifier("A");

            var (exists, view) = await database.TryGetViewAsync(viewName, CancellationToken.None).ConfigureAwait(false);
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
            var database = Mock.Of<IRelationalDatabase>();
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
            var database = Mock.Of<IRelationalDatabase>();
            Identifier sequenceName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSequenceAsync(sequenceName));
        }

        [Test]
        public static void TryGetSequence_GivenPresentSequenceName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequence = Mock.Of<IDatabaseSequence>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequence(sequenceName)).Returns(Option<IDatabaseSequence>.Some(sequence));
            var database = databaseMock.Object;

            Assert.IsTrue(database.TryGetSequence(sequenceName, out var _));
        }

        [Test]
        public static void TryGetSequence_GivenPresentSequenceName_ReturnsCorrectSequence()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequence = Mock.Of<IDatabaseSequence>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequence(sequenceName)).Returns(Option<IDatabaseSequence>.Some(sequence));
            var database = databaseMock.Object;

            database.TryGetSequence(sequenceName, out var sequenceResult);

            Assert.AreEqual(sequence, sequenceResult);
        }

        [Test]
        public static void TryGetSequence_GivenMissingSequenceName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequenceName = new Identifier("A");

            Assert.IsFalse(database.TryGetSequence(sequenceName, out var _));
        }

        [Test]
        public static void TryGetSequence_GivenMissingSequenceName_ReturnsNullSequence()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequenceName = new Identifier("A");

            database.TryGetSequence(sequenceName, out var sequence);

            Assert.IsNull(sequence);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenPresentSequenceName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequence = Mock.Of<IDatabaseSequence>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequenceAsync(sequenceName, CancellationToken.None)).Returns(Task.FromResult(Option<IDatabaseSequence>.Some(sequence)));
            var database = databaseMock.Object;

            var result = await database.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenPresentSequenceName_ReturnsCorrectSequence()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var sequence = Mock.Of<IDatabaseSequence>();
            var sequenceName = new Identifier("A");
            databaseMock.Setup(db => db.GetSequenceAsync(sequenceName, CancellationToken.None)).Returns(Task.FromResult(Option<IDatabaseSequence>.Some(sequence)));
            var database = databaseMock.Object;

            var result = await database.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(sequence, result.sequence);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenMissingSequenceName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequenceName = new Identifier("A");

            var (exists, sequence) = await database.TryGetSequenceAsync(sequenceName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetSequenceAsync_GivenMissingSequenceName_ReturnsNullSequence()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequenceName = new Identifier("A");

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
            var database = Mock.Of<IRelationalDatabase>();
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
            var database = Mock.Of<IRelationalDatabase>();
            Identifier synonymName = null;

            Assert.Throws<ArgumentNullException>(() => database.TryGetSynonymAsync(synonymName));
        }

        [Test]
        public static void TryGetSynonym_GivenPresentSynonymName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonym = Mock.Of<IDatabaseSynonym>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonym(synonymName)).Returns(Option<IDatabaseSynonym>.Some(synonym));
            var database = databaseMock.Object;

            Assert.IsTrue(database.TryGetSynonym(synonymName, out var _));
        }

        [Test]
        public static void TryGetSynonym_GivenPresentSynonymName_ReturnsCorrectSynonym()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonym = Mock.Of<IDatabaseSynonym>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonym(synonymName)).Returns(Option<IDatabaseSynonym>.Some(synonym));
            var database = databaseMock.Object;

            database.TryGetSynonym(synonymName, out var synonymResult);

            Assert.AreEqual(synonym, synonymResult);
        }

        [Test]
        public static void TryGetSynonym_GivenMissingSynonymName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonymName = new Identifier("A");

            Assert.IsFalse(database.TryGetSynonym(synonymName, out var _));
        }

        [Test]
        public static void TryGetSynonym_GivenMissingSynonymName_ReturnsNullSynonym()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonymName = new Identifier("A");

            database.TryGetSynonym(synonymName, out var synonym);

            Assert.IsNull(synonym);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenPresentSynonymName_ReturnsTrue()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonym = Mock.Of<IDatabaseSynonym>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonymAsync(synonymName, CancellationToken.None)).Returns(Task.FromResult(Option<IDatabaseSynonym>.Some(synonym)));
            var database = databaseMock.Object;

            var result = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(result.exists);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenPresentSynonymName_ReturnsCorrectSynonym()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            var synonym = Mock.Of<IDatabaseSynonym>();
            var synonymName = new Identifier("A");
            databaseMock.Setup(db => db.GetSynonymAsync(synonymName, CancellationToken.None)).Returns(Task.FromResult(Option<IDatabaseSynonym>.Some(synonym)));
            var database = databaseMock.Object;

            var result = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(synonym, result.synonym);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenMissingSynonymName_ReturnsFalse()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonymName = new Identifier("A");

            var (exists, synonym) = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsFalse(exists);
        }

        [Test]
        public static async Task TryGetSynonymAsync_GivenMissingSynonymName_ReturnsNullSynonym()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonymName = new Identifier("A");

            var (exists, synonym) = await database.TryGetSynonymAsync(synonymName, CancellationToken.None).ConfigureAwait(false);
            Assert.IsNull(synonym);
        }
    }
}
