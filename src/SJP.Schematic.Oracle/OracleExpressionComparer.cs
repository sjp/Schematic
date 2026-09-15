using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using SJP.Schematic.Oracle.Parsing.Antlr;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An expression comparer for Oracle expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class OracleExpressionComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleExpressionComparer"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="sqlStringComparer">The SQL string comparer.</param>
    public OracleExpressionComparer(IEqualityComparer<string>? comparer = null, IEqualityComparer<string>? sqlStringComparer = null)
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
        if (ReferenceEquals(x, y))
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
            return OracleLexing.GetSignificantTokens(expression);
        }
        catch (OracleSyntaxErrorException ex)
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

    /// <summary>
    /// Removes a leading '(' and trailing ')' pair, then unwraps every number enclosed directly in
    /// parentheses, e.g. <c>(1)</c>.
    /// </summary>
    /// <remarks>
    /// The outer pair is removed whenever the first and last tokens are parentheses, whether or not
    /// they match each other. Each parenthesised number is unwrapped once only, so <c>x + ((1))</c>
    /// becomes <c>x + (1)</c>, not <c>x + 1</c>.
    /// </remarks>
    private static List<IToken> StripWrappingParens(IReadOnlyList<IToken> tokens)
    {
        var start = 0;
        var end = tokens.Count;
        if (end >= 2 && tokens[0].Type == PlSqlLexer.LEFT_PAREN && tokens[end - 1].Type == PlSqlLexer.RIGHT_PAREN)
        {
            start++;
            end--;
        }

        var result = new List<IToken>(end - start);
        for (var i = start; i < end; i++)
        {
            if (tokens[i].Type == PlSqlLexer.LEFT_PAREN
                && i + 2 < end
                && IsNumber(tokens[i + 1].Type)
                && tokens[i + 2].Type == PlSqlLexer.RIGHT_PAREN)
            {
                result.Add(tokens[i + 1]);
                i += 2;
                continue;
            }

            result.Add(tokens[i]);
        }

        return result;
    }

    private static bool IsStringLiteral(int tokenType)
        => tokenType is PlSqlLexer.CHAR_STRING or PlSqlLexer.NATIONAL_CHAR_STRING_LIT;

    private static bool IsNumber(int tokenType)
        => tokenType is PlSqlLexer.UNSIGNED_INTEGER or PlSqlLexer.APPROXIMATE_NUM_LIT;
}
