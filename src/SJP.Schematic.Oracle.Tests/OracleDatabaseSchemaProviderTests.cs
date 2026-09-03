using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseSchemaProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new OracleDatabaseSchemaProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new OracleDatabaseSchemaProvider(connection, null), Throws.ArgumentNullException);
    }
}
