using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDatabaseSchemaProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new MySqlDatabaseSchemaProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();

        Assert.That(() => new MySqlDatabaseSchemaProvider(connection, null), Throws.ArgumentNullException);
    }
}
