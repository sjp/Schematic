using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseComputedColumnTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedColumn(null, columnType, true, definition));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            const string definition = "test";

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseComputedColumn("test_column", null, true, definition));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, false, definition);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsTrue()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.IsTrue(column.IsComputed);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.AreEqual(definition, column.Definition.UnwrapSome());
        }

        [Test]
        public static void AutoIncrement_PropertyGet_ReturnsNone()
        {
            Identifier columnName = "test_column";
            var columnType = Mock.Of<IDbType>();
            const string definition = "test";
            var column = new OracleDatabaseComputedColumn(columnName, columnType, true, definition);

            Assert.IsTrue(column.AutoIncrement.IsNone);
        }
    }
}
