using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Dapper;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer.Tests.Integration
{
    [TestFixture, DatabaseDependent]
    internal class SqlServerRelationalDatabaseTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(SqlServerDialect.Instance, Connection);

        [Test]
        public void MissingDialectThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(null, Connection));
        }

        [Test]
        public void MissingConnectionThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabase(SqlServerDialect.Instance, null));
        }

        [Test]
        public void DatabaseNameMatches()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void CorrectSchemaName()
        {
            Assert.AreEqual(Database.DefaultSchema, "dbo");
        }

        [TestFixture, DatabaseDependent]
        internal class ViewTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy");
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop view db_test_view_1");
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(SqlServerDialect.Instance, Connection);

            [Test]
            public void TestPresenceOfPresentViewSync()
            {
                var hasView = Database.ViewExists("db_test_view_1");
                Assert.IsTrue(hasView);
            }

            [Test]
            public async Task TestPresenceOfPresentViewAsync()
            {
                var hasView = await Database.ViewExistsAsync("db_test_view_1");
                Assert.IsTrue(hasView);
            }

            [Test]
            public void TestPresenceOfMissingViewSync()
            {
                var hasView = Database.ViewExists("missing_view");
                Assert.IsFalse(hasView);
            }

            [Test]
            public async Task TestPresenceOfMissingViewAsync()
            {
                var hasView = await Database.ViewExistsAsync("missing_view");
                Assert.IsFalse(hasView);
            }
        }
    }
}
