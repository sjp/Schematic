using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class TableStatisticsTests
{
    [Test]
    public static void Ctor_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TableStatistics(null, Option<long>.None, false, Option<long>.None, Option<long>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void TableName_PropertyGet_EqualsCtorArg()
    {
        Identifier tableName = "test_table";
        var statistics = new TableStatistics(tableName, Option<long>.None, false, Option<long>.None, Option<long>.None);

        Assert.That(statistics.TableName, Is.EqualTo(tableName));
    }

    [Test]
    public static void RowCount_PropertyGetGivenNoneCtorArg_IsNone()
    {
        var statistics = new TableStatistics("test_table", Option<long>.None, false, Option<long>.None, Option<long>.None);

        Assert.That(statistics.RowCount, OptionIs.None);
    }

    [Test]
    public static void RowCount_PropertyGetGivenValidCtorArg_MatchesCtorArg()
    {
        const long rowCount = 1234;
        var statistics = new TableStatistics("test_table", Option<long>.Some(rowCount), false, Option<long>.None, Option<long>.None);

        Assert.That(statistics.RowCount.UnwrapSome(), Is.EqualTo(rowCount));
    }

    [Test]
    public static void IsExact_PropertyGet_EqualsCtorArg()
    {
        var statistics = new TableStatistics("test_table", Option<long>.Some(0), true, Option<long>.None, Option<long>.None);

        Assert.That(statistics.IsExact, Is.True);
    }

    [Test]
    public static void DataSizeBytes_PropertyGetGivenValidCtorArg_MatchesCtorArg()
    {
        const long dataSize = 8192;
        var statistics = new TableStatistics("test_table", Option<long>.None, false, Option<long>.Some(dataSize), Option<long>.None);

        Assert.That(statistics.DataSizeBytes.UnwrapSome(), Is.EqualTo(dataSize));
    }

    [Test]
    public static void IndexSizeBytes_PropertyGetGivenValidCtorArg_MatchesCtorArg()
    {
        const long indexSize = 16384;
        var statistics = new TableStatistics("test_table", Option<long>.None, false, Option<long>.None, Option<long>.Some(indexSize));

        Assert.That(statistics.IndexSizeBytes.UnwrapSome(), Is.EqualTo(indexSize));
    }

    [Test]
    public static void ToString_WhenRowCountPresent_ContainsTableNameAndRowCount()
    {
        var statistics = new TableStatistics("test_table", Option<long>.Some(42), false, Option<long>.None, Option<long>.None);

        Assert.That(statistics.ToString(), Is.EqualTo("Statistics: test_table, Rows: 42"));
    }

    [Test]
    public static void ToString_WhenRowCountMissing_DescribesRowCountAsUnknown()
    {
        var statistics = new TableStatistics("test_table", Option<long>.None, false, Option<long>.None, Option<long>.None);

        Assert.That(statistics.ToString(), Is.EqualTo("Statistics: test_table, Rows: unknown"));
    }
}
