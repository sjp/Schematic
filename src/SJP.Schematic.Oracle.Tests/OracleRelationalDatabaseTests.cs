using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(null, connection, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabase(new OracleDialect(), connection, null));
        }
    }
}
