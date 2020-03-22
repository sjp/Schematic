using System.Data;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Pragma
{
    [TestFixture]
    internal static class DatabasePragmaTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => new DatabasePragma(null, "main"), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            Assert.That(() => new DatabasePragma(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            Assert.That(() => new DatabasePragma(connection, string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<ISchematicConnection>();
            Assert.That(() => new DatabasePragma(connection, "      "), Throws.ArgumentNullException);
        }

        [Test]
        public static void SchemaName_PropertyGet_MatchesCtorArg()
        {
            var dialectMock = new Mock<IDatabaseDialect>();
            dialectMock.Setup(dialect => dialect.QuoteIdentifier(It.IsAny<string>())).Returns("test");
            var connection = new SchematicConnection(Mock.Of<IDbConnection>(), dialectMock.Object);

            const string schemaName = "test";
            var dbPragma = new DatabasePragma(connection, schemaName);

            Assert.That(dbPragma.SchemaName, Is.EqualTo(schemaName));
        }
    }
}
