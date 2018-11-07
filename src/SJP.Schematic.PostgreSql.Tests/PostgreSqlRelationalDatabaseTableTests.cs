using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlRelationalDatabaseTableTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(null, database, typeProvider, "test", identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(connection, null, typeProvider, "test", identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(connection, database, null, "test", identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(connection, database, typeProvider, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(connection, database, typeProvider, "test", null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var tableName = new Identifier("table_test_table_1");
            var table = new PostgreSqlRelationalDatabaseTable(connection, database, typeProvider, tableName, identifierResolver);

            Assert.AreEqual(tableName, table.Name);
        }
    }
}
