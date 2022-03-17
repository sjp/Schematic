using System;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteExpressionComparerTests
{
    [Test]
    public static void Ctor_GivenNullComparer_CreatesWithoutError()
    {
        var argComparer = StringComparer.Ordinal;
        Assert.That(() => new SqliteExpressionComparer(sqlStringComparer: argComparer), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNullSqlStringComparer_CreatesWithoutError()
    {
        var argComparer = StringComparer.Ordinal;
        Assert.That(() => new SqliteExpressionComparer(argComparer), Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqliteExpressionComparer(), Throws.Nothing);
    }

    [Test]
    public static void Equals_GivenEqualSqlStringArguments_ReturnsTrue()
    {
        const string input = "'test'";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentSqlStringArguments_ReturnsFalse()
    {
        const string inputX = "'test'";
        const string inputY = "'alternative'";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualDateArguments_ReturnsTrue()
    {
        const string input = "getdate()";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentDateArguments_ReturnsFalse()
    {
        const string inputX = "getdate()";
        const string inputY = "getutcdate()";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualNumberArguments_ReturnsTrue()
    {
        const string input = "123";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentNumberArguments_ReturnsFalse()
    {
        const string inputX = "123";
        const string inputY = "456";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenEqualComplexExpressions_ReturnsTrue()
    {
        const string input = "[test_column_1] > len(left([test], 50))";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(input, input);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDifferentComplexExpressions_ReturnsFalse()
    {
        const string inputX = "[test_column_1] > len(left([test], 50))";
        const string inputY = "[test_column_2] < len(left([test], 50))";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsFalse()
    {
        const string inputX = "([test_column_1] > len(left([test], (50))))";
        const string inputY = "([TEST_Column_1] > len(left([test], (50))))";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentCase_ReturnsTrue()
    {
        const string inputX = "([test_column_1] > len(left([test], (50))))";
        const string inputY = "([TEST_Column_1] > len(left([test], (50))))";
        var comparer = new SqliteExpressionComparer(StringComparer.OrdinalIgnoreCase);

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }

    [Test]
    public static void Equals_GivenDefaultTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsFalse()
    {
        const string inputX = "([test_column_1] > len(left('test', (50))))";
        const string inputY = "([test_column_1] > len(left('TEST', (50))))";
        var comparer = new SqliteExpressionComparer();

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.False);
    }

    [Test]
    public static void Equals_GivenIgnoreCaseTextComparerAndEqualComplexExpressionsButDifferentStringCase_ReturnsTrue()
    {
        const string inputX = "([test_column_1] > len(left('test', (50))))";
        const string inputY = "([test_column_1] > len(left('TEST', (50))))";
        var comparer = new SqliteExpressionComparer(sqlStringComparer: StringComparer.OrdinalIgnoreCase);

        var equals = comparer.Equals(inputX, inputY);

        Assert.That(equals, Is.True);
    }
}
