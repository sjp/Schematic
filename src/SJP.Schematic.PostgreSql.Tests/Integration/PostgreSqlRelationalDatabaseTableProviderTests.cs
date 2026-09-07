using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
{
    private IRelationalDatabaseTableProvider TableProvider => new PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver);
    private AsyncLazy<IReadOnlyCollection<IRelationalDatabaseTable>> _getAllTables;
    private Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables() => _getAllTables.Task;

    [OneTimeSetUp]
    public async Task Init()
    {
        _getAllTables = new AsyncLazy<IReadOnlyCollection<IRelationalDatabaseTable>>(() => TableProvider.GetAllTables());

        await DbConnection.ExecuteAsync("create table db_test_table_1 ( title varchar(200) )", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table table_test_table_1 ( test_column int )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_2 ( test_column int not null primary key )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
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
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_8 (
    test_column int
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_10 (
    test_column int,
    test_column_2 int
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_10 on table_test_table_10 (test_column)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_11 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_11 on table_test_table_11 (first_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_12 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_12 on table_test_table_12 (first_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check (test_column > 1)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent varchar(50),
    middle_name_parent varchar(50),
    last_name_parent varchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_15 on table_test_table_15 (last_name_parent) include (first_name_parent)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_16 on table_test_table_16 (last_name) include (middle_name, first_name_child)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check (test_column > 1)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_33 ( test_column int not null default 1 )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_35 ( test_column serial primary key )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_36 ( test_column int generated always as identity (start with 123 increment by 456) )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_37 (
    test_column_1 int,
    test_column_2 int generated always as (test_column_1 * 2) stored
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_38 ( test_column int not null )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_table_38 on table_test_table_38 (test_column) where test_column > 100", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_39 (
    json_column json,
    jsonb_column jsonb
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_40 ( xml_column xml )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_41 (
    point_column point,
    line_column line,
    lseg_column lseg,
    box_column box,
    path_column path,
    polygon_column polygon,
    circle_column circle
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_42 ( uuid_column uuid )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_test_table_43 ( test_column int generated by default as identity (start with 5 increment by 2 minvalue 1 maxvalue 900 cycle) )", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table table_test_partitioned_1 (
    part_key int not null,
    payload varchar(50),
    constraint pk_test_partitioned_1 primary key (part_key)
) partition by range (part_key)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index ix_test_partitioned_1 on table_test_partitioned_1 (payload)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table table_test_partitioned_1_p1
    partition of table_test_partitioned_1 for values from (0) to (100)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("comment on table table_test_partitioned_1 is 'test partitioned table comment'", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("comment on column table_test_partitioned_1.payload is 'test partitioned column comment'", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create unlogged table table_test_unlogged_1 ( test_column int )", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table constraint_state_parent (
    a int not null,
    b int not null,
    constraint pk_constraint_state_parent primary key (a, b) deferrable initially deferred
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table constraint_state_fk_parent (
    a int not null,
    b int not null,
    constraint pk_constraint_state_fk_parent primary key (a, b)
)", TestContext.CurrentContext.CancellationToken);
        // NOT VALID is only honoured by ALTER TABLE ADD CONSTRAINT; constraints declared
        // inline in CREATE TABLE are always recorded as validated.
        await DbConnection.ExecuteAsync(@"
create table constraint_state_child (
    a int,
    b int
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
alter table constraint_state_child
    add constraint ck_constraint_state_child check (a > 0) not valid", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
alter table constraint_state_child
    add constraint fk_constraint_state_child foreign key (a, b) references constraint_state_fk_parent (a, b)
        match full deferrable initially immediate not valid", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync(@"
create table fk_bare_unique_parent (
    a int
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create unique index ux_fk_bare_unique_parent on fk_bare_unique_parent (a)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create table fk_bare_unique_child (
    a int references fk_bare_unique_parent (a)
)", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table trigger_test_table_1 (table_id int primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table trigger_test_table_2 (table_id int primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create function test_trigger_fn()
returns trigger as
$BODY$
BEGIN
    RETURN null;
END;
$BODY$
LANGUAGE PLPGSQL", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
before update
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
before delete
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
after insert
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
after update
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
after delete
on trigger_test_table_1
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_7
after update of table_id
on trigger_test_table_1
for each row
when (new.table_id > 1)
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_8
after truncate
on trigger_test_table_1
for each statement
execute procedure test_trigger_fn()", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop table db_test_table_1",
        "drop table table_test_table_1",
        "drop table table_test_table_2",
        "drop table table_test_table_3",
        "drop table table_test_table_4",
        "drop table table_test_table_5",
        "drop table table_test_table_6",
        "drop table table_test_table_7",
        "drop table table_test_table_8",
        "drop table table_test_table_9",
        "drop table table_test_table_10",
        "drop table table_test_table_11",
        "drop table table_test_table_12",
        "drop table table_test_table_13",
        "drop table table_test_table_14",
        "drop table table_test_table_16",
        "drop table table_test_table_17",
        "drop table table_test_table_18",
        "drop table table_test_table_19",
        "drop table table_test_table_20",
        "drop table table_test_table_21",
        "drop table table_test_table_22",
        "drop table table_test_table_23",
        "drop table table_test_table_24",
        "drop table table_test_table_25",
        "drop table table_test_table_26",
        "drop table table_test_table_27",
        "drop table table_test_table_28",
        "drop table table_test_table_29",
        "drop table table_test_table_30",
        "drop table table_test_table_31",
        "drop table table_test_table_15",
        "drop table table_test_table_32",
        "drop table table_test_table_33",
        "drop table table_test_table_35",
        "drop table table_test_table_36",
        "drop table table_test_table_37",
        "drop table table_test_table_38",
        "drop table table_test_table_39",
        "drop table table_test_table_40",
        "drop table table_test_table_41",
        "drop table table_test_table_42",
        "drop table table_test_table_43",
        "drop table table_test_partitioned_1",
        "drop table table_test_unlogged_1",
        "drop table constraint_state_child",
        "drop table constraint_state_fk_parent",
        "drop table constraint_state_parent",
        "drop table fk_bare_unique_child",
        "drop table fk_bare_unique_parent",
        "drop table trigger_test_table_1",
        "drop table trigger_test_table_2",
        "drop function test_trigger_fn()"
    );

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
    public async Task GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
    {
        const string tableName = "db_test_table_1";
        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name.LocalName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync();

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");

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

    [Test]
    public async Task GetTable_WhenGivenPartitionedTable_ReturnsTableWithColumns()
    {
        var table = await GetTableAsync("table_test_partitioned_1");

        Assert.That(table.Columns, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetTable_WhenGivenPartitionedTable_ReturnsTableWithIndex()
    {
        var table = await GetTableAsync("table_test_partitioned_1");

        var containsIndex = table.Indexes.Any(i => string.Equals(i.Name.LocalName, "ix_test_partitioned_1", StringComparison.Ordinal));

        Assert.That(containsIndex, Is.True);
    }

    [Test]
    public async Task GetTable_WhenGivenPartitionedTable_ReturnsTableWithPrimaryKey()
    {
        var table = await GetTableAsync("table_test_partitioned_1");

        var primaryKeyIsSome = table.PrimaryKey.IsSome;

        Assert.That(primaryKeyIsSome, Is.True);
    }

    [Test]
    public async Task GetTable_WhenGivenPartitionedTable_ReturnsPartitioningDescribingItsPartitions()
    {
        var table = await GetTableAsync("table_test_partitioned_1");

        var partitioning = table.Partitioning.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.PartitionParent));
            Assert.That(partitioning.Strategy, Is.EqualTo("RANGE"));
            Assert.That(partitioning.Columns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "part_key" }));
            Assert.That(partitioning.Partitions.Select(p => p.LocalName), Is.EqualTo(new[] { "table_test_partitioned_1_p1" }));
        }
    }

    [Test]
    public async Task GetTable_WhenGivenIndividualPartition_ReturnsTableWithPartitionKind()
    {
        var table = await GetTableAsync("table_test_partitioned_1_p1");

        Assert.That(table.Kind, Is.EqualTo(TableKind.Partition));
    }

    [Test]
    public async Task GetAllTables_WhenRetrieved_ContainsPartitionedParentAndItsPartition()
    {
        var tables = await GetAllTables();

        var containsParent = tables.Any(t => string.Equals(t.Name.LocalName, "table_test_partitioned_1", StringComparison.Ordinal));
        var containsPartition = tables.Any(t => string.Equals(t.Name.LocalName, "table_test_partitioned_1_p1", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(containsParent, Is.True);
            Assert.That(containsPartition, Is.True);
        }
    }

    [Test]
    public async Task GetTable_WhenGivenUnloggedTable_ReturnsTableThatIsNotLogged()
    {
        var table = await GetTableAsync("table_test_unlogged_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.IsLogged, Is.False);
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
        }
    }

    [Test]
    public async Task GetTable_WhenGivenOrdinaryTable_ReturnsLoggedRegularTable()
    {
        var table = await GetTableAsync("table_test_table_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
            Assert.That(table.IsLogged, Is.True);
            Assert.That(table.Partitioning, OptionIs.None);
            Assert.That(table.SystemVersioning, OptionIs.None);
            Assert.That(table.Collation, OptionIs.None);
        }
    }

    [Test]
    public async Task GetTable_WhenGivenForeignKeyReferencingBareUniqueIndex_ReturnsParentKeyFromIndex()
    {
        var table = await GetTableAsync("fk_bare_unique_child");

        Assert.That(table.ParentKeys, Has.Exactly(1).Items);

        var parentKey = table.ParentKeys.Single().ParentKey;
        Assert.Multiple(() =>
        {
            Assert.That(parentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            Assert.That(parentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_fk_bare_unique_parent"));
        });
    }

    [Test]
    public async Task GetTable_WhenGivenTableWithChildReferencingBareUniqueIndex_ReturnsChildKeyFromIndex()
    {
        var table = await GetTableAsync("fk_bare_unique_parent");

        Assert.That(table.ChildKeys, Has.Exactly(1).Items);

        var childKey = table.ChildKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(childKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            Assert.That(childKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_fk_bare_unique_parent"));
        });
    }
}