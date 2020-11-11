using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Parsing;
using Superpower.Model;

namespace SJP.Schematic.SqlServer
{
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

            var tokenizer = new SqlServerTokenizer();

            var xParseResult = tokenizer.TryTokenize(x);
            if (!xParseResult.HasValue)
                throw new ArgumentException($"Could not parse the '{ nameof(x) }' string as a SQL expression. Given: { x }", nameof(x));

            var yParseResult = tokenizer.TryTokenize(y);
            if (!yParseResult.HasValue)
                throw new ArgumentException($"Could not parse the '{ nameof(y) }' string as a SQL expression. Given: { y }", nameof(y));

            var xTokens = xParseResult.Value.ToList();
            var yTokens = yParseResult.Value.ToList();

            var xCleanedTokens = StripWrappingParens(xTokens);
            var yCleanedTokens = StripWrappingParens(yTokens);

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

        private bool TokensEqual(Token<SqlServerToken> x, Token<SqlServerToken> y)
        {
            if (x.Kind != y.Kind)
                return false;

            var comparer = x.Kind == SqlServerToken.String
                ? SqlStringComparer
                : Comparer;

            var xString = x.ToStringValue();
            var yString = y.ToStringValue();

            return comparer.Equals(xString, yString);
        }

        private static IReadOnlyList<Token<SqlServerToken>> StripWrappingParens(IReadOnlyList<Token<SqlServerToken>> tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            // copy to mutable result set
            if (tokens.Empty())
                return Array.Empty<Token<SqlServerToken>>();

            var result = new List<Token<SqlServerToken>>();
            foreach (var token in tokens)
                result.Add(token);

            var lastIndex = tokens.Count - 1;
            if (result[0].Kind == SqlServerToken.LParen && result[lastIndex].Kind == SqlServerToken.RParen)
            {
                result.RemoveAt(lastIndex);
                result.RemoveAt(0);
            }

            for (var i = 0; i < result.Count; i++)
            {
                var token = result[i];
                if (token.Kind != SqlServerToken.Number)
                    continue;

                // can't unwrap first char, no prefix to strip
                // same applies to last char
                if (i == 0 || i == (result.Count - 1))
                    continue;

                var prevToken = result[i - 1];
                var nextToken = result[i + 1];
                if (prevToken.Kind == SqlServerToken.LParen
                    && nextToken.Kind == SqlServerToken.RParen)
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
}
