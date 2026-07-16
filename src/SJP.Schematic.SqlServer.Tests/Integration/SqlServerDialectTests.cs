using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerDialectTests : SqlServerTest
{
    [Test]
    public async Task GetDatabaseDisplayVersionAsync_WhenInvoked_ReturnsNonEmptyString()
    {
        var versionStr = await DatabaseProvider.GetDatabaseDisplayVersionAsync();
        var validStr = !versionStr.IsNullOrWhiteSpace();

        Assert.That(validStr, Is.True);
    }

    [Test]
    public async Task GetDatabaseVersionAsync_WhenInvoked_ReturnsNonNullVersion()
    {
        var version = await DatabaseProvider.GetDatabaseVersionAsync();

        Assert.That(version, Is.Not.Null);
    }

    [Test]
    public async Task GetCompatibilityLevel_WhenInvoked_ReturnsNonZeroValue()
    {
        var compatibilityLevel = await DatabaseProvider.GetCompatibilityLevel();

        using (Assert.EnterMultipleScope())
        {
            // assertions are assuming that we have something at least SQL Server 2008, but not a specific version
            Assert.That(compatibilityLevel, Is.Not.Null);
            Assert.That(compatibilityLevel.Value, Is.GreaterThan(100));
            Assert.That(compatibilityLevel.SqlServerVersion, Is.GreaterThan(SqlServerCompatibilityLevel.SqlServer2008));
        }
    }
}