using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    [TestFixture]
    internal class SqliteRelationalDatabaseViewTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

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
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(null, Database, "test"));
        }

        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(Connection, null, "test"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(Connection, Database, null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var view = new SqliteRelationalDatabaseView(Connection, database, "view_test_view_1");

            Assert.AreSame(database, view.Database);
        }

        [Test]
        public void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            Assert.AreEqual(viewName, view.Name.LocalName);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var viewName = new LocalIdentifier("view_test_view_1");
            var expectedViewName = new Identifier(database.DefaultSchema, "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(Connection, database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("asd", "view_test_view_1");
            var expectedViewName = new Identifier("asd", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("qwe", "asd", "view_test_view_1");
            var expectedViewName = new Identifier("asd", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("qwe", "asd", "zxc", "view_test_view_1");
            var expectedViewName = new Identifier("zxc", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public void Definition_PropertyGet_ReturnsCorrectDefinition()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            var definition = view.Definition;
            const string expected = "create view view_test_view_1 as select 1 as test";

            var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(definitionEqual);
        }

        [Test]
        public async Task DefinitionAsync_PropertyGet_ReturnsCorrectDefinition()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);

            var definition = await view.DefinitionAsync().ConfigureAwait(false);
            const string expected = "create view view_test_view_1 as select 1 as test";

            var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(definitionEqual);
        }

        [Test]
        public void IsIndexed_WhenViewIsNotIndexed_ReturnsFalse()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");

            Assert.IsFalse(view.IsIndexed);
        }

        [Test]
        public void Index_WhenViewIsNotIndexed_ReturnsEmptyLookup()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");
            var indexCount = view.Index.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexAsync_WhenViewIsNotIndexed_ReturnsEmptyLookup()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");
            var indexes = await view.IndexAsync().ConfigureAwait(false);
            var indexCount = indexes.Count;

            Assert.Zero(indexCount);
        }

        [Test]
        public void Indexes_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");
            var indexCount = view.Indexes.Count();

            Assert.Zero(indexCount);
        }

        [Test]
        public async Task IndexesAsync_WhenViewIsNotIndexed_ReturnsEmptyCollection()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");
            var indexes = await view.IndexesAsync().ConfigureAwait(false);
            var indexCount = indexes.Count();

            Assert.Zero(indexCount);
        }

        [Test]
        public void Column_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Column.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Column_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            var view = new SqliteRelationalDatabaseView(Connection, Database, "view_test_view_1");
            var containsColumn = view.Column.ContainsKey("test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var containsColumn = view.Columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnAsync().ConfigureAwait(false);
            var containsColumn = columns.ContainsKey("test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsOneValueOnly()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(1, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsSingleColumn_ContainsColumnName()
        {
            const string viewName = "view_test_view_1";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var containsColumn = columns.Any(c => c.Name == "test");

            Assert.IsTrue(containsColumn);
        }

        [Test]
        public void Column_WhenViewContainsUnnamedColumns_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Column.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Integer, DataType.Float, DataType.Unicode, DataType.Binary };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsUnnamedColumns_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_2";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Integer, DataType.Float, DataType.Unicode, DataType.Binary };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public void Column_WhenViewContainsUnnamedColumnsAndTableColumn_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Column.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Binary, DataType.Binary, DataType.Binary, DataType.Binary, DataType.Integer };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsUnnamedColumnsAndTableColumn_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(5, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_3";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Binary, DataType.Binary, DataType.Binary, DataType.Binary, DataType.Integer };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public void Column_WhenViewContainsDuplicatedUnnamedColumns_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Column.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnCount = view.Columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public void Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Integer, DataType.Integer, DataType.Integer, DataType.Integer };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }

        [Test]
        public async Task ColumnAsync_WhenViewContainsDuplicatedUnnamedColumns_ReturnsNonEmptyLookup()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.Count;

            Assert.AreEqual(4, columnCount);
        }

        [Test]
        public async Task ColumnsAsync_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
        {
            const string viewName = "view_test_view_4";
            var view = new SqliteRelationalDatabaseView(Connection, Database, viewName);
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnTypes = view.Columns.Select(c => c.Type.Type).ToList();
            var expectedTypes = new[] { DataType.Integer, DataType.Integer, DataType.Integer, DataType.Integer };

            var typesEqual = columnTypes.SequenceEqual(expectedTypes);
            Assert.IsTrue(typesEqual);
        }
    }
}
