using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleRelationalDatabaseViewProviderTests : OracleTest
    {
        public OracleRelationalDatabaseViewProviderTests()
        {
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            var database = new OracleRelationalDatabase(Dialect, Connection, identifierResolver);
            IdentifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(database.ServerName)
                .WithDatabase(database.DatabaseName)
                .WithSchema(database.DefaultSchema)
                .Build();
            ViewProvider = new OracleRelationalDatabaseViewProvider(Connection, IdentifierDefaults, identifierResolver, Dialect.TypeProvider);
        }

        private IDatabaseIdentifierDefaults IdentifierDefaults { get; }
        private IRelationalDatabaseViewProvider ViewProvider { get; }

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy from dual").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test from dual").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id number)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create materialized view view_test_view_2 as select table_id as test from view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_view_test_view_2 on view_test_view_2 (test)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop materialized view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void GetView_WhenViewPresent_ReturnsView()
        {
            var view = ViewProvider.GetView("db_test_view_1");
            Assert.IsTrue(view.IsSome);
        }

        [Test]
        public void GetView_WhenViewPresent_ReturnsViewWithCorrectName()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewMissing_ReturnsNone()
        {
            var view = ViewProvider.GetView("view_that_doesnt_exist");
            Assert.IsTrue(view.IsNone);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresent_ReturnsView()
        {
            var viewIsSome = await ViewProvider.GetViewAsync("db_test_view_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresent_ReturnsViewWithCorrectName()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_VIEW_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewMissing_ReturnsNone()
        {
            var viewIsNone = await ViewProvider.GetViewAsync("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public void Views_WhenEnumerated_ContainsViews()
        {
            var views = ViewProvider.Views.ToList();

            Assert.NotZero(views.Count);
        }

        [Test]
        public void Views_WhenEnumerated_ContainsTestView()
        {
            const string viewName = "DB_TEST_VIEW_1";
            var containsTestView = ViewProvider.Views.Any(v => v.Name.LocalName == viewName);

            Assert.True(containsTestView);
        }

        [Test]
        public async Task ViewsAsync_WhenEnumerated_ContainsViews()
        {
            var views = await ViewProvider.ViewsAsync().ConfigureAwait(false);

            Assert.NotZero(views.Count);
        }

        [Test]
        public async Task ViewsAsync_WhenEnumerated_ContainsTestView()
        {
            const string viewName = "DB_TEST_VIEW_1";
            var views = await ViewProvider.ViewsAsync().ConfigureAwait(false);
            var containsTestView = views.Any(v => v.Name.LocalName == viewName);

            Assert.True(containsTestView);
        }

        [Test]
        public void Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            var definition = view.Definition;
            const string expected = "select 1 as test from dual";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "select 1 as test from dual";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = ViewProvider.GetView("VIEW_TEST_VIEW_1").UnwrapSome();

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = ViewProvider.GetView("VIEW_TEST_VIEW_1").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = await ViewProvider.GetViewAsync("VIEW_TEST_VIEW_1").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string expectedColumnName = "TEST";

            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var containsColumn = view.Columns.Any(c => c.Name == expectedColumnName);

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string expectedColumnName = "TEST";

            var viewName = new Identifier(IdentifierDefaults.Schema, "VIEW_TEST_VIEW_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var containsColumn = columns.Any(c => c.Name == expectedColumnName);

            Assert.IsTrue(containsColumn);
        }

        // TODO: uncomment when materialized view support is available
        /*
        [Test]
        public void IsIndexed_WhenViewHasSingleIndex_ReturnsTrue()
        {
            var view = ViewProvider.GetView("VIEW_TEST_VIEW_2").UnwrapSome();

            Assert.IsTrue(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var view = ViewProvider.GetView("VIEW_TEST_VIEW_2").UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsOneValueOnly()
        {
            var view = await ViewProvider.GetViewAsync("VIEW_TEST_VIEW_2").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.AreEqual(1, indexCount);
        }

        [Test]
        public void Indexes_WhenViewHasSingleIndex_ContainsIndexName()
        {
            const string indexName = "IX_VIEW_TEST_VIEW_2";
            var view = ViewProvider.GetView("VIEW_TEST_VIEW_2").UnwrapSome();
            var containsIndex = view.Indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }

        [Test]
        public async Task IndexesAsync_WhenViewHasSingleIndex_ContainsIndexName()
        {
            const string indexName = "IX_VIEW_TEST_VIEW_2";
            var view = await ViewProvider.GetViewAsync("VIEW_TEST_VIEW_2").UnwrapSomeAsync().ConfigureAwait(false);
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var containsIndex = indexes.Any(i => i.Name == indexName);

            Assert.IsTrue(containsIndex);
        }
        */
    }
}
