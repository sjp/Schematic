using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schema.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTests
    {
        [Test]
        public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection));
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), null));
        }
    }
}
