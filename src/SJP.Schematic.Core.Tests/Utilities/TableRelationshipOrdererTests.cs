using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class TableRelationshipOrdererTests
{
    [Test]
    public static void GetDeletionOrder_GivenNullTables_ThrowsArgumentNullException()
    {
        var tableOrder = new TableRelationshipOrderer();

        Assert.That(() => tableOrder.GetDeletionOrder(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetInsertionOrder_GivenNullTables_ThrowsArgumentNullException()
    {
        var tableOrder = new TableRelationshipOrderer();

        Assert.That(() => tableOrder.GetInsertionOrder(null), Throws.ArgumentNullException);
    }
}