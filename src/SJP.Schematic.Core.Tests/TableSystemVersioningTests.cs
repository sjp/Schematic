using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class TableSystemVersioningTests
{
    [Test]
    public static void Ctor_GivenNullHistoryTable_ThrowsArgumentNullException()
    {
        Assert.That(() => new TableSystemVersioning(null, "valid_from", "valid_to"), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullPeriodStartColumn_ThrowsArgumentNullException()
    {
        Assert.That(() => new TableSystemVersioning("test_table_history", null, "valid_to"), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullPeriodEndColumn_ThrowsArgumentNullException()
    {
        Assert.That(() => new TableSystemVersioning("test_table_history", "valid_from", null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Properties_WhenConstructed_RetainGivenValues()
    {
        var systemVersioning = new TableSystemVersioning("test_table_history", "valid_from", "valid_to");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(systemVersioning.HistoryTable.LocalName, Is.EqualTo("test_table_history"));
            Assert.That(systemVersioning.PeriodStartColumn.LocalName, Is.EqualTo("valid_from"));
            Assert.That(systemVersioning.PeriodEndColumn.LocalName, Is.EqualTo("valid_to"));
        }
    }

    [Test]
    public static void ToString_WhenInvoked_ReturnsExpectedString()
    {
        var systemVersioning = new TableSystemVersioning("test_table_history", "valid_from", "valid_to");

        Assert.That(systemVersioning.ToString(), Is.EqualTo("System versioning: history table test_table_history"));
    }
}
