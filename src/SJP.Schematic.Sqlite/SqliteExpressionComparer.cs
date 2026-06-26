using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using SJP.Schematic.Sqlite.Parsing.Antlr;

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

        var xTokens = Tokenize(x, nameof(x));
        var yTokens = Tokenize(y, nameof(y));

        if (xTokens.Count != yTokens.Count)
            return false;

        for (var i = 0; i < xTokens.Count; i++)
        {
            if (!TokensEqual(xTokens[i], yTokens[i]))
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
            return SqliteLexing.GetSignificantTokens(expression);
        }
        catch (SqliteSyntaxErrorException ex)
        {
            throw new ArgumentException($"Could not parse the '{paramName}' string as a SQL expression. Given: {expression}", paramName, ex);
        }
    }

    private bool TokensEqual(IToken x, IToken y)
    {
        if (x.Type != y.Type)
            return false;

        var isStringToken = x.Type == SQLiteLexer.STRING_LITERAL;
        var comparer = isStringToken ? SqlStringComparer : Comparer;

        return comparer.Equals(x.Text, y.Text);
    }
}
