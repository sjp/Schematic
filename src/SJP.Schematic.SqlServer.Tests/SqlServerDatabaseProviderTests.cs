using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerDatabaseProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => new SqlServerDatabaseProvider(null), Throws.ArgumentNullException);
    }
}
