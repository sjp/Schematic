using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(null, connection));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(new MySqlDialect(), null));
        }
    }
}
