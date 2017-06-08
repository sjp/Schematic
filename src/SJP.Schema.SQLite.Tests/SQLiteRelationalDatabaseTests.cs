using System;
using NUnit.Framework;
using Moq;
using System.Data;

namespace SJP.Schema.SQLite.Tests
{
    [TestFixture]
    internal class SQLiteRelationalDatabaseTests
    {
        [Test]
        public void MissingDialectThrowsException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new SQLiteRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SQLiteRelationalDatabase(SQLiteDialect.Instance, null));
        }
    }
}
