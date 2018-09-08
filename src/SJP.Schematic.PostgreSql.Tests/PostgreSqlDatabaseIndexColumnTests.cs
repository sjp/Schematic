using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseIndexColumnTests
    {
        [Test]
        public static void Ctor_GivenNullExpression_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn((string)null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenEmptyExpression_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn(string.Empty, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpression_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn("    ", IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn((IDatabaseColumn)null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenInvalidColumnOrder_ThrowsArgumentException()
        {
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseIndexColumn(column, order));
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

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
            var indexColumn = new PostgreSqlDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            Assert.Throws<ArgumentNullException>(() => indexColumn.GetExpression(null));
        }

        [Test]
        public static void GetExpression_WhenInvoked_EqualsQuotedColumnName()
        {
            const string expectedExpression = "\"test\"";

            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.SetupGet(c => c.Name).Returns("test");
            var column = columnMock.Object;

            var indexColumn = new PostgreSqlDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            var dialect = new PostgreSqlDialect();
            var result = indexColumn.GetExpression(dialect);

            Assert.AreEqual(expectedExpression, result);
        }

        [Test]
        public static void Order_WithAscendingCtorArgPropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(column, IndexColumnOrder.Ascending);

            Assert.AreEqual(IndexColumnOrder.Ascending, indexColumn.Order);
        }

        [Test]
        public static void Order_WithDescendingCtorArgPropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(column, IndexColumnOrder.Descending);

            Assert.AreEqual(IndexColumnOrder.Descending, indexColumn.Order);
        }
    }
}
