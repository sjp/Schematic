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
            var viewName = new Identifier("test", "test_view");
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(null, typeProvider, viewName, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var viewName = new Identifier("test", "test_view");
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, null, viewName, identifierResolver));
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
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            const string viewName = "test_view";
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentException>(() => new PostgreSqlRelationalDatabaseView(connection, typeProvider, viewName, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("test", "test_view");

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseView(connection, typeProvider, viewName, null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("test", "test_view");
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var view = new PostgreSqlRelationalDatabaseView(connection, typeProvider, viewName, identifierResolver);

            Assert.AreEqual(viewName, view.Name);
        }
    }
}
