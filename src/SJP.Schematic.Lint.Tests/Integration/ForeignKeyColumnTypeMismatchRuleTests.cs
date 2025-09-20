using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class ForeignKeyColumnTypeMismatchRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table parent_table_with_int_key_column_1 ( column_1 integer not null primary key autoincrement )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table child_table_with_int_key_column_1 (
    column_1 integer,
    column_2 integer,
    constraint test_valid_fk foreign key (column_2) references parent_table_with_int_key_column_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table child_table_with_text_key_column_1 (
    column_1 integer,
    column_2 text,
    constraint test_valid_fk foreign key (column_2) references parent_table_with_int_key_column_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table parent_table_with_int_key_column_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table child_table_with_int_key_column_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table child_table_with_text_key_column_1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeyColumnTypeMismatchRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithMatchingTypesInForeignKeys_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("parent_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("child_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithMismatchingTypesInForeignKeys_ProducesMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("parent_table_with_int_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("child_table_with_text_key_column_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Not.Empty);
    }
}