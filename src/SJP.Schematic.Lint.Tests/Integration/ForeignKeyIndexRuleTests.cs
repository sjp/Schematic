using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class ForeignKeyIndexRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table no_index_parent_table_1 ( column_1 integer not null primary key autoincrement )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_indexed_child_table_1 on indexed_child_table_1 (column_2)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table indexed_child_table_2 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index ix_indexed_child_table_2 on indexed_child_table_2 (column_2, column_1)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table not_indexed_child_table_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references no_index_parent_table_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table no_index_parent_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table indexed_child_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table indexed_child_table_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table not_indexed_child_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithTableWithIndexOnForeignKey_ProducesNoMessages()
    {
        var rule = new ForeignKeyIndexRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithTableWithoutIndexOnForeignKey_ProducesMessages()
    {
        var rule = new ForeignKeyIndexRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("no_index_parent_table_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("not_indexed_child_table_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}