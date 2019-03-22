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
        public static async Task GetAllSequences_PropertyGet_ReturnsCountOfZero()
        {
            var sequences = await Database.GetAllSequences().ConfigureAwait(false);

            Assert.Zero(sequences.Count);
        }

        [Test]
        public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
        {
            var sequences = await Database.GetAllSequences().ConfigureAwait(false);
            var count = sequences.ToList().Count;

            Assert.Zero(count);
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
        public static async Task GetAllSynonyms_PropertyGet_ReturnsCountOfZero()
        {
            var synonyms = await Database.GetAllSynonyms().ConfigureAwait(false);

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
        {
            var synonyms = await Database.GetAllSynonyms().ConfigureAwait(false);
            var count = synonyms.ToList().Count;

            Assert.Zero(count);
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
        public static async Task GetAllRoutines_PropertyGet_ReturnsCountOfZero()
        {
            var routines = await Database.GetAllRoutines().ConfigureAwait(false);

            Assert.Zero(routines.Count);
        }

        [Test]
        public static async Task GetAllRoutines_WhenEnumerated_ContainsNoValues()
        {
            var routines = await Database.GetAllRoutines().ConfigureAwait(false);
            var count = routines.ToList().Count;

            Assert.Zero(count);
        }
    }
}
