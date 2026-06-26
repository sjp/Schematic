using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing.Antlr;
using static SJP.Schematic.Sqlite.Parsing.Antlr.AntlrParsingExtensions;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// Provides expression dependencies for SQLite expressions.
/// </summary>
/// <seealso cref="IDependencyProvider" />
public sealed class SqliteDependencyProvider : IDependencyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDependencyProvider"/> class.
    /// </summary>
    /// <param name="comparer">An identifier comparer.</param>
    public SqliteDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
    {
        Comparer = comparer ?? IdentifierComparer.OrdinalIgnoreCase;
    }

    private IEqualityComparer<Identifier> Comparer { get; }

    /// <summary>
    /// Retrieves all dependencies for an expression.
    /// </summary>
    /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
    /// <param name="expression">A SQL expression that may contain dependent object names.</param>
    /// <returns>A collection of identifiers found in the expression.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> is <see langword="null" />. Alternatively, if <paramref name="expression"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> could not be tokenized as a valid SQL expression.</exception>
    /// <remarks>This will also return unqualified identifiers, which may cause ambiguity between object names and column names. Additionally it may return other identifiers, such as aliases or type names.</remarks>
    public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var tokens = GetSignificantTokens(expression);

        var seen = new HashSet<Identifier>(Comparer);
        var result = new List<Identifier>();

        var i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i].Type != SQLiteLexer.IDENTIFIER)
            {
                i++;
                continue;
            }

            // Stitch together qualified names of the form IDENTIFIER (DOT IDENTIFIER)*,
            // e.g. schema.table or schema.table.column.
            var parts = new List<string> { UnquoteIdentifier(tokens[i].Text) };
            while (i + 2 < tokens.Count
                && tokens[i + 1].Type == SQLiteLexer.DOT
                && tokens[i + 2].Type == SQLiteLexer.IDENTIFIER)
            {
                parts.Add(UnquoteIdentifier(tokens[i + 2].Text));
                i += 2;
            }

            var identifier = BuildIdentifier(parts);
            if (!Comparer.Equals(identifier, objectName) && seen.Add(identifier))
                result.Add(identifier);

            i++;
        }

        return result;
    }

    private static IReadOnlyList<IToken> GetSignificantTokens(string expression)
    {
        var inputStream = new AntlrInputStream(expression);
        var lexer = new SQLiteLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        try
        {
            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();

            // Comments and whitespace are emitted on the hidden channel; restrict to the
            // default channel so that qualified-name parts remain adjacent.
            return tokenStream.GetTokens()
                .Where(static t => t.Channel == Lexer.DefaultTokenChannel && t.Type != TokenConstants.EOF)
                .ToList();
        }
        catch (SqliteSyntaxErrorException ex)
        {
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {expression}", nameof(expression), ex);
        }
    }

    private static Identifier BuildIdentifier(IReadOnlyList<string> parts)
    {
        // Identifier supports at most four levels (server.database.schema.local); keep the
        // right-most parts if somehow more were found.
        var relevant = parts.Count > 4
            ? parts.Skip(parts.Count - 4).ToList()
            : parts;

        return relevant.Count switch
        {
            1 => Identifier.CreateQualifiedIdentifier(relevant[0]),
            2 => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1]),
            3 => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1], relevant[2]),
            _ => Identifier.CreateQualifiedIdentifier(relevant[0], relevant[1], relevant[2], relevant[3]),
        };
    }
}
