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
            await Connection.ExecuteAsync(@"
create table table_test_table_10 (
    test_column int,
    test_column_2 int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_10 on table_test_table_10 (test_column) include (test_column_2)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_11 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_11 on table_test_table_11 (first_name) include (last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_12 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_12 on table_test_table_12 (first_name) include (last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter index ix_test_table_12 on table_test_table_12 disable").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check ([test_column]>(1))
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
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
            await Connection.ExecuteAsync("drop table table_test_table_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_11").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_12").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_13").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_14").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_16").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_17").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_15").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table table_test_table_18").ConfigureAwait(false);
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

        [Test]
        public void Index_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = Database.GetTable("table_test_table_9");
            var indexLookup = table.Index;
            var index = indexLookup["ix_test_table_9"];
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            Assert.AreEqual(0, includedColumns.Count);
        }

        [Test]
        public void Index_WhenGivenTableWithIndexContainingSingleIncludedColumn_ReturnsIndexWithIncludedColumn()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = Database.GetTable("table_test_table_10");
            var indexLookup = table.Index;
            var index = indexLookup["ix_test_table_10"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(1, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithIndexContainingMultipleIncludedColumns_ReturnsIndexWithAllColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name" };
            var expectedIncludedColumnNames = new[] { "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_11");
            var indexLookup = table.Index;
            var index = indexLookup["ix_test_table_11"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(2, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            Assert.AreEqual(0, includedColumns.Count);
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingSingleIncludedColumn_ReturnsIndexWithIncludedColumn()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = Database.GetTable("table_test_table_10");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(1, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Indexes_WhenGivenTableWithIndexContainingMultipleIncludedColumns_ReturnsIndexWithAllColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name" };
            var expectedIncludedColumnNames = new[] { "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_11");
            var index = table.Indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(2, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_9"];
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            Assert.AreEqual(0, includedColumns.Count);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithIndexContainingSingleIncludedColumn_ReturnsIndexWithIncludedColumn()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = await Database.GetTableAsync("table_test_table_10").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_10"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(1, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithIndexContainingMultipleIncludedColumns_ReturnsIndexWithAllColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name" };
            var expectedIncludedColumnNames = new[] { "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_11").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup["ix_test_table_11"];
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(2, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            Assert.AreEqual(0, includedColumns.Count);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithIndexContainingSingleIncludedColumn_ReturnsIndexWithIncludedColumn()
        {
            var expectedColumnNames = new[] { "test_column" };
            var expectedIncludedColumnNames = new[] { "test_column_2" };

            var table = await Database.GetTableAsync("table_test_table_10").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(1, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithIndexContainingMultipleIncludedColumns_ReturnsIndexWithAllColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name" };
            var expectedIncludedColumnNames = new[] { "last_name", "middle_name" };

            var table = await Database.GetTableAsync("table_test_table_11").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();
            var indexColumns = index.Columns
                .Select(c => c.DependentColumns.Single())
                .Select(c => c.Name.LocalName)
                .ToList();
            var includedColumns = index.IncludedColumns
                .Select(c => c.Name.LocalName)
                .ToList();

            var columnsEqual = indexColumns.SequenceEqual(expectedColumnNames);
            var includedColumnsEqual = includedColumns.SequenceEqual(expectedIncludedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumns.Count);
                Assert.IsTrue(columnsEqual);
                Assert.AreEqual(2, includedColumns.Count);
                Assert.IsTrue(includedColumnsEqual);
            });
        }

        [Test]
        public void Index_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_11");
            var index = table.Index.Values.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_11");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_11").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_11").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public void Index_WhenGivenTableWithDisabledIndex_ReturnsIndexWithIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_12");
            var index = table.Index.Values.Single();

            Assert.IsFalse(index.IsEnabled);
        }

        [Test]
        public void Indexes_WhenGivenTableWithDisabledIndex_ReturnsIndexWithIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_12");
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsEnabled);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithDisabledIndex_ReturnsIndexWithIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_12").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsFalse(index.IsEnabled);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithDisabledIndex_ReturnsIndexWithIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_12").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsFalse(index.IsEnabled);
        }

        [Test]
        public void Index_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Index.Values.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = Database.GetTable("table_test_table_9");
            var index = table.Indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_9").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public void Index_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = Database.GetTable("table_test_table_13");
            var index = table.Index.Values.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public void Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = Database.GetTable("table_test_table_13");
            var index = table.Indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public async Task IndexAsync_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_13").ConfigureAwait(false);
            var indexLookup = await table.IndexAsync().ConfigureAwait(false);
            var index = indexLookup.Values.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public async Task IndexesAsync_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_13").ConfigureAwait(false);
            var indexes = await table.IndexesAsync().ConfigureAwait(false);
            var index = indexes.Single();

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public void CheckConstraint_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_1");
            var checkLookup = table.CheckConstraint;

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_1");
            var count = table.CheckConstraints.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithNoChecks_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);

            Assert.AreEqual(0, checkLookup.Count);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var count = checks.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void CheckConstraint_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.LocalName);
        }

        [Test]
        public void CheckConstraint_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraint["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void CheckConstraints_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = Database.GetTable("table_test_table_14");
            var check = table.CheckConstraints.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task CheckConstraintAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checkLookup = await table.CheckConstraintAsync().ConfigureAwait(false);
            var check = checkLookup["ck_test_table_14"];

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public async Task CheckConstraintsAsync_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await Database.GetTableAsync("table_test_table_14").ConfigureAwait(false);
            var checks = await table.CheckConstraintsAsync().ConfigureAwait(false);
            var check = checks.Single();

            Assert.AreEqual("([test_column]>(1))", check.Definition);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithNoForeignKeys_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_15");
            var parentKeyLookup = table.ParentKey;

            Assert.AreEqual(0, parentKeyLookup.Count);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_15");
            var count = table.ParentKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithNoForeignKeys_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);

            Assert.AreEqual(0, parentKeyLookup.Count);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var count = parentKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }




        // TODO
        // ParentKeys
        // - Test for delete action changes
        // - Test for update action changes
        // - IsEnabled
        // - Foreign keys to unique constraints



        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }



        // TODO
        // ParentKeys
        // - Test for delete action changes
        // - Test for update action changes
        // - IsEnabled
        // - Foreign keys to unique constraints
    }
}
