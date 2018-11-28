using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleRelationalDatabaseViewTests : OracleTest
    {
        private IIdentifierResolutionStrategy IdentifierResolver { get; } = new DefaultOracleIdentifierResolutionStrategy();
        private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test from dual").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id number)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create materialized view view_test_view_2 as select table_id as test from view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_view_test_view_2 on view_test_view_2 (test)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop materialized view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = database.GetView(viewName).UnwrapSome();

            var definition = view.Definition;
            const string expected = "select 1 as test from dual";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "select 1 as test from dual";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = Database.GetView("VIEW_TEST_VIEW_1").UnwrapSome();

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = Database.GetView("VIEW_TEST_VIEW_1").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = await Database.GetViewAsync("VIEW_TEST_VIEW_1").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(Database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = Database.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string expectedColumnName = "TEST";

            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = database.GetView(viewName).UnwrapSome();
            var containsColumn = view.Columns.Any(c => c.Name == expectedColumnName);

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string expectedColumnName = "TEST";

            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "VIEW_TEST_VIEW_1");
            var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var containsColumn = columns.Any(c => c.Name == expectedColumnName);

            Assert.IsTrue(containsColumn);
        }

        // TODO: uncomment when materialized view support is available
        /*
        [Test]
        public void IsIndexed_WhenViewHasSingleIndex_ReturnsTrue()
        {
            var view = Database.GetView("VIEW_TEST_VIEW_2").UnwrapSome();

            Assert.IsTrue(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var view = Database.GetView("VIEW_TEST_VIEW_2").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var view = await Database.GetViewAsync("VIEW_TEST_VIEW_2").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsIndexName()
        {
            const string indexName = "IX_VIEW_TEST_VIEW_2";
            var view = Database.GetView("VIEW_TEST_VIEW_2").UnwrapSome();
            var containsIndex = view.Indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsIndexName()
        {
            const string indexName = "IX_VIEW_TEST_VIEW_2";
            var view = await Database.GetViewAsync("VIEW_TEST_VIEW_2").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var containsIndex = indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }
        */
    }
}
