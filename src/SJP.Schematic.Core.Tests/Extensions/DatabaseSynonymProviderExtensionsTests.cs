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
    internal static class DatabaseSynonymProviderExtensionsTests
    {
        private static FakeRelationalDatabase GetFakeDatabase()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }

        private static IDatabaseSynonym GetMockSynonym(Identifier synonymName)
        {
            var synonymMock = new Mock<IDatabaseSynonym>();
            synonymMock.SetupGet(s => s.Name).Returns(synonymName);

            return synonymMock.Object;
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
