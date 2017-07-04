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
        public void MissingDialectThrowsException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(new SqlServerDialect(), null));
        }
    }
}
