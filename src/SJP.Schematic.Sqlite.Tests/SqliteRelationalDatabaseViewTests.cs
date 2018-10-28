using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteRelationalDatabaseViewTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(null, database, new Identifier("main", "test")));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(connection, null, new Identifier("main", "test")));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalDatabaseView(connection, database, null));
        }

        [Test]
        public static void Ctor_GivenNullSchemaName_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalDatabaseView(connection, database, new Identifier("test_view")));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;
            var viewName = new Identifier("main", "table_test_view_1");

            var view = new SqliteRelationalDatabaseView(connection, database, viewName);

            Assert.AreEqual(viewName.LocalName, view.Name.LocalName);
        }

        [Test]
        public static void Name_GivenSchemaAndLocalNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            var viewName = new Identifier("asd", "view_test_view_1");
            var expectedViewName = new Identifier("asd", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(connection, database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public static void Name_GivenDatabaseAndSchemaAndLocalNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            var viewName = new Identifier("qwe", "asd", "view_test_view_1");
            var expectedViewName = new Identifier("asd", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(connection, database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }

        [Test]
        public static void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var connection = Mock.Of<IDbConnection>();
            var dbMock = new Mock<IRelationalDatabase>();
            var dialect = Mock.Of<IDatabaseDialect>();
            dbMock.SetupGet(db => db.Dialect).Returns(dialect);
            var database = dbMock.Object;

            var viewName = new Identifier("qwe", "asd", "zxc", "view_test_view_1");
            var expectedViewName = new Identifier("zxc", "view_test_view_1");

            var view = new SqliteRelationalDatabaseView(connection, database, viewName);

            Assert.AreEqual(expectedViewName, view.Name);
        }
    }
}
