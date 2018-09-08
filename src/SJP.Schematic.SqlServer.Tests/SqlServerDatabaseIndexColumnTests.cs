using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseIndexColumnTests
    {
        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseIndexColumn((IDatabaseColumn)null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenInvalidColumnOrder_ThrowsArgumentException()
        {
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseIndexColumn(column, order));
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new SqlServerDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumn.DependentColumns.Count);
                Assert.AreEqual(column, indexColumn.DependentColumns[0]);
            });
        }

        [Test]
        public static void GetExpression_GivenNullDialect_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new SqlServerDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            Assert.Throws<ArgumentNullException>(() => indexColumn.GetExpression(null));
        }

        [Test]
        public static void GetExpression_WhenInvoked_EqualsQuotedColumnName()
        {
            const string expectedExpression = "[test]";

            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.SetupGet(c => c.Name).Returns("test");
            var column = columnMock.Object;

            var indexColumn = new SqlServerDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            var dialect = new SqlServerDialect();
            var result = indexColumn.GetExpression(dialect);

            Assert.AreEqual(expectedExpression, result);
        }

        [Test]
        public static void Order_WithAscendingCtorArgPropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new SqlServerDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            Assert.AreEqual(IndexColumnOrder.Ascending, indexColumn.Order);
        }

        [Test]
        public static void Order_WithDescendingCtorArgPropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new SqlServerDatabaseIndexColumn(column, IndexColumnOrder.Descending);

            Assert.AreEqual(IndexColumnOrder.Descending, indexColumn.Order);
        }
    }
}
