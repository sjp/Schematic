using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Parsing.Antlr;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// An expression comparer for PostgreSQL expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class PostgreSqlExpressionComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlExpressionComparer"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="sqlStringComparer">The SQL string comparer.</param>
    public PostgreSqlExpressionComparer(IEqualityComparer<string>? comparer = null, IEqualityComparer<string>? sqlStringComparer = null)
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

        // Identical expressions are the common case (e.g. comparing an expression against itself),
        // and this avoids lexing both sides twice for it.
        if (string.Equals(x, y, StringComparison.Ordinal))
            return true;

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
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is an expression that could not be parsed as a SQL expression.</exception>
    /// <remarks>
    /// The hash is computed over the same normalized token sequence that <see cref="Equals(string, string)"/>
    /// compares, so expressions differing only in whitespace, comments or redundant wrapping parentheses hash
    /// to the same value. This requires lexing <paramref name="obj"/>, which is more expensive than hashing the
    /// raw string, but is required for the <see cref="IEqualityComparer{T}"/> contract to hold.
    /// </remarks>
    public int GetHashCode(string obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var tokens = StripWrappingParens(Tokenize(obj, nameof(obj)));

        var hashCode = new HashCode();
        foreach (var token in tokens)
        {
            hashCode.Add(token.Type);

            var comparer = IsStringLiteral(token.Type)
                ? SqlStringComparer
                : Comparer;
            hashCode.Add(comparer.GetHashCode(token.Text));
        }

        return hashCode.ToHashCode();
    }

    private static IReadOnlyList<IToken> Tokenize(string expression, string paramName)
    {
        try
        {
            return PostgreSqlLexing.GetSignificantTokens(expression);
        }
        catch (PostgreSqlSyntaxErrorException ex)
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

        // Strip a single outermost wrapping pair, e.g. "(a + b)" -> "a + b". A one-off check against
        // the raw token boundaries, not applied recursively.
        var start = 0;
        var end = tokens.Count;
        if (tokens[0].Type == PostgreSQLLexer.OPEN_PAREN && tokens[end - 1].Type == PostgreSQLLexer.CLOSE_PAREN)
        {
            start++;
            end--;
        }

        // Collapse a paren pair that directly wraps a single number token, e.g. "(5)" -> "5". This is
        // deliberately non-cascading, matching the original implementation: once a number at position i
        // has been unwrapped, the scan resumes just past it, so "((5))" collapses fully to "5" but a
        // triple-wrapped "(((5)))" is left as "(5)" -- the newly-exposed outer pair around the already
        // -unwrapped number is not re-examined.
        var result = new List<IToken>(end - start);
        for (var i = start; i < end; i++)
        {
            if (tokens[i].Type == PostgreSQLLexer.OPEN_PAREN
                && i + 2 < end
                && IsNumber(tokens[i + 1].Type)
                && tokens[i + 2].Type == PostgreSQLLexer.CLOSE_PAREN)
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
        => tokenType is PostgreSQLLexer.StringConstant
            or PostgreSQLLexer.EscapeStringConstant
            or PostgreSQLLexer.UnicodeEscapeStringConstant
            or PostgreSQLLexer.BinaryStringConstant
            or PostgreSQLLexer.HexadecimalStringConstant
            or PostgreSQLLexer.BeginDollarStringConstant
            or PostgreSQLLexer.DollarText
            or PostgreSQLLexer.EndDollarStringConstant;

    private static bool IsNumber(int tokenType)
        => tokenType is PostgreSQLLexer.Integral
            or PostgreSQLLexer.BinaryIntegral
            or PostgreSQLLexer.OctalIntegral
            or PostgreSQLLexer.HexadecimalIntegral
            or PostgreSQLLexer.Numeric;
}
