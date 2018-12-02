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
        public SqliteRelationalDatabaseViewProviderTests()
        {
            const string defaultSchema = "main";
            var pragma = new ConnectionPragma(Dialect, Connection);
            IdentifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithSchema(defaultSchema)
                .Build();
            ViewProvider = new SqliteRelationalDatabaseViewProvider(Connection, pragma, Dialect, IdentifierDefaults, Dialect.TypeProvider);
        }

        private IDatabaseIdentifierDefaults IdentifierDefaults { get; }
        private IRelationalDatabaseViewProvider ViewProvider { get; }

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_2 as select 1, 2.345, 'asd', X'DEADBEEF'").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_3 as select 1, 2.345, 'asd', X'DEADBEEF', table_id from view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view view_test_view_4 as select 1, 1, 1, 1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view view_test_view_4").ConfigureAwait(false);
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
