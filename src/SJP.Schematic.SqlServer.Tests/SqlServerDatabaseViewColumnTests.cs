using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseViewColumnTests
    {
        [Test]
        public static void Ctor_GivenNullView_ThrowsArgumentNullException()
        {
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewColumn(null, "test_column", columnType, true, null, null));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewColumn(view, null, columnType, true, null, null));
        }

        [Test]
        public static void Ctor_GivenNullType_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewColumn(view, "test_column", null, true, null, null));
        }

        [Test]
        public static void View_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            var view = new Mock<IRelationalDatabaseView>();
            view.Setup(t => t.Name).Returns(viewName);
            var viewArg = view.Object;

            var columnType = Mock.Of<IDbType>();

            var column = new SqlServerDatabaseViewColumn(viewArg, "test_column", columnType, true, null, null);

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

            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, null);

            Assert.AreEqual(columnName, column.Name);
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();

            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, null);

            Assert.AreEqual(columnType, column.Type);
        }

        [Test]
        public static void IsNullable_GivenFalseCtorArgPropertyGet_EqualsFalse()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, false, null, null);

            Assert.IsFalse(column.IsNullable);
        }

        [Test]
        public static void IsNullable_GivenTrueCtorArgPropertyGet_EqualsTrue()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, null);

            Assert.IsTrue(column.IsNullable);
        }

        [Test]
        public static void IsComputed_PropertyGet_ReturnsFalse()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, null);

            Assert.IsFalse(column.IsComputed);
        }

        [Test]
        public static void AutoIncrement_GivenNullCtorArgPropertyGet_EqualsNull()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, null);

            Assert.IsNull(column.AutoIncrement);
        }

        [Test]
        public static void AutoIncrement_GivenValidCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier columnName = "test_column";
            var view = Mock.Of<IRelationalDatabaseView>();
            var columnType = Mock.Of<IDbType>();
            var autoIncrement = new AutoIncrement(30, 2);

            var column = new SqlServerDatabaseViewColumn(view, columnName, columnType, true, null, autoIncrement);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(autoIncrement.InitialValue, column.AutoIncrement.InitialValue);
                Assert.AreEqual(autoIncrement.Increment, column.AutoIncrement.Increment);
            });
        }
    }
}
