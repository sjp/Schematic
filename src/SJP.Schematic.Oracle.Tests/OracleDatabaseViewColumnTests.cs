using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseViewColumnTests
    {
        [Test]
        public static void Ctor_GivenNullView_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseViewColumn(null, "test_column", columnType, true, null));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseViewColumn(view, null, columnType, true, null));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseViewColumn(view, "test_column", null, true, null));
        }

        [Test]
        public static void View_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            var view = new Mock<IRelationalDatabaseView>();
            view.Setup(t => t.Name).Returns(viewName);
            var viewArg = view.Object;

            var columnType = Mock.Of<IDbType>();

            var column = new OracleDatabaseViewColumn(viewArg, "test_column", columnType, true, null);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(viewName, column.View.Name);
                Assert.AreSame(viewArg, column.View);
            });
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();

            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, null);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();

            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, null);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseViewColumn(view, columnName, columnType, false, null);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, null);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void DefaultValue_PropertyGet_ReturnsCtorArg()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            const string defaultValue = "1";
            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, defaultValue);

            Assert.AreEqual(defaultValue, column.DefaultValue);
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsFalse()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, null);

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public static void AutoIncrement_PropertyGet_EqualsNull()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new OracleDatabaseViewColumn(view, columnName, columnType, true, null);

            Assert.IsNull(column.AutoIncrement);
        }
    }
}
