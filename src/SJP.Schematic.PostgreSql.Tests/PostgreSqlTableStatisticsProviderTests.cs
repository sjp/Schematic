using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlTableStatisticsProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlTableStatisticsProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new PostgreSqlTableStatisticsProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTableStatistics_GivenNullTableName_ThrowsArgNullException()
    {
        var provider = new PostgreSqlTableStatisticsProvider(Mock.Of<IDbConnectionFactory>(), Mock.Of<IIdentifierDefaults>());

        Assert.That(() => provider.GetTableStatistics(null), Throws.ArgumentNullException);
    }
}
