using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.That(() => new OracleRelationalDatabase(null, connection, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.That(() => new OracleRelationalDatabase(dialect, null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.That(() => new OracleRelationalDatabase(dialect, connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new OracleRelationalDatabase(dialect, connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetTable(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetView(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetSequence(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetSynonym(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = new OracleDialect();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(dialect, connection, identifierDefaults, identifierResolver);

            Assert.That(() => database.GetRoutine(null), Throws.ArgumentNullException);
        }
    }
}
