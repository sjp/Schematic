using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDatabaseSchemaProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnectionPragma_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteDatabaseSchemaProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connectionPragma = Mock.Of<ISqliteConnectionPragma>();

        Assert.That(() => new SqliteDatabaseSchemaProvider(connectionPragma, null), Throws.ArgumentNullException);
    }
}
