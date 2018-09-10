using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Oracle.Parsing
{
    internal sealed class SqlIdentifier
    {
        public SqlIdentifier(Token<OracleToken> serverToken, Token<OracleToken> databaseToken, Token<OracleToken> schemaToken, Token<OracleToken> localNameToken)
        {
            if (serverToken.Kind != OracleToken.Identifier || serverToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided serverToken token must be an identifier token. Instead given: " + serverToken.Kind.ToString(), nameof(serverToken));
            if (databaseToken.Kind != OracleToken.Identifier || databaseToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided database token must be an identifier token. Instead given: " + databaseToken.Kind.ToString(), nameof(databaseToken));
            if (schemaToken.Kind != OracleToken.Identifier || schemaToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided schema token must be an identifier token. Instead given: " + schemaToken.Kind.ToString(), nameof(schemaToken));
            if (localNameToken.Kind != OracleToken.Identifier || localNameToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided local name token must be an identifier token. Instead given: " + localNameToken.Kind.ToString(), nameof(localNameToken));

            var serverName = UnwrapIdentifier(serverToken.ToStringValue());
            var databaseName = UnwrapIdentifier(databaseToken.ToStringValue());
            var schemaName = UnwrapIdentifier(schemaToken.ToStringValue());
            var localName = UnwrapIdentifier(localNameToken.ToStringValue());

            Value = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
        }

        public SqlIdentifier(Token<OracleToken> databaseToken, Token<OracleToken> schemaToken, Token<OracleToken> localNameToken)
        {
            if (databaseToken.Kind != OracleToken.Identifier || databaseToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided database token must be an identifier token. Instead given: " + databaseToken.Kind.ToString(), nameof(databaseToken));
            if (schemaToken.Kind != OracleToken.Identifier || schemaToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided schema token must be an identifier token. Instead given: " + schemaToken.Kind.ToString(), nameof(schemaToken));
            if (localNameToken.Kind != OracleToken.Identifier || localNameToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided local name token must be an identifier token. Instead given: " + localNameToken.Kind.ToString(), nameof(localNameToken));

            var databaseName = UnwrapIdentifier(databaseToken.ToStringValue());
            var schemaName = UnwrapIdentifier(schemaToken.ToStringValue());
            var localName = UnwrapIdentifier(localNameToken.ToStringValue());

            Value = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
        }

        public SqlIdentifier(Token<OracleToken> schemaToken, Token<OracleToken> localNameToken)
        {
            if (schemaToken.Kind != OracleToken.Identifier || schemaToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided schema token must be an identifier token. Instead given: " + schemaToken.Kind.ToString(), nameof(schemaToken));
            if (localNameToken.Kind != OracleToken.Identifier || localNameToken.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided local name token must be an identifier token. Instead given: " + localNameToken.Kind.ToString(), nameof(localNameToken));

            var schemaName = UnwrapIdentifier(schemaToken.ToStringValue());
            var localName = UnwrapIdentifier(localNameToken.ToStringValue());

            Value = Identifier.CreateQualifiedIdentifier(schemaName, localName);
        }

        public SqlIdentifier(Token<OracleToken> token)
        {
            if (token.Kind != OracleToken.Identifier || token.ToStringValue().IsNullOrWhiteSpace())
                throw new ArgumentException("The provided token must be an identifier token. Instead given: " + token.Kind.ToString(), nameof(token));

            Value = UnwrapIdentifier(token.ToStringValue());
        }

        public Identifier Value { get; }

        private static string UnwrapIdentifier(string identifier)
        {
            if (identifier.StartsWith("\""))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("\"\"", "\"");
            }
            else if (identifier.StartsWith("["))
            {
                var result = TrimWrappingChars(identifier);
                return result.Replace("]]", "]");
            }
            else
            {
                return identifier;
            }
        }

        private static string TrimWrappingChars(string input) => input.Substring(1, input.Length - 2);
    }
}
