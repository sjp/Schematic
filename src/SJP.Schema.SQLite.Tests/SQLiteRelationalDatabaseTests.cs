using System;
using NUnit.Framework;
using System.Data.SqlClient;

namespace SJP.Schema.SQLite.Tests
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTests
    {
        [Test]
        public void MissingDialectThrowsException()
        {
            using (var connection = new SqlConnection())
                Assert.Throws<ArgumentNullException>(() => new SQLiteRelationalDatabase(null, connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SQLiteRelationalDatabase(SQLiteDialect.Instance, null));
        }
    }
}
