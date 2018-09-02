using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseViewIndexTests
    {
        [Test]
        public static void Ctor_GivenNullView_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(null, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(view, null, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(view, indexName, isUnique, null, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var taviewle = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumns = new IDatabaseViewColumn[] { null };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseViewIndex(taviewle, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void View_PropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(view, index.View);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(indexName, index.Name);
        }

        [Test]
        public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(columns, index.Columns);
        }

        [Test]
        public static void IncludedColumns_PropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(includedColumns, index.IncludedColumns);
        }

        [Test]
        public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseViewColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = false;

            var index = new SqlServerDatabaseViewIndex(view, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsFalse(index.IsEnabled);
        }
    }
}
