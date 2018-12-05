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
        public static void Ctor_GivenNullExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseIndexColumn(null, column));
        }

        [Test]
        public static void Ctor_GivenEmptyExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseIndexColumn(string.Empty, column));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseIndexColumn("   ", column));
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            const string expression = "`test`";

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseIndexColumn(expression, null));
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, indexColumn.DependentColumns.Count);
                Assert.AreEqual(column, indexColumn.DependentColumns[0]);
            });
        }

        [Test]
        public static void Expression_PropertyGet_EqualsCtorArg()
        {
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.AreEqual(expression, indexColumn.Expression);
        }

        [Test]
        public static void Order_PropertyGet_EqualsAscending()
        {
            // MySQL doesn't support descending ordering so this is always true
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.AreEqual(IndexColumnOrder.Ascending, indexColumn.Order);
        }
    }
}
