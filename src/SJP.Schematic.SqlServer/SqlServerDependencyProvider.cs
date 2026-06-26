using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <see langword="null" />. Alternatively, if <paramref name="expression"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> could not be parsed as a valid SQL expression.</exception>
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

            var remainder = whitespaceRemovedTokens
                .Skip(i)
                .TakeWhile(t => IsIdentifier(t) || t.TokenType == TSqlTokenType.Dot)
                .ToList();

            if (remainder.Count > 1 && TryGetQualifiedIdentifier(remainder, out var qualifiedIdentifier))
            {
                if (!Comparer.Equals(qualifiedIdentifier, objectName))
                    result.Add(qualifiedIdentifier);

                // because we have skipped past the current token, we need to advance the index
                i += remainder.Count - 1;
            }
            else
            {
                var identifier = Identifier.CreateQualifiedIdentifier(GetIdentifierValue(token));
                if (!Comparer.Equals(identifier, objectName))
                    result.Add(identifier);
            }
        }

        return result;
    }

    private static bool TryGetQualifiedIdentifier(IEnumerable<TSqlParserToken> tokens, [NotNullWhen(true)] out Identifier? qualifiedIdentifier)
    {
        qualifiedIdentifier = null;

        var isQualifiedIdentifierSequence = tokens
            .Select((t, i) =>
                (IsEven(i) && IsIdentifier(t))
                || (!IsEven(i) && t.TokenType == TSqlTokenType.Dot))
            .All(x => x);
        if (isQualifiedIdentifierSequence)
        {
            var pieces = tokens
                .Where(IsIdentifier)
                .Select(GetIdentifierValue)
                .ToList();
            qualifiedIdentifier = pieces switch
            {
                { Count: 1 } => Identifier.CreateQualifiedIdentifier(pieces[0]),
                { Count: 2 } => Identifier.CreateQualifiedIdentifier(pieces[0], pieces[1]),
                { Count: 3 } => Identifier.CreateQualifiedIdentifier(pieces[0], pieces[1], pieces[2]),
                { Count: 4 } => Identifier.CreateQualifiedIdentifier(pieces[0], pieces[1], pieces[2], pieces[3]),
                _ => null,
            };

            return qualifiedIdentifier != null;
        }

        return false;
    }

    private static bool IsWhitespace(TSqlParserToken token) => token.TokenType == TSqlTokenType.WhiteSpace || token.TokenType == TSqlTokenType.EndOfFile;

    private static bool IsIdentifier(TSqlParserToken token) =>
        token.TokenType == TSqlTokenType.Identifier
        || token.TokenType == TSqlTokenType.QuotedIdentifier
        // A double-quoted name lexes to this ambiguous type; under QUOTED_IDENTIFIER ON (how the
        // parser is configured) it is always a delimited identifier rather than a string literal.
        || token.TokenType == TSqlTokenType.AsciiStringOrQuotedIdentifier;

    private static bool IsEven(int i) => i % 2 == 0;

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
