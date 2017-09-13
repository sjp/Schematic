using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTableTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table table_test_table_1 ( test_column int )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_2 ( test_column int not null primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_5 ( test_column int not null unique )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_8 (
    test_column int,
    index ix_test_table_8 (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    index ix_test_table_9 (first_name, last_name, middle_name)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table table_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_9").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(null, Database, "test"));
        }

        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(Connection, null, "test"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(Connection, Database, null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var table = new SqlServerRelationalDatabaseTable(Connection, database, "table_test_table_1");

            Assert.AreSame(database, table.Database);
        }

        [Test]
        public void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string tableName = "table_test_table_1";
            var table = new SqlServerRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(tableName, table.Name.LocalName);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new LocalIdentifier("table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, "asd", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("qwe", "asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, "qwe", "asd", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");
            var expectedTableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNull()
        {
            var table = Database.GetTable("table_test_table_1");

            Assert.IsNull(table.PrimaryKey);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_2");

            Assert.AreEqual(DatabaseKeyType.Primary, table.PrimaryKey.KeyType);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_2");
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_3");
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_3");
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_4");
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            var columnsEqual = pkColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, pkColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_4");
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithNoPrimaryKey_ReturnsNull()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);

            Assert.IsNull(pk);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);

            Assert.AreEqual(DatabaseKeyType.Primary, table.PrimaryKey.KeyType);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            var columnsEqual = pkColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, pkColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = await table.PrimaryKeyAsync().ConfigureAwait(false);

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.UniqueKey.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKey.Values.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKey_WhenQueriedByName_ReturnsCorrectKey()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKey.Values.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey["uk_test_table_6"];
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKey["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKey["uk_test_table_7"];
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void UniqueKey_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKey["uk_test_table_7"];

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.UniqueKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_7");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var count = ukLookup.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup.Values.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenQueriedByName_ReturnsCorrectKey()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup.Values.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_6"];
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_6"];

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_7"];
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task UniqueKeyAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var ukLookup = await table.UniqueKeyAsync().ConfigureAwait(false);
            var uk = ukLookup["uk_test_table_7"];

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var count = uniqueKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_5").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_6").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task UniqueKeysAsync_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_7").ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var uk = uniqueKeys.Single();

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }
















        [Test]
        public void Index_WhenGivenTableWithNoIndexes_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Index.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Index_WhenQueriedByName_ReturnsCorrectIndex()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Index["ix_test_table_8"];

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public void Index_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Index["ix_test_table_8"];
            var indexColumns = index.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("test_column", indexColumns.Single().DependentColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Index["ix_test_table_8"];

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public void Index_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_9");
            var index = table.Index["ix_test_table_9"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Index["ix_test_table_9"];

            Assert.AreEqual("ix_test_table_9", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.Indexes.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("test_column", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_8");
            var index = table.Indexes.Single();

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.AreEqual("ix_test_table_9", index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithNoIndexes_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var count = indexLookup.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task IndexAsync_WhenQueriedByName_ReturnsCorrectIndex()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_8"];

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_8"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("test_column", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_8"];

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_9"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_9"];

            Assert.AreEqual("ix_test_table_9", index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var count = indexes.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.AreEqual("test_column", indexColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_8").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("ix_test_table_8", index.Name.LocalName);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.AreEqual("ix_test_table_9", index.Name.LocalName);
        }

        // TODO
        // - included columns
        // - isenabled
        // - unique/non-unique
    }
}
