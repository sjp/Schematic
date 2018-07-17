using System;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerExpressionComparerTests
    {
        [Test]
        public static void Ctor_GivenNullComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            Assert.DoesNotThrow(() => new SqlServerExpressionComparer(sqlStringComparer: argComparer));
        }

        [Test]
        public static void Ctor_GivenNullSqlStringComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            Assert.DoesNotThrow(() => new SqlServerExpressionComparer(argComparer));
        }

        [Test]
        public static void Ctor_GivenNoComparers_CreatesWithoutError()
        {
            Assert.DoesNotThrow(() => new SqlServerExpressionComparer());
        }

        [Test]
        public static void Equals_GivenEqualSqlStringArguments_ReturnsTrue()
        {
            const string input = "'abc'";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringArguments_ReturnsFalse()
        {
            const string inputX = "'abc'";
            const string inputY = "'zxc'";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualSqlStringArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "('abc')";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "('abc')";
            const string inputY = "('zxc')";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualSqlStringsWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "('abc')";
            const string inputY = "'abc'";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringsWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "('abc')";
            const string inputY = "'zxc'";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualDateArguments_ReturnsTrue()
        {
            const string input = "getdate()";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentDateArguments_ReturnsFalse()
        {
            const string inputX = "getdate()";
            const string inputY = "getutcdate()";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualDateArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "(getdate())";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentDateArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "(getdate())";
            const string inputY = "(getutcdate())";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualDatesWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(getdate())";
            const string inputY = "getdate()";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentDatesWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(getdate())";
            const string inputY = "getutcdate()";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualNumberArguments_ReturnsTrue()
        {
            const string input = "123";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentNumberArguments_ReturnsFalse()
        {
            const string inputX = "123";
            const string inputY = "456";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualNumberArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "(123)";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentNumberArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "(123)";
            const string inputY = "(456)";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualNumbersWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(123)";
            const string inputY = "123";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentNumbersWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(123)";
            const string inputY = "456";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualNumbersWithOneWrappedTwiceInParens_ReturnsTrue()
        {
            const string inputX = "((123))";
            const string inputY = "123";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressions_ReturnsTrue()
        {
            const string input = "[test_column_1] > len(left([abc], 50))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDifferentComplexExpressions_ReturnsFalse()
        {
            const string inputX = "[test_column_1] > len(left([abc], 50))";
            const string inputY = "[test_column_2] < len(left([abc], 50))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsWrappedInParens_ReturnsTrue()
        {
            const string input = "([test_column_1] > len(left([abc], 50)))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsOneWithParenWrappedNumericValue_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "[test_column_1] > len(left([abc], 50))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsOneWithWhitespaceRemoved_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "[test_column_1]>len(left([abc],50))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([abc], (50))))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([abc], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([abc], (50))))";
            var comparer = new SqlServerExpressionComparer(StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }

        [Test]
        public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left('asd', (50))))";
            const string inputY = "([test_column_1] > len(left('ASD', (50))))";
            var comparer = new SqlServerExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsFalse(equals);
        }

        [Test]
        public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left('asd', (50))))";
            const string inputY = "([test_column_1] > len(left('ASD', (50))))";
            var comparer = new SqlServerExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.IsTrue(equals);
        }
    }
}
