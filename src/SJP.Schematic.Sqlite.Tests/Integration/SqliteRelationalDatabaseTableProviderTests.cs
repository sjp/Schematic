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

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    public SqliteRelationalDatabaseTableProviderTests()
    {
        TableProvider = new SqliteRelationalDatabaseTableProvider(Connection, Pragma, IdentifierDefaults);
    }

    private IRelationalDatabaseTableProvider TableProvider { get; }

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table db_test_table_1 (id integer)", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create table table_test_table_1 ( test_column int )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_2 ( test_column int not null primary key )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_5 ( test_column int not null unique )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_8 ( test_column int )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check ([test_column]>(1))
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check ([test_column]>(1))
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_33 ( test_column int not null default 1 )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_34 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_34 foreign key (first_name_child) references table_test_table_35 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_35 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_35 primary key (first_name_parent),
    constraint fk_test_table_35 foreign key (last_name_parent) references table_test_table_35 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_36 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint fk_test_table_36 foreign key (last_name_parent) references table_test_table_35 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_37 (
    test_column_1 int not null,
    test_column_2 int as (test_column_1 * test_column_1),
    test_column_3 int generated always as (test_column_1 * test_column_1 * test_column_1) stored,
    test_column_4 int constraint computed_col_constraint as (test_column_1 * test_column_1 * test_column_1 * test_column_1) virtual
)", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync(@"
create table table_test_table_38 (
    test_column_1 int not null,
    test_column_2 int not null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_38_1 on table_test_table_38 (test_column_1)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_38_2 on table_test_table_38 (test_column_2) where test_column_2 < 100 and test_column_2 > 3", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create table trigger_test_table_1 (table_id integer primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table trigger_test_table_2 (table_id integer primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_2
before update
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_3
before delete
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_4
after insert
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_5
after update
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create trigger trigger_test_table_1_trigger_6
after delete
on trigger_test_table_1
begin
    select 1;
end", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table db_test_table_1", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("drop table table_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_3", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_4", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_5", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_6", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_7", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_8", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_9", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_13", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_14", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_16", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_17", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_18", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_19", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_20", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_21", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_22", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_23", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_24", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_25", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_26", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_27", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_28", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_29", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_30", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_31", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_15", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_32", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_33", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_34", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_36", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_35", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_37", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_38", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table trigger_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table trigger_test_table_2", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

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
    private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = [];

    [Test]
    public async Task GetTable_WhenTablePresent_ReturnsTable()
    {
        var tableIsSome = await TableProvider.GetTable("db_test_table_1").IsSome.ConfigureAwait(false);
        Assert.That(tableIsSome, Is.True);
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
    {
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

        var table = await TableProvider.GetTable(expectedTableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("test", IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");

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
    public async Task GetTable_WhenTablePresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("DB_TEST_table_1");
        var table = await TableProvider.GetTable(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Main", "DB_TEST_table_1");
        var table = await TableProvider.GetTable(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, table.Name);
        Assert.That(equalNames, Is.True);
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
            .AnyAsync(t => string.Equals(t.Name.LocalName, "db_test_table_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetAllTables2_WhenRetrieved_ContainsTables()
    {
        var tables = await TableProvider.GetAllTables2().ConfigureAwait(false);

        Assert.That(tables, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllTables2_WhenRetrieved_ContainsTestTable()
    {
        var tables = await TableProvider.GetAllTables2().ConfigureAwait(false);
        var containsTestTable = tables.Any(t => string.Equals(t.Name.LocalName, "db_test_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }
}