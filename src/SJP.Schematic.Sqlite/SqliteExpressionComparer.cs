using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Parsing;
using Superpower.Model;

namespace SJP.Schematic.Sqlite
{
    public sealed class SqliteExpressionComparer : IEqualityComparer<string>
    {
        public SqliteExpressionComparer(IEqualityComparer<string> comparer = null, IEqualityComparer<string> sqlStringComparer = null)
        {
            Comparer = comparer ?? StringComparer.Ordinal;
            SqlStringComparer = sqlStringComparer ?? StringComparer.Ordinal;
        }

        private IEqualityComparer<string> Comparer { get; }

        private IEqualityComparer<string> SqlStringComparer { get; }

        public bool Equals(string x, string y)
        {
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) ^ ReferenceEquals(y, null))
                return false;

            var tokenizer = new SqliteTokenizer();

            var xParseResult = tokenizer.TryTokenize(x);
            if (!xParseResult.HasValue)
                throw new ArgumentException($"Could not parse the '{ nameof(x) }' string as a SQL expression. Given: { x }", nameof(x));

            var yParseResult = tokenizer.TryTokenize(y);
            if (!yParseResult.HasValue)
                throw new ArgumentException($"Could not parse the '{ nameof(y) }' string as a SQL expression. Given: { y }", nameof(y));

            var xTokens = xParseResult.Value.ToReadOnlyList();
            var yTokens = yParseResult.Value.ToReadOnlyList();

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
}
