using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyTableStatisticsProviderTests
{
    [Test]
    public static void GetTableStatistics_GivenNullTableName_ThrowsArgumentNullException()
    {
        var provider = new EmptyTableStatisticsProvider();

        Assert.That(() => provider.GetTableStatistics(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetTableStatistics_WhenRetrieved_IsNone()
    {
        var provider = new EmptyTableStatisticsProvider();
        var statistics = await provider.GetTableStatistics("test_table").ToOption();

        Assert.That(statistics, OptionIs.None);
    }

    [Test]
    public static async Task GetAllTableStatistics_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyTableStatisticsProvider();
        var statistics = await provider.GetAllTableStatistics();

        Assert.That(statistics, Is.Empty);
    }
}
