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

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    private IRelationalDatabaseTableProvider TableProvider => new OracleRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver);
    private AsyncLazy<List<IRelationalDatabaseTable>> _tables;
    private Task<List<IRelationalDatabaseTable>> GetAllTables() => _tables.Task;

    [OneTimeSetUp]
    public async Task Init()
    {
        _tables = new AsyncLazy<List<IRelationalDatabaseTable>>(() => TableProvider.GetAllTables().ToListAsync().AsTask());

        await DbConnection.ExecuteAsync("create table db_test_table_1 ( title varchar2(200) )", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create table table_test_table_1 ( test_column number )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_2 ( test_column number not null primary key )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column number,
    constraint pk_test_table_3 primary key (test_column)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_5 ( test_column number not null unique )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column number,
    constraint uk_test_table_6 unique (test_column)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_8 (test_column number)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column number not null,
    constraint ck_test_table_14 check (test_column > 1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent varchar2(50),
    middle_name_parent varchar2(50),
    last_name_parent varchar2(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child varchar2(50),
    middle_name varchar2(50),
    last_name varchar2(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("alter table table_test_table_30 disable constraint fk_test_table_30", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child varchar2(50),
    middle_name_child varchar2(50),
    last_name_child varchar2(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("alter table table_test_table_31 disable constraint fk_test_table_31", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column number not null,
    constraint ck_test_table_32 check (test_column > 1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("alter table table_test_table_32 disable constraint ck_test_table_32", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_33 ( test_column number default 1 not null )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create table table_test_table_34 (
    test_column_1 number,
    test_column_2 number,
    test_column_3 as (test_column_1 + test_column_2)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_test_table_35 ( test_column number primary key )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table trigger_test_table_1 (table_id number primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table trigger_test_table_2 (table_id number primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
before insert on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
before update on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
before delete on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
after insert on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
after update on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
after delete on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_7
after insert or update or delete on trigger_test_table_1
for each row
begin
    null;
end;
", CancellationToken.None).ConfigureAwait(false);
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
        await DbConnection.ExecuteAsync("drop table table_test_table_24", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_25", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_27", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_28", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_30", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_31", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_15", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_32", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_33", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_34", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_test_table_35", CancellationToken.None).ConfigureAwait(false);
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
    private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTable>> _tablesCache = new();

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
        const string expectedTableName = "DB_TEST_TABLE_1";
        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name.LocalName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

        var table = await TableProvider.GetTable(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(table.Name, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_TABLE_1");

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
        var tables = await GetAllTables().ConfigureAwait(false);

        Assert.That(tables, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllTables_WhenEnumerated_ContainsTestTable()
    {
        const string expectedTableName = "DB_TEST_TABLE_1";
        var tables = await GetAllTables().ConfigureAwait(false);
        var containsTestTable = tables.Any(t => string.Equals(t.Name.LocalName, expectedTableName, StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }
}