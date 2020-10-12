using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal sealed class SqlIdentifier
    {
        public SqlIdentifier(Token<SqliteToken> schemaToken, Token<SqliteToken> localNameToken)
        {
            if (schemaToken.Kind != SqliteToken.Identifier || schemaToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided schema token must be an identifier token. Instead given: " + schemaToken.Kind.ToString(), nameof(schemaToken));
            if (localNameToken.Kind != SqliteToken.Identifier || localNameToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided local name token must be an identifier token. Instead given: " + localNameToken.Kind.ToString(), nameof(localNameToken));

            var schemaName = UnwrapIdentifier(schemaToken.ToStringValue());
            var localName = UnwrapIdentifier(localNameToken.ToStringValue());

            Value = Identifier.CreateQualifiedIdentifier(schemaName, localName);
        }

        public SqlIdentifier(Token<SqliteToken> token)
        {
            if (token.Kind != SqliteToken.Identifier || token.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided token must be an identifier token. Instead given: " + token.Kind.ToString(), nameof(token));

            Value = UnwrapIdentifier(token.ToStringValue());
        }

        public Identifier Value { get; }

        private static string UnwrapIdentifier(string identifier)
        {
            if (identifier.StartsWith("\"", StringComparison.Ordinal))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("\"\"", "\"", StringComparison.Ordinal);
            }
            else if (identifier.StartsWith("[", StringComparison.Ordinal))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("]]", "]", StringComparison.Ordinal);
            }
            else if (identifier.StartsWith("`", StringComparison.Ordinal))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("``", "`", StringComparison.Ordinal);
            }
            else
            {
                return identifier;
            }
        }

        private static string TrimWrappingChars(string input) => input[1..^1];
    }
}
