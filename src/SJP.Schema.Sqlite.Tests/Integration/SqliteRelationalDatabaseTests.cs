using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Dapper;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite.Tests.Integration
{
    [TestFixture]
    internal class SqliteRelationalDatabaseTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(SqliteDialect.Instance, Connection);

        [Test]
        public void DatabaseNameMatches()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void CorrectSchemaName()
        {
            Assert.AreEqual(Database.DefaultSchema, null);
        }

        [TestFixture]
        internal class TableTests : SqliteTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create table db_test_table_presence (id integer)");
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop table db_test_table_presence");
            }

            private IRelationalDatabase Database => new SqliteRelationalDatabase(SqliteDialect.Instance, Connection);

            [Test]
            public void TableExistsThrowsExceptionOnMissingName()
            {
                Assert.Throws<ArgumentNullException>(() => Database.TableExists(null));
                Assert.Throws<ArgumentNullException>(() => Database.TableExists(string.Empty));
            }

            [Test]
            public void TableExistsAsyncThrowsExceptionOnMissingName()
            {
                Assert.ThrowsAsync<ArgumentNullException>(() => Database.TableExistsAsync(null));
                Assert.ThrowsAsync<ArgumentNullException>(() => Database.TableExistsAsync(string.Empty));
            }

            [Test]
            public void TestPresenceOfPresentTableSync()
            {
                var hasTable = Database.TableExists("db_test_table_presence");
                Assert.IsTrue(hasTable);
            }

            [Test]
            public async Task TestPresenceOfPresentTableAsync()
            {
                var hasTable = await Database.TableExistsAsync("db_test_table_presence");
                Assert.IsTrue(hasTable);
            }

            [Test]
            public void TestPresenceOfMissingTableSync()
            {
                var hasTable = Database.TableExists("missing_table");
                Assert.IsFalse(hasTable);
            }

            [Test]
            public async Task TestPresenceOfMissingTableAsync()
            {
                var hasTable = await Database.TableExistsAsync("missing_table");
                Assert.IsFalse(hasTable);
            }
        }

        [TestFixture]
        internal class ViewTests : SqliteTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create view db_test_view_presence as select 1 as dummy");
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop view db_test_view_presence");
            }

            private IRelationalDatabase Database => new SqliteRelationalDatabase(SqliteDialect.Instance, Connection);

            [Test]
            public void ViewExistsThrowsExceptionOnMissingName()
            {
                Assert.Throws<ArgumentNullException>(() => Database.ViewExists(null));
                Assert.Throws<ArgumentNullException>(() => Database.ViewExists(string.Empty));
            }

            [Test]
            public void ViewExistsAsyncThrowsExceptionOnMissingName()
            {
                Assert.ThrowsAsync<ArgumentNullException>(() => Database.ViewExistsAsync(null));
                Assert.ThrowsAsync<ArgumentNullException>(() => Database.ViewExistsAsync(string.Empty));
            }

            [Test]
            public void TestPresenceOfPresentViewSync()
            {
                var hasView = Database.ViewExists("db_test_view_presence");
                Assert.IsTrue(hasView);
            }

            [Test]
            public async Task TestPresenceOfPresentViewAsync()
            {
                var hasView = await Database.ViewExistsAsync("db_test_view_presence");
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
