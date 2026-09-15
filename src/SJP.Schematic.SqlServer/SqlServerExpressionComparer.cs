using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// An expression comparer for SQL Server expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class SqlServerExpressionComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerExpressionComparer"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="sqlStringComparer">The SQL string comparer.</param>
    public SqlServerExpressionComparer(IEqualityComparer<string>? comparer = null, IEqualityComparer<string>? sqlStringComparer = null)
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

        var xTokens = ScriptDomTokenizer.Tokenize(x, nameof(x));
        var yTokens = ScriptDomTokenizer.Tokenize(y, nameof(y));

        var xCleanedTokens = RemoveWhitespace(xTokens);
        var yCleanedTokens = RemoveWhitespace(yTokens);

        StripWrappingParens(xCleanedTokens);
        StripWrappingParens(yCleanedTokens);

        if (xCleanedTokens.Count != yCleanedTokens.Count)
            return false;

        for (var i = 0; i < xCleanedTokens.Count; i++)
        {
            var xToken = xCleanedTokens[i];
            var yToken = yCleanedTokens[i];

            if (!TokensEqual(xToken, yToken))
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

    private bool TokensEqual(TSqlParserToken x, TSqlParserToken y)
    {
        if (x.TokenType != y.TokenType)
            return false;

        var comparer = x.TokenType == TSqlTokenType.AsciiStringLiteral || x.TokenType == TSqlTokenType.UnicodeStringLiteral
            ? SqlStringComparer
            : Comparer;

        var xString = x.Text;
        var yString = y.Text;

        return comparer.Equals(xString, yString);
    }

    private static bool IsWhitespace(TSqlParserToken token) => token.TokenType == TSqlTokenType.WhiteSpace || token.TokenType == TSqlTokenType.EndOfFile;

    private static bool IsNumeric(TSqlParserToken token) => token.TokenType == TSqlTokenType.Integer
        || token.TokenType == TSqlTokenType.Numeric
        || token.TokenType == TSqlTokenType.Double;

    private static List<TSqlParserToken> RemoveWhitespace(IList<TSqlParserToken> tokens)
    {
        var result = new List<TSqlParserToken>(tokens.Count);
        foreach (var token in tokens)
        {
            if (!IsWhitespace(token))
                result.Add(token);
        }

        return result;
    }

    /// <summary>
    /// Removes a leading '(' and trailing ')' pair, then unwraps every number enclosed directly in
    /// parentheses, e.g. <c>(1)</c>. The tokens are modified in place.
    /// </summary>
    /// <remarks>
    /// The outer pair is removed whenever the first and last tokens are parentheses, whether or not
    /// they match each other. Each parenthesised number is unwrapped once only, so <c>x + ((1))</c>
    /// becomes <c>x + (1)</c>, not <c>x + 1</c>.
    /// </remarks>
    private static void StripWrappingParens(List<TSqlParserToken> tokens)
    {
        var count = tokens.Count;
        var start = 0;
        var end = count;
        if (count >= 2 && tokens[0].TokenType == TSqlTokenType.LeftParenthesis && tokens[count - 1].TokenType == TSqlTokenType.RightParenthesis)
        {
            start++;
            end--;
        }

        // The write position never passes the read position, so tokens still to be read are never overwritten.
        var write = 0;
        for (var read = start; read < end; read++)
        {
            var token = tokens[read];
            if (token.TokenType == TSqlTokenType.LeftParenthesis
                && read + 2 < end
                && IsNumeric(tokens[read + 1])
                && tokens[read + 2].TokenType == TSqlTokenType.RightParenthesis)
            {
                token = tokens[read + 1];
                read += 2;
            }

            tokens[write++] = token;
        }

        tokens.RemoveRange(write, count - write);
    }
}