using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;
using System.Threading.Tasks;
using System.Linq;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(null, connection, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(dialect, null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new MySqlDialect(connection);

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(dialect, connection, null));
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new MySqlDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new MySqlRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetTable(null));
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new MySqlDialect(connection);
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var database = new MySqlRelationalDatabase(dialect, connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => database.GetView(null));
        }

        // testing that the behaviour is equivalent to an empty sequence provider
        [TestFixture]
        internal static class SequenceTests
        {
            private static IRelationalDatabase Database
            {
                get
                {
                    var connection = Mock.Of<IDbConnection>();
                    var dialect = new MySqlDialect(connection);
                    var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                    return new MySqlRelationalDatabase(dialect, connection, identifierDefaults);
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
        [TestFixture]
        internal static class SynonymTests
        {
            private static IRelationalDatabase Database
            {
                get
                {
                    var connection = Mock.Of<IDbConnection>();
                    var dialect = new MySqlDialect(connection);
                    var identifierDefaults = Mock.Of<IIdentifierDefaults>();

                    return new MySqlRelationalDatabase(dialect, connection, identifierDefaults);
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
    }
}
