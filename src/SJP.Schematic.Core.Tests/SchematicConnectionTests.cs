using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class SchematicConnectionTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var dialect = Mock.Of<IDatabaseDialect>();

        Assert.That(() => new SchematicConnection(null, dialect), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new SchematicConnection(dbConnection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ConnectionFactory_PropertyGet_ReturnsCtorArg()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(dbConnection, dialect);

        Assert.That(connection.ConnectionFactory, Is.EqualTo(dbConnection));
    }

    [Test]
    public static void Dialect_PropertyGet_ReturnsCtorArg()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(dbConnection, dialect);

        Assert.That(connection.Dialect, Is.EqualTo(dialect));
    }
}
