using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal sealed class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
    {
        private IRelationalDatabaseTableProvider TableProvider => new PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync("create table db_test_table_1 ( title varchar(200) )", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop table db_test_table_1", CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableProvider.GetTable("db_test_table_1").IsSome.ConfigureAwait(false);
            Assert.That(tableIsSome, Is.True);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "db_test_table_1";
            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name.LocalName, Is.EqualTo(tableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(tableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTableMissing_ReturnsNone()
        {
            var tableIsNone = await TableProvider.GetTable("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(tableIsNone, Is.True);
        }

        [Test]
        public async Task GetAllTables_WhenEnumerated_ContainsTables()
        {
            var hasTables = await TableProvider.GetAllTables()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasTables, Is.True);
        }

        [Test]
        public async Task GetAllTables_WhenEnumerated_ContainsTestTable()
        {
            var containsTestTable = await TableProvider.GetAllTables()
                .AnyAsync(t => string.Equals(t.Name.LocalName, "db_test_table_1", System.StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestTable, Is.True);
        }
    }
}