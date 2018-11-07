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
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(null, typeProvider, "test", identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, null, "test", identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, typeProvider, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, typeProvider, "test", null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var viewName = new Identifier("view_test_view_1");
            var view = new PostgreSqlRelationalDatabaseView(connection, typeProvider, viewName, identifierResolver);

            Assert.AreEqual(viewName, view.Name);
        }
    }
}
