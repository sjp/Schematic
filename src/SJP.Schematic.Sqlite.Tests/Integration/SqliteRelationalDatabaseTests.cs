using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    internal sealed class SqliteRelationalDatabaseTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualMain()
        {
            Assert.AreEqual("main", Database.DefaultSchema);
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

        [Test]
        public void Vacuum_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            sqliteDb.Vacuum();
        }

        [Test]
        public Task VacuumAsync_WhenInvoked_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            return sqliteDb.VacuumAsync();
        }

        [Test]
        public void Vacuum_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            sqliteDb.Vacuum("main");
        }

        [Test]
        public void Vacuum_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            Assert.Throws<SqliteException>(() => sqliteDb.Vacuum("asdas"));
        }

        [Test]
        public Task VacuumAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            return sqliteDb.VacuumAsync("main");
        }

        [Test]
        public void VacuumAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Connection);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.VacuumAsync("asdas").ConfigureAwait(false));
        }

        [Test]
        public void AttachDatabase_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            sqliteDb.AttachDatabase("test", ":memory:");
        }

        [Test]
        public Task AttachDatabaseAsync_WhenGivenValidSchemaAndFileNames_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            return sqliteDb.AttachDatabaseAsync("test", ":memory:");
        }

        [Test]
        public void DetachDatabase_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            sqliteDb.AttachDatabase("test", ":memory:");
            sqliteDb.DetachDatabase("test");
        }

        [Test]
        public async Task DetachDatabaseAsync_WhenGivenValidSchemaName_RunsWithoutError()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            await sqliteDb.AttachDatabaseAsync("test", ":memory:").ConfigureAwait(false);
            await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false);
        }

        [Test]
        public void DetachDatabase_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            Assert.Throws<SqliteException>(() => sqliteDb.DetachDatabase("test"));
        }

        [Test]
        public void DetachDatabaseAsync_WhenGivenUnknownSchemaName_ThrowsSqliteException()
        {
            var sqliteDb = new SqliteRelationalDatabase(Dialect, Config.Connection);
            Assert.ThrowsAsync<SqliteException>(async () => await sqliteDb.DetachDatabaseAsync("test").ConfigureAwait(false));
        }

        internal sealed class TableTests : SqliteTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create table db_test_table_1 (id integer)").ConfigureAwait(false);
                await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as test").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);
                await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetTable_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTable(null));
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTable()
            {
                var table = Database.GetTable("db_test_table_1");
                Assert.NotNull(table);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("db_test_table_1");
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(expectedTableName);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier(database.DefaultSchema, "main", "db_test_table_1");
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = database.GetTable(tableName);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTableMissing_ReturnsNull()
            {
                var table = Database.GetTable("table_that_doesnt_exist");
                Assert.IsNull(table);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_table_1");
                var table = Database.GetTable(inputName);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetTable_WhenTablePresentGivenQualifiedNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Main", "DB_TEST_table_1");
                var table = Database.GetTable(inputName);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name);
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
                var table = await Database.GetTableAsync("db_test_table_1").ConfigureAwait(false);
                Assert.NotNull(table);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("db_test_table_1");
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(expectedTableName).ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var tableName = new Identifier("asd", database.DefaultSchema, "db_test_table_1");
                var expectedTableName = new Identifier(database.DefaultSchema, "db_test_table_1");

                var table = await database.GetTableAsync(tableName).ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name);
            }

            [Test]
            public async Task GetTableAsync_WhenTableMissing_ReturnsNull()
            {
                var table = await Database.GetTableAsync("table_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(table);
            }

            [Test]
            public async Task TableExistsAsync_WhenTablePresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_table_1");
                var table = await Database.GetTableAsync(inputName).ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task TableExistsAsync_WhenTablePresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Main", "DB_TEST_table_1");
                var table = await Database.GetTableAsync(inputName).ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name);
                Assert.IsTrue(equalNames);
            }
        }

        internal sealed class ViewTests : SqliteTest
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

            private IRelationalDatabase Database => new SqliteRelationalDatabase(new SqliteDialect(), Connection);

            [Test]
            public void GetView_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsView()
            {
                var view = Database.GetView("db_test_view_1");
                Assert.NotNull(view);
            }

            [Test]
            public void GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("db_test_view_1");
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(expectedViewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("asd", database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNull()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsNull(view);
            }

            [Test]
            public void GetView_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_view_1");
                var view = Database.GetView(inputName);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public void GetView_WhenViewPresentGivenQualifiedNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Main", "DB_TEST_view_1");
                var view = Database.GetView(inputName);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
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
                var view = await Database.GetViewAsync("db_test_view_1").ConfigureAwait(false);
                Assert.NotNull(view);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("db_test_view_1");
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(expectedViewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("asd", database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.DefaultSchema, "db_test_view_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewMissing_ReturnsNull()
            {
                var view = await Database.GetViewAsync("view_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(view);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("DB_TEST_view_1");
                var view = await Database.GetViewAsync(inputName).ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
                Assert.IsTrue(equalNames);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
            {
                var inputName = new Identifier("Main", "DB_TEST_view_1");
                var view = await Database.GetViewAsync(inputName).ConfigureAwait(false);

                var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
                Assert.IsTrue(equalNames);
            }
        }

        internal sealed class SequenceTests : SqliteTest
        {
            private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_GivenValidSequenceName_ReturnsNull()
            {
                var sequenceName = new Identifier("asd");
                var sequence = Database.GetSequence(sequenceName);

                Assert.IsNull(sequence);
            }

            [Test]
            public void Sequences_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = Database.Sequences.ToList();
                var count = sequences.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSequenceAsync_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public async Task GetSequenceAsync_GivenValidSequenceName_ReturnsNull()
            {
                var sequenceName = new Identifier("asd");
                var sequence = await Database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.IsNull(sequence);
            }

            [Test]
            public async Task SequencesAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);

                Assert.Zero(sequences.Count);
            }
        }

        internal sealed class SynonymTests : SqliteTest
        {
            private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_GivenValidSynonymName_ReturnsNull()
            {
                var synonymName = new Identifier("asd");
                var synonym = Database.GetSynonym(synonymName);

                Assert.IsNull(synonym);
            }

            [Test]
            public void Synonyms_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = Database.Synonyms.ToList();
                var count = synonyms.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSynonymAsync_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_GivenValidSynonymName_ReturnsNull()
            {
                var synonymName = new Identifier("asd");
                var synonym = await Database.GetSynonymAsync(synonymName).ConfigureAwait(false);

                Assert.IsNull(synonym);
            }

            [Test]
            public async Task SynonymsAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.Zero(synonyms.Count);
            }
        }
    }
}
