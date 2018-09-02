using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDatabaseTableIndexTests
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

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(null, indexName, isUnique, columns, includedColumns));
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

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(table, null, isUnique, columns, includedColumns));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(table, indexName, isUnique, null, includedColumns));
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

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns));
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

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns));
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

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns));
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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

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

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

            Assert.AreEqual(includedColumns, index.IncludedColumns);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseTableColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseTableIndex(table, indexName, isUnique, columns, includedColumns);

            Assert.IsTrue(index.IsEnabled);
        }
    }
}
