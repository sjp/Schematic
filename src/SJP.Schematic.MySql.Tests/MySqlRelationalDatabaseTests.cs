using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal class MySqlRelationalDatabaseTests
    {
        [Test]
        public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(null, connection));
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabase(new MySqlDialect(), null));
        }
    }
}
