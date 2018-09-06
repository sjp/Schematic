using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseTableColumnTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTableColumn(null, "test_column", columnType, true, null, null));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTableColumn(table, null, columnType, true, null, null));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseTableColumn(table, "test_column", null, true, null, null));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var table = new Mock<IRelationalDatabaseTable>();
            table.Setup(t => t.Name).Returns(tableName);
            var tableArg = table.Object;

            var columnType = Mock.Of<IDbType>();

            var column = new PostgreSqlDatabaseTableColumn(tableArg, "test_column", columnType, true, null, null);

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

            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, null);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();

            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, null);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, false, null, null);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, null);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void DefaultValue_PropertyGet_ReturnsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            const string defaultValue = "1";
            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, defaultValue, null);

            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsFalse()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, null);

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public static void AutoIncrement_GivenNullCtorArgPropertyGet_EqualsNull()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, null);

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public static void AutoIncrement_GivenValidCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var columnType = Mock.Of<IDbType>();
            var autoIncrement = new AutoIncrement(30, 2);

            var column = new PostgreSqlDatabaseTableColumn(table, columnName, columnType, true, null, autoIncrement);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(autoIncrement.InitialValue, column.AutoIncrement.InitialValue);
                Assert.AreEqual(autoIncrement.Increment, column.AutoIncrement.Increment);
            });
        }
    }
}
