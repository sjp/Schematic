using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyRelationalDatabaseTableProviderTests
{
    [Test]
    public static void GetTable_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseTableProvider();
        Assert.That(() => provider.GetTable(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetTable_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseTableProvider();
        var table = provider.GetTable("table_name");
        var tableIsNone = await table.IsNone.ConfigureAwait(false);

        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllTables_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyRelationalDatabaseTableProvider();
        var hasTables = await provider.GetAllTables().AnyAsync().ConfigureAwait(false);

        Assert.That(hasTables, Is.False);
    }
}