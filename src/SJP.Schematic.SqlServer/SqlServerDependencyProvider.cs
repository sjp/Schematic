using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

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

        var result = new HashSet<Identifier>(Comparer);
        var scanner = new SqlServerNameScanner(expression, nameof(expression));

        // The parts of the dotted name being read, `identifier (dot identifier)*`. An identifier
        // that does not follow a dot (e.g. an alias written without AS, as in `Sales.Orders o`)
        // ends the name and starts one of its own. Comments, like any other token, end it too.
        Span<SqlServerNameToken> parts = stackalloc SqlServerNameToken[MaxNameParts];
        var partCount = 0;
        var afterDot = false;

        while (scanner.MoveNext())
        {
            var token = scanner.Current;
            if (token.IsIdentifier)
            {
                if (partCount > 0 && !afterDot)
                {
                    AddName(result, objectName, scanner, parts[..partCount]);
                    partCount = 0;
                }

                // a name of more than four parts is not an object name, so its leading parts are
                // emitted on their own until the last four remain
                if (partCount == MaxNameParts)
                {
                    AddName(result, objectName, scanner, parts[..1]);
                    parts[1..].CopyTo(parts);
                    partCount--;
                }

                parts[partCount++] = token;
                afterDot = false;
            }
            else if (token.Kind == SqlServerNameTokenKind.Dot && partCount > 0 && !afterDot)
            {
                afterDot = true;
            }
            else
            {
                if (partCount > 0)
                    AddName(result, objectName, scanner, parts[..partCount]);
                partCount = 0;
                afterDot = false;
            }
        }

        if (partCount > 0)
            AddName(result, objectName, scanner, parts[..partCount]);

        return result;
    }

    private const int MaxNameParts = 4;

    private void AddName(HashSet<Identifier> result, Identifier objectName, SqlServerNameScanner scanner, ReadOnlySpan<SqlServerNameToken> parts)
    {
        var identifier = parts.Length switch
        {
            1 => Identifier.CreateQualifiedIdentifier(scanner.GetIdentifierValue(parts[0])),
            2 => Identifier.CreateQualifiedIdentifier(
                scanner.GetIdentifierValue(parts[0]),
                scanner.GetIdentifierValue(parts[1])),
            3 => Identifier.CreateQualifiedIdentifier(
                scanner.GetIdentifierValue(parts[0]),
                scanner.GetIdentifierValue(parts[1]),
                scanner.GetIdentifierValue(parts[2])),
            _ => Identifier.CreateQualifiedIdentifier(
                scanner.GetIdentifierValue(parts[0]),
                scanner.GetIdentifierValue(parts[1]),
                scanner.GetIdentifierValue(parts[2]),
                scanner.GetIdentifierValue(parts[3])),
        };

        if (!Comparer.Equals(identifier, objectName))
            result.Add(identifier);
    }
}
