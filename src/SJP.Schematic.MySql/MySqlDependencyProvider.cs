using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Parsing.Antlr;
using static SJP.Schematic.MySql.Parsing.Antlr.AntlrParsingExtensions;

namespace SJP.Schematic.MySql;

/// <summary>
/// A dependency provider for MySQL database objects.
/// </summary>
/// <seealso cref="IDependencyProvider" />
public sealed class MySqlDependencyProvider : IDependencyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDependencyProvider"/> class.
    /// </summary>
    /// <param name="comparer">A comparer.</param>
    public MySqlDependencyProvider(IEqualityComparer<Identifier>? comparer = null)
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
            if (!IsIdentifier(tokens[i]))
            {
                i++;
                continue;
            }

            // Stitch together qualified names of the form identifier (DOT identifier)*,
            // e.g. schema.table or schema.table.column.
            var parts = new List<string> { UnquoteIdentifier(tokens[i].Text) };
            while (i + 2 < tokens.Count
                && tokens[i + 1].Type == MySQLLexer.DOT_SYMBOL
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

        return result;
    }

    private static IReadOnlyList<IToken> GetSignificantTokens(string expression)
    {
        try
        {
            return MySqlLexing.GetSignificantTokens(expression);
        }
        catch (MySqlSyntaxErrorException ex)
        {
            throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: {expression}", nameof(expression), ex);
        }
    }
}
