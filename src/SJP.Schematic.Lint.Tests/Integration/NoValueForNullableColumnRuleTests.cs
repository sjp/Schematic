using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class NoValueForNullableColumnRuleTests : SqliteTest
{
    // Mirrors the rule's private ProbeBatchSize. Kept in sync manually -- if the rule's batch size
    // changes, the query-count assertion below should be updated (and will fail loudly if it isn't).
    private const int ProbeBatchSize = 64;

    // Chosen so that the columns span exactly two 64-column probe batches, with always-null columns
    // planted at the first and last position of each -- the positions most likely to reveal an
    // off-by-one when counts are mapped back onto the columns that requested them.
    private const int WideColumnCount = 128;
    private static readonly IReadOnlyCollection<int> AlwaysNullWideColumnIndexes = [0, 63, 64, 127];

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_without_nullable_columns_1 ( column_1 integer not null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table table_for_nullable_columns_1 ( column_1 integer not null, column_2 integer null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table table_for_nullable_columns_2 ( column_1 integer not null, column_2 integer null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into table_for_nullable_columns_2 ( column_1 ) values (1)", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table table_for_nullable_columns_3 ( column_1 integer not null, column_2 integer null, column_3 integer null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into table_for_nullable_columns_3 ( column_1, column_2 ) values (1, 2)", CancellationToken.None);

        var wideColumnNames = Enumerable.Range(0, WideColumnCount).Select(static i => $"column_{i}").ToList();
        var wideColumnDefinitions = wideColumnNames.Select(static name => name + " integer null").Join(", ");
        await DbConnection.ExecuteAsync($"create table table_for_nullable_columns_wide ( {wideColumnDefinitions} )", CancellationToken.None);

        var populatedColumnIndexes = Enumerable.Range(0, WideColumnCount).Where(static i => !AlwaysNullWideColumnIndexes.Contains(i)).ToList();
        var populatedColumnNames = populatedColumnIndexes.Select(i => wideColumnNames[i]).Join(", ");
        var populatedColumnValues = populatedColumnIndexes.Select(static i => i.ToString()).Join(", ");
        await DbConnection.ExecuteAsync($"insert into table_for_nullable_columns_wide ( {populatedColumnNames} ) values ( {populatedColumnValues} )", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_without_nullable_columns_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_3", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_wide", CancellationToken.None);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        ISchematicConnection connection = null;
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new NoValueForNullableColumnRule(connection, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoValueForNullableColumnRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var rule = new NoValueForNullableColumnRule(connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutNullableColumns_ProducesNoMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_without_nullable_columns_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsButNoRows_ProducesNoMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsWithNoData_ProducesMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_2").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTableWithPopulatedAndAlwaysNullColumns_ReportsOnlyTheAlwaysNullColumn()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_3").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Count.EqualTo(1));
        Assert.That(messages.Single().Message, Does.Contain("column_3"));
    }

    [Test]
    public async Task AnalyseTables_GivenAlwaysNullColumnsAtProbeBatchBoundaries_ReportsExactlyThoseColumns()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_wide").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Has.Count.EqualTo(AlwaysNullWideColumnIndexes.Count));
        foreach (var index in AlwaysNullWideColumnIndexes)
        {
            var expectedColumnName = $"column_{index}";
            Assert.That(messages.Any(m => m.Message.Contains($"'{expectedColumnName}'")), Is.True, $"Expected a message naming '{expectedColumnName}'.");
        }
    }

    [Test]
    public async Task AnalyseTables_GivenTableWithNullableColumns_ExecutesOneQueryPerProbeBatch()
    {
        var countingFactory = new CountingDbConnectionFactory(DbConnection);
        var countingConnection = new SchematicConnection(countingFactory, Connection.Dialect);
        var rule = new NoValueForNullableColumnRule(countingConnection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_3").UnwrapSomeAsync(),
            await database.GetTable("table_for_nullable_columns_wide").UnwrapSomeAsync(),
        };

        await rule.AnalyseTables(tables);

        const int expectedQueryCount = 1 + ((WideColumnCount + ProbeBatchSize - 1) / ProbeBatchSize);
        Assert.That(countingFactory.QueryCount, Is.EqualTo(expectedQueryCount));
    }
}
