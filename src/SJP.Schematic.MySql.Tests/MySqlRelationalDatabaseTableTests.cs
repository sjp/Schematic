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

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(null, database, typeProvider, "test"));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(connection, null, typeProvider, "test"));
        }

        [Test]
        public static void Ctor_GivenNullTypeProvider_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(connection, database, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalDatabaseTable(connection, database, typeProvider, null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var database = Mock.Of<IRelationalDatabase>();
            var typeProvider = Mock.Of<IDbTypeProvider>();

            var tableName = new Identifier("table_test_table_1");
            var table = new MySqlRelationalDatabaseTable(connection, database, typeProvider, tableName);

            Assert.AreEqual(tableName, table.Name);
        }
    }
}
