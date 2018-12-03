using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal sealed class SqliteRelationalDatabaseViewProviderTests : SqliteTest
    {
        private IRelationalDatabaseViewProvider ViewProvider => new SqliteRelationalDatabaseViewProvider(Connection, Pragma, Dialect, IdentifierDefaults, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 as select 1, 2.345, 'asd', X'DEADBEEF'").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_3 as select 1, 2.345, 'asd', X'DEADBEEF', table_id from view_test_table_1").ConfigureAwait(false);
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

        [Test]
        public void GetView_WhenViewPresent_ReturnsView()
        {
            var view = ViewProvider.GetView("db_test_view_1");
            Assert.IsTrue(view.IsSome);
        }

        [Test]
        public void GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
        {
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = ViewProvider.GetView(expectedViewName).UnwrapSome();

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void GetView_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("asd", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

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
        public void GetView_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_view_1");
            var view = ViewProvider.GetView(inputName).UnwrapSome();

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public void GetView_WhenViewPresentGivenQualifiedNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Main", "DB_TEST_view_1");
            var view = ViewProvider.GetView(inputName).UnwrapSome();

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresent_ReturnsView()
        {
            var viewIsSome = await ViewProvider.GetViewAsync("db_test_view_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
        {
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

            var view = await ViewProvider.GetViewAsync(expectedViewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("asd", IdentifierDefaults.Schema, "db_test_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

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
        public async Task GetViewAsync_WhenViewPresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_view_1");
            var view = await ViewProvider.GetViewAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetViewAsync_WhenViewPresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Main", "DB_TEST_view_1");
            var view = await ViewProvider.GetViewAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public void Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            var definition = view.Definition;
            const string expected = "create view view_test_view_1 as select 1 as test";

            var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(definitionEqual);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "create view view_test_view_1 as select 1 as test";

            var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(definitionEqual);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var indexCount = view.Indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
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
        public void Columns_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.Float, DataType.UnicodeText, DataType.LargeBinary };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.Float, DataType.UnicodeText, DataType.LargeBinary };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public void Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = ViewProvider.GetView(viewName).UnwrapSome();
            var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.BigInteger, DataType.BigInteger, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
            var view = await ViewProvider.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = columns.Select(c => c.Type.DataType).ToList();
            var expectedTypes = new[] { DataType.BigInteger, DataType.BigInteger, DataType.BigInteger, DataType.BigInteger };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }
    }
}
