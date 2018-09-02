using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseTableIndexTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(null, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(table, null, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(table, indexName, isUnique, null, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumns = new IDatabaseTableColumn[] { null };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(table, index.Table);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(indexName, index.Name);
        }

        [Test]
        public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsTrue(index.IsUnique);
        }

        [Test]
        public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsFalse(index.IsUnique);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(columns, index.Columns);
        }

        [Test]
        public static void IncludedColumns_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.AreEqual(includedColumns, index.IncludedColumns);
        }

        [Test]
        public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = true;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsTrue(index.IsEnabled);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };
            const bool enabled = false;

            var index = new SqlServerDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns, enabled);

            Assert.IsFalse(index.IsEnabled);
        }
    }
}
