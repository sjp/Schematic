using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseIndexTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.That(() => new PostgreSqlDatabaseIndex(null, isUnique, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;

            Assert.That(() => new PostgreSqlDatabaseIndex(indexName, isUnique, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();

            Assert.That(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };

            Assert.That(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIncludedColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.That(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenIncludedColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            var includedColumns = new IDatabaseColumn[] { null };

            Assert.That(() => new PostgreSqlDatabaseIndex(indexName, isUnique, columns, includedColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.Name, Is.EqualTo(indexName));
        }

        [Test]
        public static void IsUnique_WithTrueCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.IsUnique, Is.True);
        }

        [Test]
        public static void IsUnique_WithFalseCtorArgPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.IsUnique, Is.False);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.Columns, Is.EqualTo(columns));
        }

        [Test]
        public static void IncludedColumns_PropertyGetWhenNotProvidedInCtor_IsEmpty()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.IncludedColumns, Is.Empty);
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

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns, includedColumns);

            Assert.That(index.IncludedColumns, Is.EqualTo(includedColumns));
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, columns);

            Assert.That(index.IsEnabled, Is.True);
        }

        [TestCase("test_index", "Index: test_index")]
        [TestCase("test_index_other", "Index: test_index_other")]
        public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
        {
            var indexName = Identifier.CreateQualifiedIdentifier(name);
            var columns = new[] { Mock.Of<IDatabaseIndexColumn>() };

            var index = new PostgreSqlDatabaseIndex(indexName, false, columns);
            var result = index.ToString();

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
