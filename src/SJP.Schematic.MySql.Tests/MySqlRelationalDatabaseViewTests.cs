using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseViewTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("test", "test_view");

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseView(null, typeProvider, viewName));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var viewName = new Identifier("test", "test_view");

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseView(connection, null, viewName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseView(connection, typeProvider, null));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            const string viewName = "test_view";

            Assert.Throws<ArgumentException>(() => new MySqlRelationalDatabaseView(connection, typeProvider, viewName));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("test", "test_view");

            var view = new MySqlRelationalDatabaseView(connection, typeProvider, viewName);

            Assert.AreEqual(viewName, view.Name);
        }
    }
}
