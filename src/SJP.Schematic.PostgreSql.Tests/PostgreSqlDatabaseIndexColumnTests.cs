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
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn(null, IndexColumnOrder.Ascending));
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
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseIndexColumn("test", null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenInvalidColumnOrder_ThrowsArgumentException()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseIndexColumn(expression, column, order));
        }

        [Test]
        public static void Expression_PropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.AreEqual(expression, indexColumn.Expression);
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumn.DependentColumns.Count);
                Assert.AreEqual(column, indexColumn.DependentColumns[0]);
            });
        }

        [Test]
        public static void Order_WithAscendingCtorArgPropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.AreEqual(IndexColumnOrder.Ascending, indexColumn.Order);
        }

        [Test]
        public static void Order_WithDescendingCtorArgPropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Descending);

            Assert.AreEqual(IndexColumnOrder.Descending, indexColumn.Order);
        }
    }
}
