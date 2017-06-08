using System;
using NUnit.Framework;
using System.Data.SqlClient;

namespace SJP.Schema.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTests
    {
        [Test]
        public void MissingDialectThrowsException()
        {
            using (var connection = new SqlConnection())
                Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(SqlServerDialect.Instance, null));
        }
    }
}
