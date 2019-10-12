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
    internal static class DatabaseSequenceProviderExtensionsTests
    {
        private static FakeRelationalDatabase GetFakeDatabase()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }

        private static IDatabaseSequence GetMockSequence(Identifier sequenceName)
        {
            var sequenceMock = new Mock<IDatabaseSequence>();
            sequenceMock.SetupGet(s => s.Name).Returns(sequenceName);

            return sequenceMock.Object;
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
    }
}
