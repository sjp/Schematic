using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
    {
        private IRelationalDatabaseTableProvider TableProvider => new PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table db_test_table_1 ( title varchar(200) )").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableProvider.GetTable("db_test_table_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "db_test_table_1";
            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, table.Name.LocalName);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTableMissing_ReturnsNone()
        {
            var tableIsNone = await TableProvider.GetTable("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public async Task GetAllTables_WhenEnumerated_ContainsTables()
        {
            var hasTables = await TableProvider.GetAllTables()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsTrue(hasTables);
        }

        [Test]
        public async Task GetAllTables_WhenEnumerated_ContainsTestTable()
        {
            var containsTestTable = await TableProvider.GetAllTables()
                .AnyAsync(t => t.Name.LocalName == "db_test_table_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestTable);
        }
    }
}