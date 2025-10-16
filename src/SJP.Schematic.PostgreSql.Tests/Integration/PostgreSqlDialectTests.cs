using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDialectTests : PostgreSqlTest
{
    [Test]
    public async Task GetDatabaseDisplayVersionAsync_WhenInvoked_ReturnsNonEmptyString()
    {
        var versionStr = await Dialect.GetDatabaseDisplayVersionAsync(Connection);
        var validStr = !versionStr.IsNullOrWhiteSpace();

        Assert.That(validStr, Is.True);
    }

    [Test]
    public async Task GetDatabaseVersionAsync_WhenInvoked_ReturnsNonNullVersion()
    {
        var version = await Dialect.GetDatabaseVersionAsync(Connection);

        Assert.That(version, Is.Not.Null);
    }
}