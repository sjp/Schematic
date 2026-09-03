using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteTableStatisticsProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var pragma = Mock.Of<ISqliteConnectionPragma>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteTableStatisticsProvider(null, pragma, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullConnectionPragma_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteTableStatisticsProvider(connection, null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var pragma = Mock.Of<ISqliteConnectionPragma>();

        Assert.That(() => new SqliteTableStatisticsProvider(connection, pragma, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTableStatistics_GivenNullTableName_ThrowsArgNullException()
    {
        var provider = new SqliteTableStatisticsProvider(
            Mock.Of<ISchematicConnection>(),
            Mock.Of<ISqliteConnectionPragma>(),
            Mock.Of<IIdentifierDefaults>()
        );

        Assert.That(() => provider.GetTableStatistics(null), Throws.ArgumentNullException);
    }
}
