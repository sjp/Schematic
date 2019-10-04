using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal sealed class SqliteDatabaseViewProviderTests : SqliteTest
    {
        private IDatabaseViewProvider ViewProvider => new SqliteDatabaseViewProvider(Connection, Pragma, Dialect, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 as select 1, 2.345, 'test', X'DEADBEEF'").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_3 as select 1, 2.345, 'test', X'DEADBEEF', table_id from view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_4 as select 1, 1, 1, 1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_4").ConfigureAwait(false);
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

                return lazyView.GetValueAsync(CancellationToken.None);
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseView>>();

        [Test]
        public async Task GetView_WhenViewPresent_ReturnsView()
        {
            var viewIsSome = await ViewProvider.GetView("db_test_view_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
        {
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetView(expectedViewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("test", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

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
        public async Task GetView_WhenViewPresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_view_1");
            var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetView_WhenViewPresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Main", "DB_TEST_view_1");
            var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);

            var definition = view.Definition;
            const string expected = "create view view_test_view_1 as select 1 as test";

            var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(definitionEqual);
        }

        [Test]
        public async Task IsMaterialized_WhenViewIsNotMaterialized_ReturnsFalse()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);

            Assert.IsFalse(view.IsMaterialized);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var containsColumn = view.Columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task Columns_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.Float, DataType.UnicodeText, DataType.LargeBinary };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = await GetViewAsync(viewName).ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.BigInteger, DataType.BigInteger, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }
    }
}
