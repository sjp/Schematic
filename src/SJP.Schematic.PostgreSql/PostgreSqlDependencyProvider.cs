using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Parsing.Antlr;
using static SJP.Schematic.PostgreSql.Parsing.Antlr.AntlrParsingExtensions;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A dependency provider for PostgreSQL database objects.
/// </summary>
/// <seealso cref="IDependencyProvider" />
public sealed class PostgreSqlDependencyProvider : IDependencyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDependencyProvider"/> class.
    /// </summary>
    /// <param name="comparer">A comparer.</param>
    public PostgreSqlDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
    {
        Comparer = comparer ?? IdentifierComparer.Ordinal;
    }

    // How many levels of dollar-quoted text are read as SQL, counting the outermost body as one.
    private const int MaxDollarQuotedDepth = 2;

    private IEqualityComparer<Identifier> Comparer { get; }

    /// <summary>
    /// Retrieves all dependencies for an expression.
    /// </summary>
    /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
    /// <param name="expression">A SQL expression that may contain dependent object names.</param>
    /// <returns>A collection of identifiers found in the expression.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> or <paramref name="expression"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="expression"/> is empty or whitespace, or could not be tokenized as a valid SQL expression.</exception>
    /// <remarks>
    /// <para>This will also return unqualified identifiers, which may cause ambiguity between object names and column names. Additionally it may return other identifiers, such as aliases or type names.</para>
    /// <para>Dollar-quoted text is read as SQL too, so that a routine definition reports the objects its body names rather than only those in its signature.</para>
    /// </remarks>
    public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var seen = new HashSet<Identifier>(Comparer);
        var result = new List<Identifier>();
        AddDependencies(GetSignificantTokens(expression), objectName, seen, result, depth: 0);

        return result;
    }

    // Collects the identifiers in one run of tokens. Dollar-quoted text is lexed and collected in
    // turn (see CollectDollarQuotedText), so 'depth' bounds how far that nesting is followed.
    private void AddDependencies(IReadOnlyList<IToken> tokens, Identifier objectName, HashSet<Identifier> seen, List<Identifier> result, int depth)
    {
        var parts = new List<string>(4);

        var i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i].Type == PostgreSQLLexer.BeginDollarStringConstant)
            {
                i = CollectDollarQuotedText(tokens, i, objectName, seen, result, depth);
                continue;
            }

            if (!IsIdentifier(tokens[i]))
            {
                i++;
                continue;
            }

            // Stitch together qualified names of the form identifier (DOT identifier)*,
            // e.g. schema.table or schema.table.column. The buffer is reused across identifier
            // occurrences -- almost every name is 1 or 2 parts -- instead of allocating a fresh
            // List for each one.
            parts.Clear();
            parts.Add(UnquoteIdentifier(tokens[i].Text));
            while (i + 2 < tokens.Count
                && tokens[i + 1].Type == PostgreSQLLexer.DOT
                && IsIdentifier(tokens[i + 2]))
            {
                parts.Add(UnquoteIdentifier(tokens[i + 2].Text));
                i += 2;
            }

            var identifier = BuildIdentifier(parts);
            if (!Comparer.Equals(identifier, objectName) && seen.Add(identifier))
                result.Add(identifier);

            i++;
        }
    }

    /// <summary>
    /// Collects the identifiers inside the dollar-quoted text starting at <paramref name="start"/>
    /// and returns the index of the token after it.
    /// </summary>
    /// <remarks>
    /// A routine body arrives as dollar-quoted text, which the lexer keeps whole, so the objects it
    /// names are only found by lexing the text in turn. The text is not always SQL -- a body may be
    /// written in any language the server has installed -- so it is lexed on a best-effort basis:
    /// what does not tokenize is skipped rather than failing the definition around it. Identifiers
    /// found this way are as approximate as the rest: a local variable named like a table reads as
    /// a reference to it.
    /// </remarks>
    private int CollectDollarQuotedText(IReadOnlyList<IToken> tokens, int start, Identifier objectName, HashSet<Identifier> seen, List<Identifier> result, int depth)
    {
        // The text between the opening and closing tag arrives as one or more tokens, split
        // wherever it contains a '$' that does not close it.
        var i = start + 1;
        var textStart = i;
        while (i < tokens.Count && tokens[i].Type == PostgreSQLLexer.DollarText)
            i++;

        var textEnd = i;
        if (i < tokens.Count && tokens[i].Type == PostgreSQLLexer.EndDollarStringConstant)
            i++;

        // A body may itself quote a body, e.g. a function that creates another one. Following that
        // nesting for ever would let a definition dictate how much work this does, so it stops at a
        // depth beyond which text is far more likely to be data than a definition.
        if (depth >= MaxDollarQuotedDepth || textEnd == textStart)
            return i;

        var text = GetText(tokens, textStart, textEnd);
        if (string.IsNullOrWhiteSpace(text))
            return i;

        AddDependencies(PostgreSqlLexing.GetSignificantTokensSafe(text), objectName, seen, result, depth + 1);

        return i;
    }

    // The text of tokens [start, end), which is almost always a single token.
    private static string GetText(IReadOnlyList<IToken> tokens, int start, int end)
    {
        if (end - start == 1)
            return tokens[start].Text;

        var builder = new StringBuilder();
        for (var i = start; i < end; i++)
            builder.Append(tokens[i].Text);

        return builder.ToString();
    }

    private static IReadOnlyList<IToken> GetSignificantTokens(string expression)
    {
        try
        {
            return PostgreSqlLexing.GetSignificantTokens(expression);
        }
        catch (PostgreSqlSyntaxErrorException ex)
        {
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {expression}", nameof(expression), ex);
        }
    }
}
