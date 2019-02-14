using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseQueryViewProviderTests : OracleTest
    {
        private IDatabaseViewProvider ViewProvider => new OracleDatabaseQueryViewProvider(Connection, IdentifierDefaults, IdentifierResolver, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view query_db_test_view_1 as select 1 as dummy from dual").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view query_view_test_view_1 as select 1 as test from dual").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table query_view_test_table_1 (table_id number)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create materialized view query_view_test_view_2 as select table_id as test from query_view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view query_db_test_view_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop view query_view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop materialized view query_view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table query_view_test_table_1").ConfigureAwait(false);
        }

        private Task<IDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            lock (_lock)
            {
                if (!_viewsCache.TryGetValue(viewName, out var lazyView))
                {
                    lazyView = new AsyncLazy<IDatabaseView>(() => ViewProvider.GetView(viewName).UnwrapSomeAsync());
                    _viewsCache[viewName] = lazyView;
                }

                return lazyView.Task;
            }
        }

        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = new ConcurrentDictionary<Identifier, AsyncLazy<IDatabaseView>>();

        [Test]
        public async Task GetView_WhenViewPresent_ReturnsView()
        {
            var viewIsSome = await ViewProvider.GetView("query_db_test_view_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");
            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "query_db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewMissing_ReturnsNone()
        {
            var viewIsNone = await ViewProvider.GetView("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public async Task GetView_WhenGivenMaterializedViewName_ReturnsNone()
        {
            var viewIsNone = await ViewProvider.GetView("query_view_test_view_2").IsNone.ConfigureAwait(false);
            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public async Task GetAllViews_WhenEnumerated_ContainsViews()
        {
            var views = await ViewProvider.GetAllViews().ConfigureAwait(false);

            Assert.NotZero(views.Count);
        }

        [Test]
        public async Task GetAllViews_WhenEnumerated_ContainsTestView()
        {
            const string viewName = "QUERY_DB_TEST_VIEW_1";
            var views = await ViewProvider.GetAllViews().ConfigureAwait(false);
            var containsTestView = views.Any(v => v.Name.LocalName == viewName);

            Assert.IsTrue(containsTestView);
        }

        [Test]
        public async Task GetAllViews_WhenEnumerated_DoesNotContainMaterializedView()
        {
            const string viewName = "QUERY_VIEW_TEST_VIEW_2";
            var views = await ViewProvider.GetAllViews().ConfigureAwait(false);
            var containsTestView = views.Any(v => v.Name.LocalName == viewName);

            Assert.IsFalse(containsTestView);
        }

        [Test]
        public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var view = await GetViewAsync("query_view_test_view_1").ConfigureAwait(false);

            var definition = view.Definition;
            const string expected = "select 1 as test from dual";

            Assert.AreEqual(expected, definition);
        }

        [Test]
        public async Task IsMaterialized_WhenViewIsNotMaterialized_ReturnsFalse()
        {
            var view = await GetViewAsync("query_view_test_view_1").ConfigureAwait(false);

            Assert.IsFalse(view.IsMaterialized);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var view = await GetViewAsync("query_view_test_view_1").ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string expectedColumnName = "TEST";
            var view = await GetViewAsync("query_view_test_view_1").ConfigureAwait(false);
            var containsColumn = view.Columns.Any(c => c.Name == expectedColumnName);

            Assert.IsTrue(containsColumn);
        }
    }
}
