using System;
using NUnit.Framework;
using Moq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseIndexTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(null, false, columns, Array.Empty<IDatabaseColumn>(), true));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(indexName, false, null, Array.Empty<IDatabaseColumn>(), true));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            var columns = Array.Empty<IDatabaseIndexColumn>();

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(indexName, false, columns, Array.Empty<IDatabaseColumn>(), true));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            var columns = new IDatabaseIndexColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(indexName, false, columns, Array.Empty<IDatabaseColumn>(), true));
        }

        [Test]
        public static void Ctor_GivenNullIncludedColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(indexName, false, columns, null, true));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_DoesNotThrowArgumentNullException()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            Assert.DoesNotThrow(() => new DatabaseIndex(indexName, false, columns, includedColumns, true));
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = new[] { (IDatabaseColumn)null };

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndex(indexName, false, columns, includedColumns, true));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, false, columns, includedColumns, true);

            Assert.AreEqual(indexName, index.Name);
        }

        [Test]
        public static void IsUnique_GivenFalseCtorArgPropertyGet_ReturnsFalse()
        {
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, isUnique, columns, includedColumns, true);

            Assert.AreEqual(isUnique, index.IsUnique);
        }

        [Test]
        public static void IsUnique_GivenFalseCtorArgPropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, isUnique, columns, includedColumns, true);

            Assert.AreEqual(isUnique, index.IsUnique);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgPropertyGet_ReturnsFalse()
        {
            Identifier indexName = "test_index";
            const bool isEnabled = false;
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, false, columns, includedColumns, isEnabled);

            Assert.AreEqual(isEnabled, index.IsEnabled);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgPropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isEnabled = true;
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, false, columns, includedColumns, isEnabled);

            Assert.AreEqual(isEnabled, index.IsEnabled);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = Array.Empty<IDatabaseColumn>();

            var index = new DatabaseIndex(indexName, false, columns, includedColumns, true);

            Assert.AreEqual(columns, index.Columns);
        }

        [Test]
        public static void IncludedColumns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };
            var includedColumns = new[] { Mock.Of<IDatabaseColumn>() };

            var index = new DatabaseIndex(indexName, false, columns, includedColumns, true);

            Assert.AreEqual(includedColumns, index.IncludedColumns);
        }
    }
}
