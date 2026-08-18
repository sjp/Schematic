using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An identifier resolver that applies the same resolution rules as Oracle databases.
/// </summary>
/// <seealso cref="IIdentifierResolutionStrategy" />
public class DefaultOracleIdentifierResolutionStrategy : IIdentifierResolutionStrategy
{
    /// <summary>
    /// Constructs the set of identifiers (in order) that should be used to query the database for an object.
    /// </summary>
    /// <param name="identifier">A database identifier.</param>
    /// <returns>A set of identifiers to query with.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />.</exception>
    public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        // Materialized once: localNames is otherwise a lazy yield-return iterator that SelectMany below
        // would re-run (re-evaluating the case check) once per schema candidate.
        var localNames = GetResolutionOrder(identifier.LocalName).ToList();

        // fast path for basic table lookup
        if (identifier.Schema == null)
            return localNames.Select(Identifier.CreateQualifiedIdentifier).Distinct();

        var schemaNames = GetResolutionOrder(identifier.Schema).ToList();

        var database = identifier.Database != null && identifier.Database.Any(char.IsLower)
            ? identifier.Database.ToUpperInvariant()
            : identifier.Database;

        var server = identifier.Server;

        return schemaNames
            .SelectMany(schema =>
                localNames.Select(localName =>
                    Identifier.CreateQualifiedIdentifier(server, database, schema, localName)))
            .Distinct();
    }

    private static IEnumerable<string> GetResolutionOrder(string identifierComponent)
    {
        // Deliberately mirrors the Any(char.IsLower) check used for the database component above,
        // rather than All(char.IsUpper) — the latter is false for any identifier containing a digit or
        // underscore (IsUpper('_') and IsUpper('1') are both false), so an already-uppercase name like
        // MY_TABLE would otherwise yield ToUpperInvariant() (identical to the input) as a second,
        // redundant candidate, doubling the resolution queries issued for it.
        var isUpperCase = !identifierComponent.Any(char.IsLower);
        if (!isUpperCase)
            yield return identifierComponent.ToUpperInvariant();

        yield return identifierComponent;
    }
}