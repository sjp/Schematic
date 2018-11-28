using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal sealed class SqlServerRelationalDatabaseTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual("dbo", Database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.IsNotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }

        internal sealed class TableTests : SqlServerTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create table db_test_table_1 ( title nvarchar(200) )");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop table db_test_table_1");
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetTable_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTable(null));
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTable()
            {
                var table = Database.GetTable("db_test_table_1");
                Assert.IsTrue(table.IsSome);
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                var table = Database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(tableName, table.Name.LocalName);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(tableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("A", "B", database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName).UnwrapSome();

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTableMissing_ReturnsNone()
            {
                var table = Database.GetTable("table_that_doesnt_exist");
                Assert.IsTrue(table.IsNone);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_table_1");
                var table = Database.GetTable(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_table_1");
                var table = Database.GetTable(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, table.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTableAsync(null));
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTable()
            {
                var tableIsSome = await Database.GetTableAsync("db_test_table_1").IsSome.ConfigureAwait(false);
                Assert.IsTrue(tableIsSome);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                var table = await Database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(tableName, table.Name.LocalName);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(tableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("A", "B", database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTableMissing_ReturnsNone()
            {
                var tableIsNone = await Database.GetTableAsync("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
                Assert.IsTrue(tableIsNone);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_table_1");
                var table = await Database.GetTableAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_table_1");
                var table = await Database.GetTableAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, table.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTables()
            {
                var tables = Database.Tables.ToList();

                Assert.NotZero(tables.Count);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTestTable()
            {
                var containsTestTable = Database.Tables.Any(t => t.Name.LocalName == "db_test_table_1");

                Assert.True(containsTestTable);
            }

            [Test]
            public async Task TablesAsync_WhenEnumerated_ContainsTables()
            {
                var tables = await Database.TablesAsync().ConfigureAwait(false);

                Assert.NotZero(tables.Count);
            }

            [Test]
            public async Task TablesAsync_WhenEnumerated_ContainsTestTable()
            {
                var tables = await Database.TablesAsync().ConfigureAwait(false);
                var containsTestTable = tables.Any(t => t.Name.LocalName == "db_test_table_1");

                Assert.True(containsTestTable);
            }
        }

        internal sealed class ViewTests : SqlServerTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop view db_test_view_1");
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetView_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsView()
            {
                var view = Database.GetView("db_test_view_1");
                Assert.IsTrue(view.IsSome);
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("A", "B", database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName).UnwrapSome();

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNone()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsTrue(view.IsNone);
            }

            [Test]
            public void GetView_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_view_1");
                var view = Database.GetView(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetView_WhenViewPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_view_1");
                var view = Database.GetView(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, view.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetViewAsync(null));
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsView()
            {
                var viewIsSome = await Database.GetViewAsync("db_test_view_1").IsSome.ConfigureAwait(false);
                Assert.IsTrue(viewIsSome);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("A", "B", database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewMissing_ReturnsNone()
            {
                var viewIsNone = await Database.GetViewAsync("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
                Assert.IsTrue(viewIsNone);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_view_1");
                var view = await Database.GetViewAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_view_1");
                var view = await Database.GetViewAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, view.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsViews()
            {
                var views = Database.Views.ToList();

                Assert.NotZero(views.Count);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsTestView()
            {
                const string viewName = "db_test_view_1";
                var containsTestView = Database.Views.Any(v => v.Name.LocalName == viewName);

                Assert.True(containsTestView);
            }

            [Test]
            public async Task ViewsAsync_WhenEnumerated_ContainsViews()
            {
                var views = await Database.ViewsAsync().ConfigureAwait(false);

                Assert.NotZero(views.Count);
            }

            [Test]
            public async Task ViewsAsync_WhenEnumerated_ContainsTestView()
            {
                const string viewName = "db_test_view_1";
                var views = await Database.ViewsAsync().ConfigureAwait(false);
                var containsTestView = views.Any(v => v.Name.LocalName == viewName);

                Assert.True(containsTestView);
            }
        }

        internal sealed class SequenceTests : SqlServerTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create sequence db_test_sequence_1 start with 1000 increment by 1");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop sequence db_test_sequence_1");
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSequence_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_WhenSequencePresent_ReturnsSequence()
            {
                var sequence = Database.GetSequence("db_test_sequence_1");
                Assert.IsTrue(sequence.IsSome);
            }

            [Test]
            public void GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
            {
                const string sequenceName = "db_test_sequence_1";
                var sequence = Database.GetSequence(sequenceName).UnwrapSome();

                Assert.AreEqual(sequenceName, sequence.Name.LocalName);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = database.GetSequence(sequenceName).UnwrapSome();

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = database.GetSequence(sequenceName).UnwrapSome();

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = database.GetSequence(sequenceName).UnwrapSome();

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = database.GetSequence(sequenceName).UnwrapSome();

                Assert.AreEqual(sequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequenceOption = database.GetSequence(sequenceName);
                var sequence = sequenceOption.UnwrapSome();

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("A", "B", database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequenceOption = database.GetSequence(sequenceName);
                var sequence = sequenceOption.UnwrapSome();

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequenceMissing_ReturnsNone()
            {
                var sequence = Database.GetSequence("sequence_that_doesnt_exist");
                Assert.IsTrue(sequence.IsNone);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_sequence_1");
                var sequence = Database.GetSequence(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, sequence.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_sequence_1");
                var sequence = Database.GetSequence(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, sequence.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, sequence.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetSequenceAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequence()
            {
                var sequenceIsSome = await Database.GetSequenceAsync("db_test_sequence_1").IsSome.ConfigureAwait(false);
                Assert.IsTrue(sequenceIsSome);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequenceWithCorrectName()
            {
                const string sequenceName = "db_test_sequence_1";
                var sequence = await Database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(sequenceName, sequence.Name.LocalName);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(sequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("A", "B", database.DefaultSchema, "db_test_sequence_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_sequence_1");

                var sequence = await database.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequenceMissing_ReturnsNone()
            {
                var sequenceIsNone = await Database.GetSequenceAsync("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
                Assert.IsTrue(sequenceIsNone);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_sequence_1");
                var sequence = await Database.GetSequenceAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, sequence.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_sequence_1");
                var sequence = await Database.GetSequenceAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, sequence.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, sequence.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void Sequences_WhenEnumerated_ContainsSequences()
            {
                var sequences = Database.Sequences.ToList();

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public void Sequences_WhenEnumerated_ContainsTestSequence()
            {
                var containsTestSequence = Database.Sequences.Any(s => s.Name.LocalName == "db_test_sequence_1");

                Assert.True(containsTestSequence);
            }

            [Test]
            public async Task SequencesAsync_WhenEnumerated_ContainsSequences()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public async Task SequencesAsync_WhenEnumerated_ContainsTestSequence()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);
                var containsTestSequence = sequences.Any(s => s.Name.LocalName == "db_test_sequence_1");

                Assert.True(containsTestSequence);
            }
        }

        internal sealed class SynonymTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create synonym db_test_synonym_1 for sys.tables").ConfigureAwait(false);

                await Connection.ExecuteAsync("create view synonym_test_view_1 as select 1 as test").ConfigureAwait(false);
                await Connection.ExecuteAsync("create table synonym_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
                await Connection.ExecuteAsync("create synonym synonym_test_synonym_1 for synonym_test_view_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("create synonym synonym_test_synonym_2 for synonym_test_table_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("create synonym synonym_test_synonym_3 for non_existent_target").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop synonym db_test_synonym_1").ConfigureAwait(false);

                await Connection.ExecuteAsync("drop view synonym_test_view_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("drop table synonym_test_table_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("drop synonym synonym_test_synonym_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("drop synonym synonym_test_synonym_2").ConfigureAwait(false);
                await Connection.ExecuteAsync("drop synonym synonym_test_synonym_3").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSynonym_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_WhenSynonymPresent_ReturnsSynonym()
            {
                var synonym = Database.GetSynonym("db_test_synonym_1");
                Assert.IsTrue(synonym.IsSome);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
            {
                var database = Database;
                var synonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");
                var synonym = database.GetSynonym(synonymName).UnwrapSome();

                Assert.AreEqual(synonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymExistsGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = database.GetSynonym(synonymName).UnwrapSome();

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymExistsGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = database.GetSynonym(synonymName).UnwrapSome();

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymExistsGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = database.GetSynonym(synonymName).UnwrapSome();

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymExistsGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = database.GetSynonym(synonymName).UnwrapSome();

                Assert.AreEqual(synonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonymOption = database.GetSynonym(synonymName);
                var synonym = synonymOption.UnwrapSome();

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("A", "B", database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonymOption = database.GetSynonym(synonymName);
                var synonym = synonymOption.UnwrapSome();

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymMissing_ReturnsNone()
            {
                var synonym = Database.GetSynonym("synonym_that_doesnt_exist");
                Assert.IsTrue(synonym.IsNone);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_synonym_1");
                var synonym = Database.GetSynonym(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonym.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_synonym_1");
                var synonym = Database.GetSynonym(inputName).UnwrapSome();

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonym.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonym.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonym()
            {
                var synonymIsSome = await Database.GetSynonymAsync("db_test_synonym_1").IsSome.ConfigureAwait(false);
                Assert.IsTrue(synonymIsSome);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
            {
                var database = Database;
                const string synonymName = "db_test_synonym_1";
                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(synonymName, synonym.Name.LocalName);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymExistsGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymExistsGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymExistsGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymExistsGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(synonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("A", database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var synonymName = new Identifier("A", "B", database.DefaultSchema, "db_test_synonym_1");
                var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_synonym_1");

                var synonym = await database.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymMissing_ReturnsNone()
            {
                var synonymIsNone = await Database.GetSynonymAsync("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
                Assert.IsTrue(synonymIsNone);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_synonym_1");
                var synonym = await Database.GetSynonymAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonym.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Dbo", "DB_TEST_synonym_1");
                var synonym = await Database.GetSynonymAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonym.Name.Schema)
                    && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonym.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsSynonyms()
            {
                var synonyms = Database.Synonyms.ToList();

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsTestSynonym()
            {
                var containsTestSynonym = Database.Synonyms.Any(s => s.Name.LocalName == "db_test_synonym_1");

                Assert.True(containsTestSynonym);
            }

            [Test]
            public async Task SynonymsAsync_WhenEnumerated_ContainsSynonyms()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public async Task SynonymsAsync_WhenEnumerated_ContainsTestSynonym()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);
                var containsTestSynonym = synonyms.Any(s => s.Name.LocalName == "db_test_synonym_1");

                Assert.IsTrue(containsTestSynonym);
            }

            [Test]
            public void GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                const string expectedTarget = "synonym_test_view_1";
                var synonym = database.GetSynonym("synonym_test_synonym_1").UnwrapSome();

                Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
            }

            [Test]
            public void GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                const string expectedTarget = "synonym_test_table_1";
                var synonym = database.GetSynonym("synonym_test_synonym_2").UnwrapSome();

                Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
            }

            [Test]
            public void GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "non_existent_target");
                var synonym = database.GetSynonym("synonym_test_synonym_3").UnwrapSome();

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_view_1");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_1").UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_table_1");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_2").UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "non_existent_target");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_3").UnwrapSomeAsync().ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }
        }
    }
}
