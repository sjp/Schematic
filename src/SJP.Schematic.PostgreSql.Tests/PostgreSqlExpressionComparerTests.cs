using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlExpressionComparerTests
{
    [Test]
    public static void Ctor_GivenNullComparer_CreatesWithoutError()
    {
        var argComparer = StringComparer.Ordinal;
        Assert.That(() => new PostgreSqlExpressionComparer(sqlStringComparer: argComparer), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNullSqlStringComparer_CreatesWithoutError()
    {
        var argComparer = StringComparer.Ordinal;
        Assert.That(() => new PostgreSqlExpressionComparer(argComparer), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new PostgreSqlExpressionComparer(), Throws.Nothing);
    }

    [Test]
    public static void Equals_GivenEqualSqlStringArguments_ReturnsTrue()
    {
        const string input = "'test'";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentSqlStringArguments_ReturnsFalse()
    {
        const string inputX = "'test'";
        const string inputY = "'alternative'";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualSqlStringArgumentsWrappedInParens_ReturnsTrue()
    {
        const string input = "('test')";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenEqualSqlStringsWithOneWrappedInParens_ReturnsTrue()
    {
        const string inputX = "('test')";
        const string inputY = "'test'";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenEqualNumberArguments_ReturnsTrue()
    {
        const string input = "123";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentNumberArguments_ReturnsFalse()
    {
        const string inputX = "123";
        const string inputY = "456";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualNumbersWithOneWrappedInParens_ReturnsTrue()
    {
        const string inputX = "(123)";
        const string inputY = "123";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenEqualNumbersWithOneWrappedTwiceInParens_ReturnsTrue()
    {
        const string inputX = "((123))";
        const string inputY = "123";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenEqualComplexExpressions_ReturnsTrue()
    {
        const string input = "\"test_column_1\" > length(left(\"test\", 50))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentComplexExpressions_ReturnsFalse()
    {
        const string inputX = "\"test_column_1\" > length(left(\"test\", 50))";
        const string inputY = "\"test_column_2\" < length(left(\"test\", 50))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualComplexExpressionsOneWithParenWrappedNumericValue_ReturnsTrue()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "\"test_column_1\" > length(left(\"test\", 50))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenEqualComplexExpressionsOneWithWhitespaceRemoved_ReturnsTrue()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "\"test_column_1\">length(left(\"test\",50))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsFalse()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "(\"TEST_Column_1\" > length(left(\"test\", (50))))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsTrue()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "(\"TEST_Column_1\" > length(left(\"test\", (50))))";
        var comparer = new PostgreSqlExpressionComparer(StringComparer.OrdinalIgnoreCase);

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsFalse()
    {
        const string inputX = "(\"test_column_1\" > length(left('test', (50))))";
        const string inputY = "(\"test_column_1\" > length(left('TEST', (50))))";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenIgnoreCaseStringComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsTrue()
    {
        const string inputX = "(\"test_column_1\" > length(left('test', (50))))";
        const string inputY = "(\"test_column_1\" > length(left('TEST', (50))))";
        var comparer = new PostgreSqlExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenNullArguments_ReturnsTrue()
    {
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(null, null);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenOneNullArgument_ReturnsFalse()
    {
        const string input = "'test'";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(input, null);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenNumberWrappedThreeTimesInParensAndUnwrappedValue_ReturnsFalse()
    {
        // StripWrappingParens only strips one outer wrapping pair plus a single (number) collapse per
        // occurrence -- it does not re-scan after a collapse, so a triple-wrapped value is left with
        // an unstripped pair of parens still around it, and is therefore not equal to the fully
        // unwrapped value.
        const string inputX = "(((5)))";
        const string inputY = "5";
        var comparer = new PostgreSqlExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void GetHashCode_GivenNullArgument_ThrowsArgumentNullException()
    {
        var comparer = new PostgreSqlExpressionComparer();

        Assert.That(() => comparer.GetHashCode(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetHashCode_GivenSameExpression_ReturnsSameValue()
    {
        const string input = "'test'";
        var comparer = new PostgreSqlExpressionComparer();

        var hashX = comparer.GetHashCode(input);
        var hashY = comparer.GetHashCode(input);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenSqlStringArgumentsDifferingOnlyByWrappingParens_ReturnsSameValue()
    {
        const string inputX = "('test')";
        const string inputY = "'test'";
        var comparer = new PostgreSqlExpressionComparer();

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenNumbersDifferingOnlyByWrappingParens_ReturnsSameValue()
    {
        const string inputX = "((123))";
        const string inputY = "123";
        var comparer = new PostgreSqlExpressionComparer();

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenComplexExpressionsDifferingByWhitespaceAndParenWrappedNumber_ReturnsSameValue()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "\"test_column_1\">length(left(\"test\",50))";
        var comparer = new PostgreSqlExpressionComparer();

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenDifferentExpressions_ReturnsDifferentValue()
    {
        const string inputX = "123";
        const string inputY = "456";
        var comparer = new PostgreSqlExpressionComparer();

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.Not.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenIgnoreCaseTextComparerAndExpressionsDifferingByCase_ReturnsSameValue()
    {
        const string inputX = "(\"test_column_1\" > length(left(\"test\", (50))))";
        const string inputY = "(\"TEST_Column_1\" > length(left(\"test\", (50))))";
        var comparer = new PostgreSqlExpressionComparer(StringComparer.OrdinalIgnoreCase);

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void GetHashCode_GivenIgnoreCaseStringComparerAndStringsDifferingByCase_ReturnsSameValue()
    {
        const string inputX = "(\"test_column_1\" > length(left('test', (50))))";
        const string inputY = "(\"test_column_1\" > length(left('TEST', (50))))";
        var comparer = new PostgreSqlExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

        var hashX = comparer.GetHashCode(inputX);
        var hashY = comparer.GetHashCode(inputY);

        Assert.That(hashX, Is.EqualTo(hashY));
    }

    [Test]
    public static void HashSet_GivenParenWrappedExpression_ContainsUnwrappedExpression()
    {
        // Exercises the actual IEqualityComparer<T> contract end-to-end: Equals("('test')", "'test'")
        // is true, so a hash-based container built with this comparer must find one via the other, not
        // just agree on Equals() and GetHashCode() in isolation.
        var comparer = new PostgreSqlExpressionComparer();
        var expressions = new HashSet<string>(comparer) { "('test')" };

        var containsUnwrapped = expressions.Contains("'test'");

        Assert.That(containsUnwrapped, Is.True);
    }
}
