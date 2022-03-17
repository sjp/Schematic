using System;
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
    public static void Ctor_GivenEmptyIdentifier_ThrowsArgumentException()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        Assert.That(() => new SchematicConnection(Guid.Empty, dbConnection, dialect), Throws.ArgumentException);
    }

    [Test]
    public static void ConnectionId_PropertyGet_ReturnsCtorArg()
    {
        var identifier = Guid.NewGuid();
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(identifier, dbConnection, dialect);

        Assert.That(connection.ConnectionId, Is.EqualTo(identifier));
    }

    [Test]
    public static void ConnectionId_PropertyGetWhenNoCtorArgProvided_IsNotEmpty()
    {
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(dbConnection, dialect);

        Assert.That(connection.ConnectionId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public static void DbConnection_PropertyGet_ReturnsCtorArg()
    {
        var identifier = Guid.NewGuid();
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(identifier, dbConnection, dialect);

        Assert.That(connection.DbConnection, Is.EqualTo(dbConnection));
    }

    [Test]
    public static void Dialect_PropertyGet_ReturnsCtorArg()
    {
        var identifier = Guid.NewGuid();
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = Mock.Of<IDatabaseDialect>();

        var connection = new SchematicConnection(identifier, dbConnection, dialect);

        Assert.That(connection.Dialect, Is.EqualTo(dialect));
    }
}
