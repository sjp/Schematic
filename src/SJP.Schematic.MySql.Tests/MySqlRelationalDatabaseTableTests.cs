using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalDatabaseTableTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var tableName = new Identifier("test", "test_table");

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(null, database, typeProvider, tableName));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            var tableName = new Identifier("test", "test_table");

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(connection, null, typeProvider, tableName));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("test", "test_table");

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(connection, database, null, tableName));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();
            const string tableName = "test_table";

            Assert.Throws<ArgumentException>(() => new MySqlRelationalDatabaseTable(connection, database, typeProvider, tableName));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var tableName = new Identifier("test", "test_table");
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var table = new MySqlRelationalDatabaseTable(connection, database, typeProvider, tableName);

            Assert.AreEqual(tableName, table.Name);
        }
    }
}
