using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Dapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(new SqlServerDialect(), Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual(Database.DefaultSchema, "dbo");
        }

        [TestFixture]
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

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(new SqlServerDialect(), Connection);

            [Test]
            public void ViewExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.ViewExists(null));
            }

            [Test]
            public void ViewExists_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = Database.ViewExists("db_test_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void ViewExists_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = Database.ViewExists("view_that_doesnt_exist");
                Assert.IsFalse(viewExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public void ViewExists_WhenViewPresentWithDifferentCase_ReturnsTrue()
            {
                var viewExists = Database.ViewExists("DB_TEST_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void GetView_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsView()
            {
                var view = Database.GetView("db_test_view_1");
                Assert.NotNull(view);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNull()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsNull(view);
            }

            [Test]
            public void ViewExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.ViewExistsAsync(null));
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = await Database.ViewExistsAsync("db_test_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = await Database.ViewExistsAsync("view_that_doesnt_exist");
                Assert.IsFalse(viewExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public async Task ViewExistsAsync_WhenViewPresentWithDifferentCase_ReturnsTrue()
            {
                var viewExists = await Database.ViewExistsAsync("DB_TEST_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetViewAsync(null));
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsView()
            {
                var view = await Database.GetViewAsync("db_test_view_1");
                Assert.NotNull(view);
            }

            [Test]
            public async Task GetViewAsync_WhenViewMissing_ReturnsNull()
            {
                var view = await Database.GetViewAsync("view_that_doesnt_exist");
                Assert.IsNull(view);
            }
        }
    }
}
