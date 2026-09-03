using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDatabaseSchemaProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlDatabaseSchemaProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new PostgreSqlDatabaseSchemaProvider(connection, null), Throws.ArgumentNullException);
    }
}
