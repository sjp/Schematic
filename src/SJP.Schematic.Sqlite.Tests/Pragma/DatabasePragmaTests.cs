using System;
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
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new DatabasePragma(null, connection, "main"));
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            Assert.Throws<ArgumentNullException>(() => new DatabasePragma(dialect, null, "main"));
        }

        [Test]
        public static void Ctor_GivenNullSchemaName_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new DatabasePragma(dialect, connection, null));
        }

        [Test]
        public static void Ctor_GivenEmptySchemaName_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new DatabasePragma(dialect, connection, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSchemaName_ThrowsArgumentNullException()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new DatabasePragma(dialect, connection, "      "));
        }

        [Test]
        public static void SchemaName_PropertyGet_MatchesCtorArg()
        {
            var dialect = Mock.Of<IDatabaseDialect>();
            var connection = Mock.Of<IDbConnection>();

            const string schemaName = "test";
            var dbPragma = new DatabasePragma(dialect, connection, schemaName);

            Assert.AreEqual(schemaName, dbPragma.SchemaName);
        }
    }
}
