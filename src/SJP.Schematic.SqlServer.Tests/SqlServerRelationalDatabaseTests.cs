using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
internal static class SqlServerRelationalDatabaseTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqlServerRelationalDatabase(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();

        Assert.That(() => new SqlServerRelationalDatabase(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(() => database.GetTable(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(() => database.GetView(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(() => database.GetSequence(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(() => database.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(() => database.GetRoutine(null), Throws.ArgumentNullException);
    }
}
