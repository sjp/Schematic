using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;
using System.Threading.Tasks;
using System.Linq;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(new SqliteDialect(), connection, null));
        }

        // testing that the behaviour is equivalent to an empty sequence provider
        internal static class SequenceTests
        {
            private static IRelationalDatabase Database
            {
                get
                {
                    var dialect = new SqliteDialect();
                    var connection = Mock.Of<IDbConnection>();
                    var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                    return new SqliteRelationalDatabase(dialect, connection, identifierDefaults);
                }
            }

            [Test]
            public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("asd");
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
        }

        // testing that the behaviour is equivalent to an empty synonym provider
        internal static class SynonymTests
        {
            private static IRelationalDatabase Database
            {
                get
                {
                    var dialect = new SqliteDialect();
                    var connection = Mock.Of<IDbConnection>();
                    var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                    return new SqliteRelationalDatabase(dialect, connection, identifierDefaults);
                }
            }

            [Test]
            public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("asd");
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
        }

        private static ISqliteDatabase SqliteDatabase
        {
            get
            {
                var dialect = new SqliteDialect();
                var connection = Mock.Of<IDbConnection>();
                var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                return new SqliteRelationalDatabase(dialect, connection, identifierDefaults);
            }
        }

        [Test]
        public static void Vacuum_GivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.Vacuum(null));
        }

        [Test]
        public static void Vacuum_GivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.Vacuum(string.Empty));
        }

        [Test]
        public static void Vacuum_GivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.Vacuum("   "));
        }

        [Test]
        public static void VacuumAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.VacuumAsync(null));
        }

        [Test]
        public static void VacuumAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.VacuumAsync(string.Empty));
        }

        [Test]
        public static void VacuumAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.VacuumAsync("   "));
        }

        [Test]
        public static void AttachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase(null, ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase(string.Empty, ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase("   ", ":memory:"));
        }

        [Test]
        public static void AttachDatabase_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase("test", null));
        }

        [Test]
        public static void AttachDatabase_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase("test", string.Empty));
        }

        [Test]
        public static void AttachDatabase_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabase("test", "   "));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync(null, ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync(string.Empty, ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync("   ", ":memory:"));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenNullFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync("test", null));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenEmptyFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync("test", string.Empty));
        }

        [Test]
        public static void AttachDatabaseAsync_WhenGivenWhiteSpaceFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.AttachDatabaseAsync("test", "   "));
        }

        [Test]
        public static void DetachDatabase_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabase(null));
        }

        [Test]
        public static void DetachDatabase_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabase(string.Empty));
        }

        [Test]
        public static void DetachDatabase_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabase("   "));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenNullSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabaseAsync(null));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenEmptySchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabaseAsync(string.Empty));
        }

        [Test]
        public static void DetachDatabaseAsync_WhenGivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SqliteDatabase.DetachDatabaseAsync("   "));
        }
    }
}
