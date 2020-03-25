using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseIndexColumnTests
    {
        [Test]
        public static void Ctor_GivenNullExpression_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlDatabaseIndexColumn(null, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyExpression_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlDatabaseIndexColumn(string.Empty, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpression_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlDatabaseIndexColumn("    ", IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullExpressionWithColumn_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.That(() => new PostgreSqlDatabaseIndexColumn(null, column, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyExpressionWithColumn_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.That(() => new PostgreSqlDatabaseIndexColumn(string.Empty, column, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpressionWithColumn_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.That(() => new PostgreSqlDatabaseIndexColumn("    ", column, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlDatabaseIndexColumn("test", null, IndexColumnOrder.Ascending), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidColumnOrder_ThrowsArgumentException()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, column, order), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenInvalidColumnOrderWithoutColumn_ThrowsArgumentException()
        {
            const string expression = "\"test\"";
            const IndexColumnOrder order = (IndexColumnOrder)55;

            Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, order), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_WhenGivenValidInputWithoutColumn_DoesNotThrow()
        {
            const string expression = "\"test\"";

            Assert.That(() => new PostgreSqlDatabaseIndexColumn(expression, IndexColumnOrder.Ascending), Throws.Nothing);
        }

        [Test]
        public static void Expression_PropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.That(indexColumn.Expression, Is.EqualTo(expression));
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.Multiple(() =>
            {
                Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
                Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
            });
        }

        [Test]
        public static void Order_WithAscendingCtorArgPropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Ascending);

            Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Ascending));
        }

        [Test]
        public static void Order_WithDescendingCtorArgPropertyGet_EqualsCtorArg()
        {
            const string expression = "\"test\"";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new PostgreSqlDatabaseIndexColumn(expression, column, IndexColumnOrder.Descending);

            Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Descending));
        }
    }
}
