using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleRelationalDatabaseViewTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("TEST", "TEST_VIEW");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabaseView(null, typeProvider, viewName, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var viewName = new Identifier("TEST", "TEST_VIEW");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabaseView(connection, null, viewName, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabaseView(connection, typeProvider, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            const string viewName = "TEST_VIEW";
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentException>(() => new OracleRelationalDatabaseView(connection, typeProvider, viewName, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("TEST", "TEST_VIEW");

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalDatabaseView(connection, typeProvider, viewName, null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var viewName = new Identifier("TEST", "TEST_VIEW");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var view = new OracleRelationalDatabaseView(connection, typeProvider, viewName, identifierResolver);

            Assert.AreEqual(viewName, view.Name);
        }
    }
}
