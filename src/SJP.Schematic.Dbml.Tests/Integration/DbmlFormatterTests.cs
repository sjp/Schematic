using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Dbml.Tests.Integration;

internal sealed class DbmlFormatterTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

    private static IDbmlFormatter Formatter => new DbmlFormatter();

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create table test_table_1 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test'
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_2 (
    test_pk_1 integer not null,
    test_pk_2 integer not null,
    first_name text not null,
    middle_name text not null,
    last_name text not null,
    comment text null,
    constraint test_table_2_pk primary key (test_pk_1, test_pk_2),
    constraint test_table_2_multi_uk unique (first_name, middle_name, last_name)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_2_first_name on test_table_2 (first_name, last_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_test_table_2_comment on test_table_2 (comment)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique index ux_test_table_2_first_name_middle_name on test_table_2 (first_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique index ux_test_table_2_last_name on test_table_2 (last_name)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_3 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test'
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_4 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test',
    test_table_3_fk1 integer,
    test_table_3_fk2 integer,
    test_table_3_fk3 integer,
    test_table_3_fk4 integer,
    constraint fk_test_table_4_test_table_3_fk1 foreign key (test_table_3_fk1) references test_table_3 (test_pk),
    constraint fk_test_table_4_test_table_3_fk2 foreign key (test_table_3_fk2) references test_table_3 (test_pk) on update cascade,
    constraint fk_test_table_4_test_table_3_fk3 foreign key (test_table_3_fk3) references test_table_3 (test_pk) on delete set null,
    constraint fk_test_table_4_test_table_3_fk4 foreign key (test_table_3_fk4) references test_table_3 (test_pk) on update set null on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table test_table_5 ( test_column_1 integer )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_6 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_7 (
    test_pk integer not null primary key autoincrement,
    test_table_6_fk1 integer not null,
    constraint fk_test_table_7_test_table_6_fk1 foreign key (test_table_6_fk1) references test_table_6 (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_8 (
    test_pk integer not null primary key autoincrement,
    test_table_8_fk1 integer not null,
    constraint fk_test_table_8_test_table_6_fk1 foreign key (test_table_8_fk1) references test_table_6 (test_pk)
    constraint test_table_8_uk1 unique (test_table_8_fk1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table test_table_9 (
    test_pk integer not null primary key autoincrement,
    test_table_9_fk1 integer not null,
    constraint fk_test_table_9_test_table_6_fk1 foreign key (test_table_9_fk1) references test_table_6 (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique index ux_test_table_9_fk1 on test_table_9 (test_table_9_fk1)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_4", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_3", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_5", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_7", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_8", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_9", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_6", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task RenderTables_GivenSimpleTable_GeneratesExpectedDbml()
    {
        var table = await GetTable("test_table_5").ConfigureAwait(false);
        var tables = new[] { table };

        var result = Formatter.RenderTables(tables);

        Assert.That(result, Is.EqualTo(TestTable5Dbml).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    [Test]
    public async Task RenderTables_GivenTableWithMultipleOptions_GeneratesExpectedDbml()
    {
        var table = await GetTable("test_table_1").ConfigureAwait(false);
        var tables = new[] { table };

        var result = Formatter.RenderTables(tables);

        Assert.That(result, Is.EqualTo(TestTable1Dbml).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    [Test]
    public async Task RenderTables_GivenTableWithMultipleIndexes_GeneratesExpectedDbml()
    {
        var table = await GetTable("test_table_2").ConfigureAwait(false);
        var tables = new[] { table };

        var result = Formatter.RenderTables(tables);

        Assert.That(result, Is.EqualTo(TestTable2Dbml).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    [Test]
    public async Task RenderTables_GivenTablesWithMultipleRelationalKeys_GeneratesExpectedDbml()
    {
        var tables = new[]
        {
            await GetTable("test_table_6").ConfigureAwait(false),
            await GetTable("test_table_7").ConfigureAwait(false),
            await GetTable("test_table_8").ConfigureAwait(false),
            await GetTable("test_table_9").ConfigureAwait(false)
        };

        var result = Formatter.RenderTables(tables);

        Assert.That(result, Is.EqualTo(MultipleRelationshipsDbml).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    private const string TestTable1Dbml = @"Table main_test_table_1 {
    test_pk INTEGER [not null, increment, primary key]
    test_int INTEGER [not null]
    test_nullable_int INTEGER [null]
    test_numeric NUMERIC [not null]
    test_nullable_numeric NUMERIC [null]
    test_blob BLOB [not null]
    test_datetime NUMERIC [null, default: ""CURRENT_TIMESTAMP""]
    test_string TEXT [null]
    test_string_with_default NUMERIC [null, default: ""'test'""]
}";

    private const string TestTable2Dbml = @"Table main_test_table_2 {
    test_pk_1 INTEGER [not null]
    test_pk_2 INTEGER [not null]
    first_name TEXT [not null]
    middle_name TEXT [not null]
    last_name TEXT [not null]
    comment TEXT [null]

    Indexes {
        last_name [name: 'ux_test_table_2_last_name', unique]
        (first_name, middle_name) [name: 'ux_test_table_2_first_name_middle_name', unique]
        comment [name: 'ix_test_table_2_comment']
        (first_name, last_name) [name: 'ix_test_table_2_first_name']
    }
}";

    private const string TestTable5Dbml = @"Table main_test_table_5 {
    test_column_1 INTEGER [null]
}";

    private const string MultipleRelationshipsDbml = @"Table main_test_table_6 {
    test_pk INTEGER [not null, increment, primary key]
    test_int INTEGER [not null]
}

Table main_test_table_7 {
    test_pk INTEGER [not null, increment, primary key]
    test_table_6_fk1 INTEGER [not null]
}

Table main_test_table_8 {
    test_pk INTEGER [not null, increment, primary key]
    test_table_8_fk1 INTEGER [not null, unique key]
}

Table main_test_table_9 {
    test_pk INTEGER [not null, increment, primary key]
    test_table_9_fk1 INTEGER [not null]

    Indexes {
        test_table_9_fk1 [name: 'ux_test_table_9_fk1', unique]
    }
}

Ref: main_test_table_7.test_table_6_fk1 > main_test_table_6.test_pk
Ref: main_test_table_8.test_table_8_fk1 - main_test_table_6.test_pk
Ref: main_test_table_9.test_table_9_fk1 - main_test_table_6.test_pk";
}