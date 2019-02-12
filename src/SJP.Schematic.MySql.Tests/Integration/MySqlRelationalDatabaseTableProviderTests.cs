using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
    {
        private IRelationalDatabaseTableProvider TableProvider => new MySqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table db_test_table_1 ( title nvarchar(200) )").ConfigureAwait(false);

            await Connection.ExecuteAsync("create table if not exists table_test_table_1 ( test_column int )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists table_test_table_2 ( test_column int not null primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_4 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists table_test_table_5 ( test_column int not null unique )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_7 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_8 (
    test_column int,
    index ix_test_table_8 (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_9 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    index ix_test_table_9 (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_10 (
    test_column int,
    test_column_2 int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_10 on table_test_table_10 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_11 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_11 on table_test_table_11 (first_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_13 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_15 (
    first_name_parent varchar(50),
    middle_name_parent varchar(50),
    last_name_parent varchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_16 (
    first_name_child varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_17 (
    first_name varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_18 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_19 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_21 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_22 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_24 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_25 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_27 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table if not exists table_test_table_28 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists table_test_table_33 ( test_column int not null default 1 )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table if not exists table_test_table_34 (
    test_column_1 int,
    test_column_2 int,
    test_column_3 int as (test_column_1 + test_column_2)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists table_test_table_35 ( test_column int not null primary key auto_increment )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists trigger_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table if not exists trigger_test_table_2 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
before update
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
before delete
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
after insert
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
after update
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
after delete
on trigger_test_table_1
for each row
begin
    insert into table_test_table_1 (test_column) values (1);
end
").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop table if exists table_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_11").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_12").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_13").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_16").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_17").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_18").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_19").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_21").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_22").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_24").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_25").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_27").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_28").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_15").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_33").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_34").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists table_test_table_35").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists trigger_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table if exists trigger_test_table_2").ConfigureAwait(false);
        }

        private Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            lock (_lock)
            {
                if (!_tablesCache.TryGetValue(tableName, out var lazyTable))
                {
                    lazyTable = new AsyncLazy<IRelationalDatabaseTable>(() => TableProvider.GetTable(tableName).UnwrapSomeAsync());
                    _tablesCache[tableName] = lazyTable;
                }

                return lazyTable.Task;
            }
        }

        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = new ConcurrentDictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>>();

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
            var tableName = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

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
            var tables = await TableProvider.GetAllTables().ConfigureAwait(false);

            Assert.NotZero(tables.Count);
        }

        [Test]
        public async Task GetAllTables_WhenEnumerated_ContainsTestTable()
        {
            var tables = await TableProvider.GetAllTables().ConfigureAwait(false);
            var containsTestTable = tables.Any(t => t.Name.LocalName == "db_test_table_1");

            Assert.IsTrue(containsTestTable);
        }
    }
}