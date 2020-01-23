using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_5
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql95Test
    {
        private IRelationalDatabaseTableProvider TableProvider => new PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver, Dialect.TypeProvider);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table v95_db_test_table_1 ( title varchar(200) )").ConfigureAwait(false);

            await Connection.ExecuteAsync("create table v95_table_test_table_1 ( test_column int )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_table_test_table_2 ( test_column int not null primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_4 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_table_test_table_5 ( test_column int not null unique )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_7 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_8 (
    test_column int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_8 on v95_table_test_table_8 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_9 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_9 on v95_table_test_table_9 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_10 (
    test_column int,
    test_column_2 int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_10 on v95_table_test_table_10 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_11 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_11 on v95_table_test_table_11 (first_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_12 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_12 on v95_table_test_table_12 (first_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_13 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on v95_table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_15 (
    first_name_parent varchar(50),
    middle_name_parent varchar(50),
    last_name_parent varchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_16 (
    first_name_child varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_17 (
    first_name varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_18 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_19 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_20 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_21 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_22 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_23 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_24 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_25 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_26 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_27 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_28 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_29 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_30 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references v95_table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_31 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references v95_table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table v95_table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_table_test_table_33 ( test_column int not null default 1 )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_table_test_table_35 ( test_column serial primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_trigger_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table v95_trigger_test_table_2 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create function v95_test_trigger_fn()
returns trigger as
$BODY$
BEGIN
    RETURN null;
END;
$BODY$
LANGUAGE PLPGSQL").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_1
before insert
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_2
before update
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_3
before delete
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_4
after insert
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_5
after update
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger v95_trigger_test_table_1_trigger_6
after delete
on v95_trigger_test_table_1
execute procedure v95_test_trigger_fn()").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table v95_db_test_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop table v95_table_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_11").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_12").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_13").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_14").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_16").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_17").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_18").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_19").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_20").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_21").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_22").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_23").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_24").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_25").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_26").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_27").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_28").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_29").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_30").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_31").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_15").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_32").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_33").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_table_test_table_35").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_trigger_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table v95_trigger_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop function v95_test_trigger_fn()").ConfigureAwait(false);
        }

        private Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetTableAsyncCore(tableName);
        }

        private async Task<IRelationalDatabaseTable> GetTableAsyncCore(Identifier tableName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_tablesCache.TryGetValue(tableName, out var lazyTable))
                {
                    lazyTable = new AsyncLazy<IRelationalDatabaseTable>(() => TableProvider.GetTable(tableName).UnwrapSomeAsync());
                    _tablesCache[tableName] = lazyTable;
                }

                return await lazyTable;
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = new Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>>();

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableProvider.GetTable("v95_db_test_table_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "v95_db_test_table_1";
            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, table.Name.LocalName);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("v95_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "v95_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "v95_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_db_test_table_1");

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
                .AnyAsync(t => t.Name.LocalName == "v95_db_test_table_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestTable);
        }
    }
}