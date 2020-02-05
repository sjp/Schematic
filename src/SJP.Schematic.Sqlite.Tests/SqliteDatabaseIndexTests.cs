using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDatabaseIndexTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            Assert.That(() => new SqliteDatabaseIndex(null, isUnique, columns, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            Assert.That(() => new SqliteDatabaseIndex(indexName, isUnique, null, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            Assert.That(() => new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            Assert.That(() => new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumns = new IDatabaseColumn[] { null };

            Assert.That(() => new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.Name, Is.EqualTo(indexName));
        }

        [Test]
        public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.IsUnique, Is.True);
        }

        [Test]
        public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.IsUnique, Is.False);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.Columns, Is.EqualTo(columns));
        }

        [Test]
        public static void IncludedColumns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.IncludedColumns, Is.EqualTo(includedColumns));
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumn = Mock.Of<IDatabaseColumn>();
            var includedColumns = new[] { includedColumn };

            var index = new SqliteDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.IsEnabled, Is.True);
        }
    }
}
