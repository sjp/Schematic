using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V13
{
    internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql13Test
    {
        private IRelationalDatabaseTableProvider TableProvider => new PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync("create table v13_db_test_table_1 ( title varchar(200) )", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("create table v13_table_test_table_1 ( test_column int )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_table_test_table_2 ( test_column int not null primary key )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_4 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_table_test_table_5 ( test_column int not null unique )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_7 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_8 (
    test_column int
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_8 on v13_table_test_table_8 (test_column)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_9 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_9 on v13_table_test_table_9 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_10 (
    test_column int,
    test_column_2 int
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_10 on v13_table_test_table_10 (test_column)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_11 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_11 on v13_table_test_table_11 (first_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_12 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_12 on v13_table_test_table_12 (first_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_13 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create unique index ix_test_table_13 on v13_table_test_table_13 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check (test_column > 1)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_15 (
    first_name_parent varchar(50),
    middle_name_parent varchar(50),
    last_name_parent varchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_15 on v13_table_test_table_15 (last_name_parent) include (first_name_parent)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_16 (
    first_name_child varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_16 on v13_table_test_table_16 (last_name) include (middle_name, first_name_child)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_17 (
    first_name varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_18 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on update cascade
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_19 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on update set null
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_20 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on update set default
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_21 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_22 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_23 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_24 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_25 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_26 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent) on delete set default
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_27 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_28 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_29 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_30 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references v13_table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_31 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references v13_table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check (test_column > 1)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_table_test_table_33 ( test_column int not null default 1 )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_table_test_table_35 ( test_column serial primary key )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_table_test_table_36 ( test_column int generated always as identity (start with 123 increment by 456) )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table v13_table_test_table_37 (
    test_column_1 int,
    test_column_2 int generated always as (test_column_1 * 2) stored
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_trigger_test_table_1 (table_id int primary key not null)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table v13_trigger_test_table_2 (table_id int primary key not null)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"create function v13_test_trigger_fn()
returns trigger as
$BODY$
BEGIN
    RETURN null;
END;
$BODY$
LANGUAGE PLPGSQL", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_1
before insert
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_2
before update
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_3
before delete
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_4
after insert
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_5
after update
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create trigger v13_trigger_test_table_1_trigger_6
after delete
on v13_trigger_test_table_1
execute procedure v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop table v13_db_test_table_1", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("drop table v13_table_test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_3", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_4", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_5", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_6", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_7", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_8", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_9", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_10", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_11", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_12", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_13", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_14", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_16", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_17", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_18", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_19", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_20", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_21", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_22", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_23", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_24", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_25", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_26", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_27", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_28", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_29", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_30", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_31", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_15", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_32", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_33", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_35", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_36", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_table_test_table_37", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_trigger_test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table v13_trigger_test_table_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop function v13_test_trigger_fn()", CancellationToken.None).ConfigureAwait(false);
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

                return await lazyTable.ConfigureAwait(false);
            }
        }

        private readonly AsyncLock _lock = new();
        private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = new();

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableProvider.GetTable("v13_db_test_table_1").IsSome.ConfigureAwait(false);
            Assert.That(tableIsSome, Is.True);
        }

        [Test]
        public async Task GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "v13_db_test_table_1";
            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name.LocalName, Is.EqualTo(tableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("v13_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "v13_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(tableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

            var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(table.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "v13_db_test_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v13_db_test_table_1");

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
                .AnyAsync(t => string.Equals(t.Name.LocalName, "v13_db_test_table_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestTable, Is.True);
        }
    }
}