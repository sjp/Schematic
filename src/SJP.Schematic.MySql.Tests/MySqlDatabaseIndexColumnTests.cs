using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlDatabaseIndexColumnTests
    {
        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseIndexColumn(null));
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(column);

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
            var indexColumn = new MySqlDatabaseIndexColumn(column);

            Assert.Throws<ArgumentNullException>(() => indexColumn.GetExpression(null));
        }

        [Test]
        public static void GetExpression_WhenInvoked_EqualsQuotedColumnName()
        {
            const string expectedExpression = "`test`";

            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.SetupGet(c => c.Name).Returns("test");
            var column = columnMock.Object;

            var indexColumn = new MySqlDatabaseIndexColumn(column);

            var dialect = new MySqlDialect();
            var result = indexColumn.GetExpression(dialect);

            Assert.AreEqual(expectedExpression, result);
        }

        [Test]
        public static void Order_PropertyGet_EqualsAscending()
        {
            // MySQL doesn't support descending ordering so this is always true
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(column);

            Assert.AreEqual(IndexColumnOrder.Ascending, indexColumn.Order);
        }
    }
}
