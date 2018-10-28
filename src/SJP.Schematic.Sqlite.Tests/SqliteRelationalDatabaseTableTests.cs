using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseTableTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var dbMock = new Mock<IRelationalDatabase>();
            var dialectMock = new Mock<IDatabaseDialect>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            dialectMock.SetupGet(d => d.TypeProvider).Returns(typeProvider);
            var dialect = dialectMock.Object;
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTable(null, database, new Identifier("main", "test")));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTable(connection, null, new Identifier("main", "test")));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseTable(connection, database, null));
        }

        [Test]
        public static void Ctor_GivenNullSchemaName_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentException>(() => new SqliteRelationalDatabaseTable(connection, database, new Identifier("main", "test_table")));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialectMock = new Mock<IDatabaseDialect>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            dialectMock.SetupGet(d => d.TypeProvider).Returns(typeProvider);
            var dialect = dialectMock.Object;
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var tableName = new Identifier("main", "table_test_table_1");

            var table = new SqliteRelationalDatabaseTable(connection, database, tableName);

            Assert.AreEqual(tableName.LocalName, table.Name.LocalName);
        }
    }
}
