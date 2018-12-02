using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal sealed class SqlServerRelationalDatabaseViewProviderTests : SqlServerTest
    {
        public SqlServerRelationalDatabaseViewProviderTests()
        {
            var database = new SqlServerRelationalDatabase(Dialect, Connection);
            IdentifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(database.ServerName)
                .WithDatabase(database.DatabaseName)
                .WithSchema(database.DefaultSchema)
                .Build();
            ViewProvider = new SqlServerRelationalDatabaseViewProvider(Connection, IdentifierDefaults, Dialect.TypeProvider);
        }

        private IDatabaseIdentifierDefaults IdentifierDefaults { get; }
        private IRelationalDatabaseViewProvider ViewProvider { get; }

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 with schemabinding as select table_id as test from [dbo].[view_test_table_1]").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique clustered index ix_view_test_view_2 on view_test_view_2 (test)").ConfigureAwait(false);
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
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            var definition = view.Definition;
            const string expected = "create view view_test_view_1 as select 1 as test";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "create view view_test_view_1 as select 1 as test";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = ViewProvider.GetView("view_test_view_1").UnwrapSome();

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = ViewProvider.GetView("view_test_view_1").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = await ViewProvider.GetViewAsync("view_test_view_1").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var containsColumn = view.Columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var containsColumn = columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public void IsIndexed_WhenViewHasSingleIndex_ReturnsTrue()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.IsTrue(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsIndexName()
        {
            Identifier indexName = "ix_view_test_view_2";
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var containsIndex = view.Indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsIndexName()
        {
            Identifier indexName = "ix_view_test_view_2";
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var containsIndex = indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }
    }
}
