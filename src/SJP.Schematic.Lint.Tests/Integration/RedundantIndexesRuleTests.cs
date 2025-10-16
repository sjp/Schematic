using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class RedundantIndexesRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table valid_table_1 ( column_1 integer )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table valid_table_2 ( column_1 integer, column_2 integer, column_3 integer )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index ix_valid_table_1 on valid_table_2 ( column_2, column_3 )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table valid_table_3 ( column_1 integer, column_2 integer, column_3 integer )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index ix_valid_table_3_1 on valid_table_3 ( column_2 )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index ix_valid_table_3_2 on valid_table_3 ( column_2, column_3 )", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table valid_table_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table valid_table_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table valid_table_3", CancellationToken.None);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new RedundantIndexesRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutIndexes_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("valid_table_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("valid_table_2").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithRedundantIndexes_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("valid_table_3").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }
}