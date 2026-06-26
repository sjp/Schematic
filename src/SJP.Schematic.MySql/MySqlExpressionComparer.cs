using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Parsing.Antlr;

namespace SJP.Schematic.MySql;

/// <summary>
/// An expression comparer for MySQL expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class MySqlExpressionComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlExpressionComparer"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="sqlStringComparer">The SQL string comparer.</param>
    public MySqlExpressionComparer(IEqualityComparer<string>? comparer = null, IEqualityComparer<string>? sqlStringComparer = null)
    {
        Comparer = comparer ?? StringComparer.Ordinal;
        SqlStringComparer = sqlStringComparer ?? StringComparer.Ordinal;
    }

    private IEqualityComparer<string> Comparer { get; }

    private IEqualityComparer<string> SqlStringComparer { get; }

    /// <summary>
    /// Determines whether the specified expressions are equal.
    /// </summary>
    /// <param name="x">The first expression to compare.</param>
    /// <param name="y">The second expression to compare.</param>
    /// <returns><see langword="true" /> if the specified expressions are equal; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="x"/> or <paramref name="y"/> are expressions that could not be parsed as a SQL expression.</exception>
    public bool Equals(string? x, string? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;

        var xTokens = Tokenize(x, nameof(x));
        var yTokens = Tokenize(y, nameof(y));

        var xCleanedTokens = StripWrappingParens(xTokens);
        var yCleanedTokens = StripWrappingParens(yTokens);

        if (xCleanedTokens.Count != yCleanedTokens.Count)
            return false;

        for (var i = 0; i < xCleanedTokens.Count; i++)
        {
            if (!TokensEqual(xCleanedTokens[i], yCleanedTokens[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a hash code for a SQL expression.
    /// </summary>
    /// <param name="obj">A SQL expression.</param>
    /// <returns>A hash code for a SQL expression, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public int GetHashCode(string obj) => Comparer.GetHashCode(obj);

    private static IReadOnlyList<IToken> Tokenize(string expression, string paramName)
    {
        try
        {
            return MySqlLexing.GetSignificantTokens(expression);
        }
        catch (MySqlSyntaxErrorException ex)
        {
            throw new ArgumentException($"Could not parse the '{paramName}' string as a SQL expression. Given: {expression}", paramName, ex);
        }
    }

    private bool TokensEqual(IToken x, IToken y)
    {
        if (x.Type != y.Type)
            return false;

        var comparer = IsStringLiteral(x.Type)
            ? SqlStringComparer
            : Comparer;

        return comparer.Equals(x.Text, y.Text);
    }

    private static IReadOnlyList<IToken> StripWrappingParens(IReadOnlyList<IToken> tokens)
    {
        if (tokens.Empty())
            return [];

        var result = new List<IToken>(tokens);

        var lastIndex = result.Count - 1;
        if (result[0].Type == MySQLLexer.OPEN_PAR_SYMBOL && result[lastIndex].Type == MySQLLexer.CLOSE_PAR_SYMBOL)
        {
            result.RemoveAt(lastIndex);
            result.RemoveAt(0);
        }

        for (var i = 0; i < result.Count; i++)
        {
            if (!IsNumber(result[i].Type))
                continue;

            // can't unwrap first char, no prefix to strip
            // same applies to last char
            if (i == 0 || i == (result.Count - 1))
                continue;

            var prevToken = result[i - 1];
            var nextToken = result[i + 1];
            if (prevToken.Type == MySQLLexer.OPEN_PAR_SYMBOL
                && nextToken.Type == MySQLLexer.CLOSE_PAR_SYMBOL)
            {
                // remove next first
                result.RemoveAt(i + 1);
                result.RemoveAt(i - 1);
                i--; // decrement because we've just removed a prefix
            }
        }

        return result;
    }

    private static bool IsStringLiteral(int tokenType)
        => tokenType is MySQLLexer.SINGLE_QUOTED_TEXT or MySQLLexer.NCHAR_TEXT or MySQLLexer.DOLLAR_QUOTED_STRING_TEXT;

    private static bool IsNumber(int tokenType)
        => tokenType is MySQLLexer.INT_NUMBER
            or MySQLLexer.LONG_NUMBER
            or MySQLLexer.ULONGLONG_NUMBER
            or MySQLLexer.DECIMAL_NUMBER
            or MySQLLexer.FLOAT_NUMBER
            or MySQLLexer.HEX_NUMBER
            or MySQLLexer.BIN_NUMBER;
}
