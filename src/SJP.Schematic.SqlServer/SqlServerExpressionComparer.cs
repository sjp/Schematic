using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// An expression comparer for SQL Server expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class SqlServerExpressionComparer : IEqualityComparer<string>
{
    private static readonly TSql150Parser _parser = new(true, SqlEngineType.All);

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

        using var xReader = new StringReader(x);
        var xTokens = _parser.GetTokenStream(xReader, out var xErrors);
        if (xErrors.Count > 0)
        {
            var parserErrorMessages = xErrors.Select(e => e.Message ?? string.Empty).Join(", ");
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {x}. Error: {parserErrorMessages}", nameof(x));
        }

        using var yReader = new StringReader(y);
        var yTokens = _parser.GetTokenStream(yReader, out var yErrors);
        if (yErrors.Count > 0)
        {
            var parserErrorMessages = yErrors.Select(e => e.Message ?? string.Empty).Join(", ");
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {y}. Error: {parserErrorMessages}", nameof(y));
        }

        var xWhitespaceRemoved = xTokens.Where(t => !IsWhitespace(t)).ToList();
        var yWhitespaceRemoved = yTokens.Where(t => !IsWhitespace(t)).ToList();

        var xCleanedTokens = StripWrappingParens(xWhitespaceRemoved);
        var yCleanedTokens = StripWrappingParens(yWhitespaceRemoved);

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

    private static IReadOnlyList<TSqlParserToken> StripWrappingParens(IReadOnlyList<TSqlParserToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        // copy to mutable result set
        if (tokens.Empty())
            return Array.Empty<TSqlParserToken>();

        var result = new List<TSqlParserToken>();
        result.AddRange(tokens);

        var lastIndex = tokens.Count - 1;
        if (result[0].TokenType == TSqlTokenType.LeftParenthesis && result[lastIndex].TokenType == TSqlTokenType.RightParenthesis)
        {
            result.RemoveAt(lastIndex);
            result.RemoveAt(0);
        }

        for (var i = 0; i < result.Count; i++)
        {
            var token = result[i];
            if (!IsNumeric(token))
                continue;

            // can't unwrap first char, no prefix to strip
            // same applies to last char
            if (i == 0 || i == (result.Count - 1))
                continue;

            var prevToken = result[i - 1];
            var nextToken = result[i + 1];
            if (prevToken.TokenType == TSqlTokenType.LeftParenthesis
                && nextToken.TokenType == TSqlTokenType.RightParenthesis)
            {
                // remove next first
                result.RemoveAt(i + 1);
                result.RemoveAt(i - 1);
                i--; // decrement because we've just removed a prefix
            }
        }

        return result;
    }
}