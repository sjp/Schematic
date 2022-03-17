using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Pragma;

[TestFixture]
internal static class DatabasePragmaTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabasePragma(null, "main"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceSchemaName_ThrowsArgumentNullException(string schemaName)
    {
        var connection = Mock.Of<ISchematicConnection>();
        Assert.That(() => new DatabasePragma(connection, schemaName), Throws.ArgumentNullException);
    }

    [Test]
    public static void SchemaName_PropertyGet_MatchesCtorArg()
    {
        var dialectMock = new Mock<IDatabaseDialect>(MockBehavior.Strict);
        dialectMock.Setup(dialect => dialect.QuoteIdentifier(It.IsAny<string>())).Returns("test");
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), dialectMock.Object);

        const string schemaName = "test";
        var dbPragma = new DatabasePragma(connection, schemaName);

        Assert.That(dbPragma.SchemaName, Is.EqualTo(schemaName));
    }
}
