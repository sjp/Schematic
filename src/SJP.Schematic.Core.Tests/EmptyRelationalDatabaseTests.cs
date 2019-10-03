using System;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Linq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseTests
    {
        private static IRelationalDatabase Database => new EmptyRelationalDatabase(Mock.Of<IDatabaseDialect>(), Mock.Of<IIdentifierDefaults>());

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new EmptyRelationalDatabase(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();

            Assert.Throws<ArgumentNullException>(() => new EmptyRelationalDatabase(dialect, null));
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GetTable(null));
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
        }

        [Test]
        public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
        }

        [Test]
        public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
        {
            var sequenceName = new Identifier("test");
            var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone.ConfigureAwait(false);

            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
        {
            var hasSequences = await Database.GetAllSequences()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasSequences);
        }

        [Test]
        public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
        }

        [Test]
        public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
        {
            var synonymName = new Identifier("test");
            var synonymIsNone = await Database.GetSynonym(synonymName).IsNone.ConfigureAwait(false);

            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
        {
            var synonyms = await Database.GetAllSynonyms().ToListAsync().ConfigureAwait(false);

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Database.GetRoutine(null));
        }

        [Test]
        public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
        {
            var routineName = new Identifier("test");
            var routineIsNone = await Database.GetRoutine(routineName).IsNone.ConfigureAwait(false);

            Assert.IsTrue(routineIsNone);
        }

        [Test]
        public static async Task GetAllRoutines_WhenEnumerated_ContainsNoValues()
        {
            var hasRoutines = await Database.GetAllRoutines()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasRoutines);
        }
    }
}
