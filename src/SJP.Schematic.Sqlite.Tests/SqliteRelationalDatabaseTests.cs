using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseTests
    {
        private static ISqliteDatabase Database
        {
            get
            {
                var connection = Mock.Of<ISchematicConnection>();
                var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                return new SqliteRelationalDatabase(connection, identifierDefaults);
            }
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqliteRelationalDatabase(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();

            Assert.That(() => new SqliteRelationalDatabase(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.GetTable(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.GetView(null), Throws.ArgumentNullException);
        }

        // testing that the behaviour is equivalent to an empty sequence provider
        [TestFixture]
        internal static class SequenceTests
        {
            [Test]
            public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.That(() => Database.GetSequence(null), Throws.ArgumentNullException);
            }

            [Test]
            public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("test");
                var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone.ConfigureAwait(false);

                Assert.That(sequenceIsNone, Is.True);
            }

            [Test]
            public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
            {
                var hasSequences = await Database.GetAllSequences()
                    .AnyAsync()
                    .ConfigureAwait(false);

                Assert.That(hasSequences, Is.False);
            }
        }

        // testing that the behaviour is equivalent to an empty synonym provider
        [TestFixture]
        internal static class SynonymTests
        {
            [Test]
            public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.That(() => Database.GetSynonym(null), Throws.ArgumentNullException);
            }

            [Test]
            public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("test");
                var synonymIsNone = await Database.GetSynonym(synonymName).IsNone.ConfigureAwait(false);

                Assert.That(synonymIsNone, Is.True);
            }

            [Test]
            public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
            {
                var hasSynonyms = await Database.GetAllSynonyms()
                    .AnyAsync()
                    .ConfigureAwait(false);

                Assert.That(hasSynonyms, Is.False);
            }
        }

        // testing that the behaviour is equivalent to an empty routines provider
        [TestFixture]
        internal static class RoutineTests
        {
            [Test]
            public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
            {
                Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
            }

            [Test]
            public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
            {
                var routineName = new Identifier("test");
                var routineIsNone = await Database.GetRoutine(routineName).IsNone.ConfigureAwait(false);

                Assert.That(routineIsNone, Is.True);
            }

            [Test]
            public static async Task GetAllRoutines_WhenEnumerated_ContainsNoValues()
            {
                var hasRoutines = await Database.GetAllRoutines()
                    .AnyAsync()
                    .ConfigureAwait(false);

                Assert.That(hasRoutines, Is.False);
            }
        }

        [Test]
        public static void VacuumAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.VacuumAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void VacuumAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.VacuumAsync(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void VacuumAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.VacuumAsync("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync(null, ":memory:"), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync(string.Empty, ":memory:"), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync("   ", ":memory:"), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync("test", null), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync("test", string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.AttachDatabaseAsync("test", "   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenMainSchemaName_ThrowsArgumentException()
        {
            Assert.That(() => Database.AttachDatabaseAsync("main", ":memory:"), Throws.ArgumentException);
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.DetachDatabaseAsync(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.DetachDatabaseAsync(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.DetachDatabaseAsync("   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenMainSchemaName_ThrowsArgumentException()
        {
            Assert.That(() => Database.DetachDatabaseAsync("main"), Throws.ArgumentException);
        }
    }
}
