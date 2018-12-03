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
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(null, connection, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();
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
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), connection, identifierDefaults, null));
        }
    }
}
