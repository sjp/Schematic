using System;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Data;

namespace SJP.Schema.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTests
    {
        [Test]
        public void MissingDialectThrowsException()
        {
            var connection = Moq.Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(SqlServerDialect.Instance, null));
        }
    }
}
