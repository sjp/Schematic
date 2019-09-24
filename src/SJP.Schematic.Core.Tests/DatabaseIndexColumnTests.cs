using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseIndexColumnTests
    {
        [Test]
        public static void Ctor_GivenNullExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn(null, column, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenEmptyExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn(string.Empty, column, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn("   ", column, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            const string expression = "lower(test_column)";

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn(expression, (IDatabaseColumn)null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenNullDependentColumns_ThrowsArgumentNullException()
        {
            const string expression = "lower(test_column)";

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn(expression, (IEnumerable<IDatabaseColumn>)null, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenDependentColumnsWithNullValue_ThrowsArgumentNullException()
        {
            const string expression = "lower(test_column)";
            var columns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new DatabaseIndexColumn(expression, columns, IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Ctor_GivenInvalidIndexColumnOrder_ThrowsArgumentException()
        {
            const string expression = "lower(test_column)";
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.Throws<ArgumentException>(() => new DatabaseIndexColumn(expression, column, order));
        }

        [Test]
        public static void Expression_PropertyGet_EqualsCtorArg()
        {
            const string expression = "lower(test_column)";
            var column = Mock.Of<IDatabaseColumn>();

            var indexColumn = new DatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.AreEqual(expression, indexColumn.Expression);
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            const string expression = "lower(test_column)";
            var column = Mock.Of<IDatabaseColumn>();

            var indexColumn = new DatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);
            var indexDependentColumn = indexColumn.DependentColumns.Single();

            Assert.AreEqual(column, indexDependentColumn);
        }

        [Test]
        public static void Order_WhenAscendingProvidedInCtor_ReturnsAscending()
        {
            const string expression = "lower(test_column)";
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = IndexColumnOrder.Ascending;

            var indexColumn = new DatabaseIndexColumn(expression, column, order);

            Assert.AreEqual(order, indexColumn.Order);
        }

        [Test]
        public static void Order_WhenDescendingProvidedInCtor_ReturnsDescending()
        {
            const string expression = "lower(test_column)";
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = IndexColumnOrder.Descending;

            var indexColumn = new DatabaseIndexColumn(expression, column, order);

            Assert.AreEqual(order, indexColumn.Order);
        }
    }
}
