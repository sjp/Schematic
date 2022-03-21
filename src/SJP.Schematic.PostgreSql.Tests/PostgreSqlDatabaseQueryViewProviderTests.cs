using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDatabaseQueryViewProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new PostgreSqlDatabaseQueryViewProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new PostgreSqlDatabaseQueryViewProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlDatabaseQueryViewProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetView_GivenNullViewName_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        var viewProvider = new PostgreSqlDatabaseQueryViewProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => viewProvider.GetView(null), Throws.ArgumentNullException);
    }
}