using System;
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

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(null, connection, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), null, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), connection, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, null));
        }

        [Test]
        public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => database.GetTable(null));
        }

        [Test]
        public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => database.GetView(null));
        }

        [Test]
        public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => database.GetSequence(null));
        }

        [Test]
        public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var database = new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => database.GetSynonym(null));
        }
    }
}
