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
            var dialect = Mock.Of<IDatabaseDialect>();
            var dbMock = new Mock<IRelationalDatabase>();
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
            var dialect = Mock.Of<IDatabaseDialect>();
            var dbMock = new Mock<IRelationalDatabase>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

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
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var tableName = new Identifier("main", "test_1");

            var table = new SqliteRelationalDatabaseTable(connection, database, tableName);

            Assert.AreEqual(tableName, table.Name);
        }

        public static void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldMatchArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var tableName = new Identifier("asd", "table_test_table_1");

            var table = new SqliteRelationalDatabaseTable(connection, database, tableName);

            Assert.AreEqual(tableName, table.Name);
        }

        [Test]
        public static void Name_GivenDatabaseAndSchemaAndLocalNameInCtor_ShouldBeOnlySchemaAndLocalName()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var tableName = new Identifier("qwe", "asd", "table_test_table_1");
            var expectedTableName = new Identifier("asd", "table_test_table_1");

            var table = new SqliteRelationalDatabaseTable(connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public static void Name_GivenFullyQualifiedNameInCtor_ShouldBeOnlySchemaAndLocalName()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var tableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");
            var expectedTableName = new Identifier("zxc", "table_test_table_1");

            var table = new SqliteRelationalDatabaseTable(connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }
    }
}
