using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    public SqliteRelationalDatabaseTableProviderTests()
    {
        TableProvider = new SqliteRelationalDatabaseTableProvider(Connection, Pragma, IdentifierDefaults);
    }

    private IRelationalDatabaseTableProvider TableProvider { get; }
    private AsyncLazy<IReadOnlyCollection<IRelationalDatabaseTable>> _getAllTables;
    private Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables() => _getAllTables.Task;

    [OneTimeSetUp]
    public async Task Init()
    {
        _getAllTables = new AsyncLazy<IReadOnlyCollection<IRelationalDatabaseTable>>(() => TableProvider.GetAllTables());

        await DbConnection.ExecuteAsync("create table db_test_table_1 (id integer)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table table_test_table_1 ( test_column int )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_2 ( test_column int not null primary key )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_5 ( test_column int not null unique )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_8 ( test_column int )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check ([test_column]>(1))
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check ([test_column]>(1))
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_33 ( test_column int not null default 1 )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_34 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_34 foreign key (first_name_child) references table_test_table_35 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_35 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_35 primary key (first_name_parent),
    constraint fk_test_table_35 foreign key (last_name_parent) references table_test_table_35 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_36 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint fk_test_table_36 foreign key (last_name_parent) references table_test_table_35 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_37 (
    test_column_1 int not null,
    test_column_2 int as (test_column_1 * test_column_1),
    test_column_3 int generated always as (test_column_1 * test_column_1 * test_column_1) stored,
    test_column_4 int constraint computed_col_constraint as (test_column_1 * test_column_1 * test_column_1 * test_column_1) virtual
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_38 (
    test_column_1 int not null,
    test_column_2 int not null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_38_1 on table_test_table_38 (test_column_1)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_38_2 on table_test_table_38 (test_column_2) where test_column_2 < 100 and test_column_2 > 3", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_39 (
    test_column_1 text not null,
    test_column_2 text not null,
    constraint uk_test_table_39 unique (test_column_2)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_39_1 on table_test_table_39 (lower(test_column_1))", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_39_2 on table_test_table_39 (test_column_1 collate nocase, lower(test_column_2) desc)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create unique index ix_test_table_39_3 on table_test_table_39 (upper(test_column_1))", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_40 (
    test_column_1 text not null,
    test_column_2 int not null,
    primary key (test_column_1, test_column_2)
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table table_test_table_41 ( test_column integer not null primary key autoincrement )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_42 ( test_column integer not null primary key )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_43 ( test_column bigint not null primary key )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_44 ( test_column integer not null primary key ) without rowid", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_45 (
    test_column integer not null,
    primary key (test_column desc)
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_46 (
    declared_type_column varchar(50),
    collated_column text collate nocase,
    uncollated_column text
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table deferrable_fk_parent (
    test_column integer not null primary key
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table deferrable_fk_child (
    test_column integer,
    constraint fk_deferrable_fk_child foreign key (test_column) references deferrable_fk_parent (test_column)
        match full deferrable initially deferred
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table implicit_fk_parent_1 (
    test_column integer not null primary key
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_child_1 (
    test_column integer,
    constraint fk_implicit_fk_child_1 foreign key (test_column) references implicit_fk_parent_1
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_parent_2 (
    first_name_parent text not null,
    last_name_parent text not null,
    constraint pk_implicit_fk_parent_2 primary key (first_name_parent, last_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_child_2 (
    first_name_child text,
    last_name_child text,
    constraint fk_implicit_fk_child_2 foreign key (first_name_child, last_name_child) references implicit_fk_parent_2
        deferrable initially deferred
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_child_3 (
    first_name_child text,
    constraint fk_implicit_fk_child_3 foreign key (first_name_child) references implicit_fk_parent_2
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_parent_3 (
    test_column integer
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table implicit_fk_child_4 (
    test_column integer,
    constraint fk_implicit_fk_child_4 foreign key (test_column) references implicit_fk_parent_3
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table trigger_test_table_1 (table_id integer primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table trigger_test_table_2 (table_id integer primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_2
before update
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_3
before delete
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_4
after insert
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_5
after update
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_6
after delete
on trigger_test_table_1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_7
after update of table_id
on trigger_test_table_1
for each row
when new.table_id > 1
begin
    select 1;
end", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table db_test_table_1", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("drop table table_test_table_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_3", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_4", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_5", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_6", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_7", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_8", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_9", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_13", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_14", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_16", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_17", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_18", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_19", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_20", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_21", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_22", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_23", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_24", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_25", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_26", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_27", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_28", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_29", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_30", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_31", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_15", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_32", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_33", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_34", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_36", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_35", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_37", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_38", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_39", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_40", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_41", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_42", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_43", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_46", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_44", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table table_test_table_45", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("drop table deferrable_fk_child", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table deferrable_fk_parent", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_child_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_parent_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_child_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_child_3", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_parent_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_child_4", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table implicit_fk_parent_3", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table trigger_test_table_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("drop table trigger_test_table_2", TestContext.CurrentContext.CancellationToken);
    }

    private Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetTableAsyncCore(tableName);
    }

    private async Task<IRelationalDatabaseTable> GetTableAsyncCore(Identifier tableName)
    {
        using (await _lock.LockAsync())
        {
            if (!_tablesCache.TryGetValue(tableName, out var lazyTable))
            {
                lazyTable = new AsyncLazy<IRelationalDatabaseTable>(() => TableProvider.GetTable(tableName).UnwrapSomeAsync());
                _tablesCache[tableName] = lazyTable;
            }

            return await lazyTable;
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = [];

    [Test]
    public async Task GetTable_WhenTablePresent_ReturnsTable()
    {
        var tableIsSome = await TableProvider.GetTable("db_test_table_1").IsSome;
        Assert.That(tableIsSome, Is.True);
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
    {
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(expectedTableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("test", IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTableMissing_ReturnsNone()
    {
        var tableIsNone = await TableProvider.GetTable("table_that_doesnt_exist").IsNone;
        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("DB_TEST_table_1");
        var table = await TableProvider.GetTable(inputName).UnwrapSomeAsync();

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Main", "DB_TEST_table_1");
        var table = await TableProvider.GetTable(inputName).UnwrapSomeAsync();

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task EnumerateAllTables_WhenEnumerated_ContainsTables()
    {
        var hasTables = await TableProvider.EnumerateAllTables().AnyAsync();

        Assert.That(hasTables, Is.True);
    }

    [Test]
    public async Task EnumerateAllTables_WhenEnumerated_ContainsTestTable()
    {
        var containsTestTable = await TableProvider.EnumerateAllTables()
            .AnyAsync(t => string.Equals(t.Name.LocalName, "db_test_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetAllTables_WhenRetrieved_ContainsTables()
    {
        var tables = await GetAllTables();

        Assert.That(tables, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllTables_WhenRetrieved_ContainsTestTable()
    {
        var tables = await GetAllTables();
        var containsTestTable = tables.Any(t => string.Equals(t.Name.LocalName, "db_test_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }
}