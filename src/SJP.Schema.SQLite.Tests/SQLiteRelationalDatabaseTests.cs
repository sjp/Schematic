using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schema.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteRelationalDatabaseTests
    {
        [Test]
        public void MissingDialectThrowsException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabase(SqliteDialect.Instance, null));
        }
    }
}
