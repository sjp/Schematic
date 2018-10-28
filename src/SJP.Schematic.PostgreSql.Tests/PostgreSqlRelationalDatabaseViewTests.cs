using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlRelationalDatabaseViewTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(null, typeProvider, "test"));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, typeProvider, null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var viewName = new Identifier("view_test_view_1");
            var view = new PostgreSqlRelationalDatabaseView(connection, typeProvider, viewName);

            Assert.AreEqual(viewName, view.Name);
        }
    }
}
