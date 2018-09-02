using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseViewIndexTests
    {
        [Test]
        public static void Ctor_GivenNullView_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseViewIndex(null, indexName, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseViewIndex(view, null, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            var view = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

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

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

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

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

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

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

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

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

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

            var index = new PostgreSqlDatabaseViewIndex(view, indexName, isUnique, columns);

            Assert.Zero(index.IncludedColumns.Count);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseView>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };

            var index = new PostgreSqlDatabaseViewIndex(table, indexName, isUnique, columns);

            Assert.IsTrue(index.IsEnabled);
        }
    }
}
