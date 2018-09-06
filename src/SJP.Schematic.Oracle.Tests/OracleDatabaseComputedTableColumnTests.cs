using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseComputedTableColumnTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(null, "test_column", columnType, true, definition));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(table, null, columnType, true, definition));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const string definition = "test";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(table, "test_column", null, true, definition));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(table, "test_column", null, true, null));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var definition = string.Empty;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(table, "test_column", null, true, definition));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const string definition = "     ";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedTableColumn(table, "test_column", null, true, definition));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var table = new Mock<IRelationalDatabaseTable>();
            table.Setup(t => t.Name).Returns(tableName);
            var tableArg = table.Object;
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";

            var column = new OracleDatabaseComputedTableColumn(tableArg, "test_column", columnType, true, definition);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(tableName, column.Table.Name);
                Assert.AreSame(tableArg, column.Table);
            });
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, false, definition);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsTrue()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.AreEqual(definition, column.Definition);
        }

        [Test]
        public static void AutoIncrement_PropertyGet_ReturnsNull()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedTableColumn(table, columnName, columnType, true, definition);

            Assert.IsNull(column.AutoIncrement);
        }
    }
}
