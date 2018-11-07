using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabase(null, connection, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabase(new PostgreSqlDialect(), null, identifierResolver));
        }
    }
}
