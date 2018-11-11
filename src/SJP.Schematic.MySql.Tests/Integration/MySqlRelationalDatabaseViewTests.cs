using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal sealed class MySqlRelationalDatabaseViewTests : MySqlTest
    {
        private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 as select table_id as test from view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var view = database.GetView(viewName).UnwrapSome();

            var definition = view.Definition;
            const string expected = "select 1 AS `test`";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var viewOption = await database.GetViewAsync(viewName).ConfigureAwait(false);
            var view = viewOption.UnwrapSome();

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "select 1 AS `test`";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = Database.GetView("view_test_view_1").UnwrapSome();

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Index_WhenViewIsNotIndexed_ReturnsEmptyLookup()
        {
            var view = Database.GetView("view_test_view_1").UnwrapSome();
            var indexCount = view.Index.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexAsync_WhenViewIsNotIndexed_ReturnsEmptyLookup()
        {
            var viewOption = await Database.GetViewAsync("view_test_view_1").ConfigureAwait(false);
            var indexes = await viewOption.UnwrapSome().IndexAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = Database.GetView("view_test_view_1").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var viewOption = await Database.GetViewAsync("view_test_view_1").ConfigureAwait(false);
            var indexes = await viewOption.UnwrapSome().IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Column_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var view = database.GetView(viewName).UnwrapSome();
            var columnCount = view.Column.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Column_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var view = database.GetView(viewName).UnwrapSome();
            var containsColumn = view.Column.ContainsKey("test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var view = database.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var view = database.GetView(viewName).UnwrapSome();
            var containsColumn = view.Columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var viewOption = await database.GetViewAsync(viewName).ConfigureAwait(false);
            var columns = await viewOption.UnwrapSome().ColumnAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var viewOption = await database.GetViewAsync(viewName).ConfigureAwait(false);
            var columns = await viewOption.UnwrapSome().ColumnAsync().ConfigureAwait(false);
            var containsColumn = columns.ContainsKey("test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var viewOption = await database.GetViewAsync(viewName).ConfigureAwait(false);
            var columns = await viewOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var database = Database;
            var viewName = new Identifier(database.DefaultSchema, "view_test_view_1");
            var viewOption = await database.GetViewAsync(viewName).ConfigureAwait(false);
            var columns = await viewOption.UnwrapSome().ColumnsAsync().ConfigureAwait(false);
            var containsColumn = columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }
    }
}
