using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using Identifier = SJP.Schematic.Core.Identifier;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Provides expression dependencies for SQL Server expressions.
/// </summary>
public class SqlServerDependencyProvider : IDependencyProvider
{
    private static readonly TSql160Parser _parser = new(true, SqlEngineType.All);

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

        using var reader = new StringReader(expression);
        var tokens = _parser.GetTokenStream(reader, out var errors);

        var sqlErrors = errors ?? [];
        if (sqlErrors.Count > 0)
        {
            var parserErrorMessages = sqlErrors.Select(e => e.Message ?? string.Empty).Join(", ");
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {expression}. Error: {parserErrorMessages}", nameof(expression));
        }

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

    private static bool IsIdentifier(TSqlParserToken token) => token.TokenType == TSqlTokenType.Identifier || token.TokenType == TSqlTokenType.QuotedIdentifier;

    private static bool IsEven(int i) => i % 2 == 0;

    private static string GetIdentifierValue(TSqlParserToken token)
    {
        if (!IsIdentifier(token))
            throw new ArgumentException($"Expected a token of type {nameof(TSqlTokenType.Identifier)} or {nameof(TSqlTokenType.QuotedIdentifier)}. Received {token.TokenType}.", nameof(token));

        if (token.TokenType == TSqlTokenType.QuotedIdentifier)
        {
            // trim off any '[', ']', i.e. the reverse of $"[{identifier.Replace("]", "]]")}]"
            if (token.Text.StartsWith('[') && token.Text.EndsWith(']'))
            {
                var trimmed = token.Text[1..^1];
                return trimmed.Replace("]]", "]", StringComparison.Ordinal);
            }

            return token.Text;
        }

        return token.Text;
    }
}
