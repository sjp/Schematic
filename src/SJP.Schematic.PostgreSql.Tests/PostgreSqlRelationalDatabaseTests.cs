using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlRelationalDatabase(null, connection, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlRelationalDatabase(dialect, null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlRelationalDatabase(dialect, connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var database = new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetTable(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var database = new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetView(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var database = new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetSequence(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var database = new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetSynonym(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new PostgreSqlDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var database = new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetRoutine(null), Throws.ArgumentNullException);
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
                    var dialect = new PostgreSqlDialect();
                    var identifierDefaults = Mock.Of<IIdentifierDefaults>();
                    var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

                    return new PostgreSqlRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);
                }
            }

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
    }
}
