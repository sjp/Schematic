using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SJP.Schematic.Core;
using Identifier = SJP.Schematic.Core.Identifier;
using ScriptDom = Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Provides expression dependencies for SQL Server expressions.
/// </summary>
public class SqlServerDependencyProvider : IDependencyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDependencyProvider"/> class.
    /// </summary>
    /// <param name="comparer">An identifier comparer.</param>
    public SqlServerDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
    {
        Comparer = comparer ?? IdentifierComparer.Ordinal;
    }

    private IEqualityComparer<Identifier> Comparer { get; }

    /// <summary>
    /// Retrieves all dependencies for an expression.
    /// </summary>
    /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
    /// <param name="expression">A SQL expression that may contain dependent object names.</param>
    /// <returns>A collection of identifiers found in the expression.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or could not be parsed as a valid SQL expression.</exception>
    /// <remarks>This will also return unqualified identifiers, which may cause ambiguity between object names and column names. Additionally it may return other identifiers, such as aliases or type names.</remarks>
    public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var tokens = ScriptDomTokenizer.Tokenize(expression, nameof(expression));

        var result = new HashSet<Identifier>(Comparer);

        var whitespaceRemovedTokens = tokens
            .Where(t => !IsWhitespace(t))
            .ToList();

        var totalTokens = whitespaceRemovedTokens.Count;

        for (var i = 0; i < totalTokens; i++)
        {
            var token = whitespaceRemovedTokens[i];
            if (token == null || !IsIdentifier(token))
                continue;

            // Measure the dotted name starting here as `identifier (dot identifier)*`, stopping at the
            // first identifier that does not follow a dot. That identifier (e.g. an alias written
            // without AS, as in `Sales.Orders o`) then starts a name of its own.
            var nameEnd = i;
            while (nameEnd + 2 < totalTokens
                && whitespaceRemovedTokens[nameEnd + 1].TokenType == TSqlTokenType.Dot
                && IsIdentifier(whitespaceRemovedTokens[nameEnd + 2]))
            {
                nameEnd += 2;
            }

            var pieceCount = (nameEnd - i) / 2 + 1;
            var identifier = pieceCount switch
            {
                2 => Identifier.CreateQualifiedIdentifier(
                    GetIdentifierValue(token),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 2])),
                3 => Identifier.CreateQualifiedIdentifier(
                    GetIdentifierValue(token),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 2]),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 4])),
                4 => Identifier.CreateQualifiedIdentifier(
                    GetIdentifierValue(token),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 2]),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 4]),
                    GetIdentifierValue(whitespaceRemovedTokens[i + 6])),
                // a single name, or more than four parts: emit this token on its own
                _ => Identifier.CreateQualifiedIdentifier(GetIdentifierValue(token)),
            };

            if (!Comparer.Equals(identifier, objectName))
                result.Add(identifier);

            // skip past the remaining parts of a qualified name
            if (pieceCount is > 1 and <= 4)
                i = nameEnd;
        }

        return result;
    }

    private static bool IsWhitespace(TSqlParserToken token) => token.TokenType == TSqlTokenType.WhiteSpace || token.TokenType == TSqlTokenType.EndOfFile;

    private static bool IsIdentifier(TSqlParserToken token) =>
        token.TokenType == TSqlTokenType.Identifier
        || token.TokenType == TSqlTokenType.QuotedIdentifier
        // A double-quoted name lexes to this ambiguous type; under QUOTED_IDENTIFIER ON (how the
        // parser is configured) it is always a delimited identifier rather than a string literal.
        || token.TokenType == TSqlTokenType.AsciiStringOrQuotedIdentifier;

    private static string GetIdentifierValue(TSqlParserToken token)
    {
        if (!IsIdentifier(token))
            throw new ArgumentException($"Expected an identifier token. Received {token.TokenType}.", nameof(token));

        // A plain identifier carries no delimiters; quoted forms ([foo], "foo") are decoded,
        // auto-detecting the quoting style (and unescaping ]] / "") from the token text.
        return token.TokenType == TSqlTokenType.Identifier
            ? token.Text
            : ScriptDom.Identifier.DecodeIdentifier(token.Text, out _);
    }
}
