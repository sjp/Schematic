using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseUserDefinedTypeProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new OracleDatabaseUserDefinedTypeProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new OracleDatabaseUserDefinedTypeProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetUserDefinedType_GivenNullTypeName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var typeProvider = new OracleDatabaseUserDefinedTypeProvider(connection, identifierDefaults);

        Assert.That(() => typeProvider.GetUserDefinedType(null), Throws.ArgumentNullException);
    }
}
