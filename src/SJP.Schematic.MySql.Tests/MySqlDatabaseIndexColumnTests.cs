using Moq;
using NUnit.Framework;
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

            Assert.That(() => new MySqlDatabaseIndexColumn(null, column), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.That(() => new MySqlDatabaseIndexColumn(string.Empty, column), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceExpression_ThrowsArgumentNullException()
        {
            var column = Mock.Of<IDatabaseColumn>();

            Assert.That(() => new MySqlDatabaseIndexColumn("   ", column), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            const string expression = "`test`";

            Assert.That(() => new MySqlDatabaseIndexColumn(expression, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void DependentColumns_PropertyGet_EqualsCtorArg()
        {
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.Multiple(() =>
            {
                Assert.That(indexColumn.DependentColumns, Has.Exactly(1).Items);
                Assert.That(indexColumn.DependentColumns[0], Is.EqualTo(column));
            });
        }

        [Test]
        public static void Expression_PropertyGet_EqualsCtorArg()
        {
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.That(indexColumn.Expression, Is.EqualTo(expression));
        }

        [Test]
        public static void Order_PropertyGet_EqualsAscending()
        {
            // MySQL doesn't support descending ordering so this is always true
            const string expression = "`test`";
            var column = Mock.Of<IDatabaseColumn>();
            var indexColumn = new MySqlDatabaseIndexColumn(expression, column);

            Assert.That(indexColumn.Order, Is.EqualTo(IndexColumnOrder.Ascending));
        }
    }
}
