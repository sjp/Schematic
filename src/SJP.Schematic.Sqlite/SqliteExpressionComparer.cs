using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Sqlite.Parsing;
using Superpower.Model;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// An expression comparer for SQLite expressions.
/// </summary>
/// <seealso cref="IEqualityComparer{T}" />
public sealed class SqliteExpressionComparer : IEqualityComparer<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteExpressionComparer"/> class.
    /// </summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="sqlStringComparer">The SQL string comparer.</param>
    public SqliteExpressionComparer(IEqualityComparer<string>? comparer = null, IEqualityComparer<string>? sqlStringComparer = null)
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

        var tokenizer = new SqliteTokenizer();

        var xParseResult = tokenizer.TryTokenize(x);
        if (!xParseResult.HasValue)
            throw new ArgumentException($"Could not parse the '{ nameof(x) }' string as a SQL expression. Given: { x }", nameof(x));

        var yParseResult = tokenizer.TryTokenize(y);
        if (!yParseResult.HasValue)
            throw new ArgumentException($"Could not parse the '{ nameof(y) }' string as a SQL expression. Given: { y }", nameof(y));

        var xTokens = xParseResult.Value.ToList();
        var yTokens = yParseResult.Value.ToList();

        if (xTokens.Count != yTokens.Count)
            return false;

        for (var i = 0; i < xTokens.Count; i++)
        {
            var xToken = xTokens[i];
            var yToken = yTokens[i];

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

    private bool TokensEqual(Token<SqliteToken> x, Token<SqliteToken> y)
    {
        if (x.Kind != y.Kind)
            return false;

        var isStringX = IsStringToken(x);
        var isStringY = IsStringToken(y);
        if (isStringX ^ isStringY)
            return false;

        var comparer = isStringX ? SqlStringComparer : Comparer;

        var xString = x.ToStringValue();
        var yString = y.ToStringValue();

        return comparer.Equals(xString, yString);
    }

    private static bool IsStringToken(Token<SqliteToken> token)
    {
        if (token.Kind != SqliteToken.String)
            return false;

        var tokenValue = token.ToStringValue();
        var lastCharIndex = tokenValue.Length - 1;
        return tokenValue[0] == '\'' && tokenValue[lastCharIndex] == '\'';
    }
}
