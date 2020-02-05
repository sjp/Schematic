using System;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleExpressionComparerTests
    {
        [Test]
        public static void Ctor_GivenNullComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            Assert.That(() => new OracleExpressionComparer(sqlStringComparer: argComparer), Throws.Nothing);
        }

        [Test]
        public static void Ctor_GivenNullSqlStringComparer_CreatesWithoutError()
        {
            var argComparer = StringComparer.Ordinal;
            Assert.That(() => new OracleExpressionComparer(argComparer), Throws.Nothing);
        }

        [Test]
        public static void Ctor_GivenNoComparers_CreatesWithoutError()
        {
            Assert.That(() => new OracleExpressionComparer(), Throws.Nothing);
        }

        [Test]
        public static void Equals_GivenEqualSqlStringArguments_ReturnsTrue()
        {
            const string input = "'test'";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringArguments_ReturnsFalse()
        {
            const string inputX = "'test'";
            const string inputY = "'alternative'";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualSqlStringArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "('test')";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "('test')";
            const string inputY = "('alternative')";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualSqlStringsWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "('test')";
            const string inputY = "'test'";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentSqlStringsWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "('test')";
            const string inputY = "'alternative'";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualDateArguments_ReturnsTrue()
        {
            const string input = "getdate()";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentDateArguments_ReturnsFalse()
        {
            const string inputX = "getdate()";
            const string inputY = "getutcdate()";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualDateArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "(getdate())";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentDateArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "(getdate())";
            const string inputY = "(getutcdate())";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualDatesWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(getdate())";
            const string inputY = "getdate()";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentDatesWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(getdate())";
            const string inputY = "getutcdate()";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualNumberArguments_ReturnsTrue()
        {
            const string input = "123";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentNumberArguments_ReturnsFalse()
        {
            const string inputX = "123";
            const string inputY = "456";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualNumberArgumentsWrappedInParens_ReturnsTrue()
        {
            const string input = "(123)";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentNumberArgumentsWrappedInParens_ReturnsFalse()
        {
            const string inputX = "(123)";
            const string inputY = "(456)";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualNumbersWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(123)";
            const string inputY = "123";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentNumbersWithOneWrappedInParens_ReturnsTrue()
        {
            const string inputX = "(123)";
            const string inputY = "456";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualNumbersWithOneWrappedTwiceInParens_ReturnsTrue()
        {
            const string inputX = "((123))";
            const string inputY = "123";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressions_ReturnsTrue()
        {
            const string input = "[test_column_1] > len(left([test], 50))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDifferentComplexExpressions_ReturnsFalse()
        {
            const string inputX = "[test_column_1] > len(left([test], 50))";
            const string inputY = "[test_column_2] < len(left([test], 50))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsWrappedInParens_ReturnsTrue()
        {
            const string input = "([test_column_1] > len(left([test], 50)))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(input, input);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsOneWithParenWrappedNumericValue_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([test], (50))))";
            const string inputY = "[test_column_1] > len(left([test], 50))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenEqualComplexExpressionsOneWithWhitespaceRemoved_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([test], (50))))";
            const string inputY = "[test_column_1]>len(left([test],50))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left([test], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([test], (50))))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left([test], (50))))";
            const string inputY = "([TEST_Column_1] > len(left([test], (50))))";
            var comparer = new OracleExpressionComparer(StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }

        [Test]
        public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsFalse()
        {
            const string inputX = "([test_column_1] > len(left('test', (50))))";
            const string inputY = "([test_column_1] > len(left('TEST', (50))))";
            var comparer = new OracleExpressionComparer();

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.False);
        }

        [Test]
        public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsTrue()
        {
            const string inputX = "([test_column_1] > len(left('test', (50))))";
            const string inputY = "([test_column_1] > len(left('TEST', (50))))";
            var comparer = new OracleExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

            var equals = comparer.Equals(inputX, inputY);

            Assert.That(equals, Is.True);
        }
    }
}
