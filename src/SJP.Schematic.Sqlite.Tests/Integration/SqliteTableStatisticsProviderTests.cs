using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed class SqliteTableStatisticsProviderTests : SqliteTest
{
    private SqliteTableStatisticsProvider StatisticsProvider => new(Connection, Pragma, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_with_statistics ( column_1 integer not null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index ix_table_with_statistics on table_with_statistics ( column_1 )", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into table_with_statistics ( column_1 ) values (1), (2), (3)", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table table_without_statistics ( column_1 integer not null )", CancellationToken.None);

        // ANALYZE is what creates sqlite_stat1; table_without_statistics is created afterwards so
        // that it has no entry in it
        await DbConnection.ExecuteAsync("analyze table_with_statistics", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_with_statistics", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_without_statistics", CancellationToken.None);
    }

    [Test]
    public async Task GetTableStatistics_GivenAnalysedTable_ReturnsTheRecordedRowCount()
    {
        var statistics = await StatisticsProvider.GetTableStatistics("table_with_statistics").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(statistics.TableName.LocalName, Is.EqualTo("table_with_statistics"));
            Assert.That(statistics.RowCount.UnwrapSome(), Is.EqualTo(3));
            Assert.That(statistics.IsExact, Is.False);
        }
    }

    [Test]
    public async Task GetTableStatistics_GivenAnalysedTable_ReportsNoSizes()
    {
        var statistics = await StatisticsProvider.GetTableStatistics("table_with_statistics").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(statistics.DataSizeBytes, OptionIs.None);
            Assert.That(statistics.IndexSizeBytes, OptionIs.None);
        }
    }

    [Test]
    public async Task GetTableStatistics_GivenTableWithNoRecordedStatistics_ReturnsNone()
    {
        var statistics = await StatisticsProvider.GetTableStatistics("table_without_statistics").ToOption();

        Assert.That(statistics, OptionIs.None);
    }

    [Test]
    public async Task GetTableStatistics_GivenUnknownTable_ReturnsNone()
    {
        var statistics = await StatisticsProvider.GetTableStatistics("table_that_does_not_exist").ToOption();

        Assert.That(statistics, OptionIs.None);
    }

    [Test]
    public async Task GetAllTableStatistics_WhenInvoked_ContainsTheAnalysedTableOnly()
    {
        var statistics = await StatisticsProvider.GetAllTableStatistics();
        var tableNames = statistics.Select(s => s.TableName.LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableNames, Does.Contain("table_with_statistics"));
            Assert.That(tableNames, Does.Not.Contain("table_without_statistics"));
            Assert.That(tableNames.Where(n => n.StartsWith("sqlite_", System.StringComparison.OrdinalIgnoreCase)), Is.Empty);
        }
    }
}
