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

        // Identical expressions are the common case (e.g. comparing an expression against itself), and
        // this avoids lexing both sides twice for it.
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

        // Strip a single outermost wrapping pair, e.g. "(a + b)" -> "a + b". This is a one-off check
        // against the raw token boundaries, not applied recursively.
        var start = 0;
        var end = tokens.Count;
        if (tokens[0].Type == MySQLLexer.OPEN_PAR_SYMBOL && tokens[end - 1].Type == MySQLLexer.CLOSE_PAR_SYMBOL)
        {
            start = 1;
            end--;
        }

        // Collapse any parens that directly wrap a single number token, e.g. "(5)" -> "5". This is done
        // as a single forward pass, treating the output list as a stack: whenever a ')' is reached and the
        // last two emitted tokens are '(' followed by a number, both are popped and the number is re-added.
        // Because a token can be wrapped by more than one such pair (e.g. "((5))"), the same number can be
        // collapsed against several enclosing pairs in turn as the closing parens are encountered.
        var result = new List<IToken>(end - start);
        for (var i = start; i < end; i++)
        {
            var token = tokens[i];
            if (token.Type == MySQLLexer.CLOSE_PAR_SYMBOL
                && result.Count >= 2
                && IsNumber(result[^1].Type)
                && result[^2].Type == MySQLLexer.OPEN_PAR_SYMBOL)
            {
                var number = result[^1];
                result.RemoveAt(result.Count - 1);
                result.RemoveAt(result.Count - 1);
                result.Add(number);
            }
            else
            {
                result.Add(token);
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
