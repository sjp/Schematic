using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalDatabaseViewTests
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
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseView(null, database, "test"));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseView(connection, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseView(connection, database, null));
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
            const string viewName = "view_test_view_1";

            var view = new SqlServerRelationalDatabaseView(connection, database, viewName);

            Assert.AreEqual(viewName, view.Name.LocalName);
        }
    }
}
