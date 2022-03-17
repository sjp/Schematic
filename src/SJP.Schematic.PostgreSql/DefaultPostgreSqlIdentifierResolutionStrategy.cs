﻿using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// An identifier resolver that applies the same resolution rules as PostgreSQL databases.
/// </summary>
/// <seealso cref="IIdentifierResolutionStrategy" />
public class DefaultPostgreSqlIdentifierResolutionStrategy : IIdentifierResolutionStrategy
{
    /// <summary>
    /// Constructs the set of identifiers (in order) that should be used to query the database for an object.
    /// </summary>
    /// <param name="identifier">A database identifier.</param>
    /// <returns>A set of identifiers to query with.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <c>null</c>.</exception>
    public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));

        var localNames = GetResolutionOrder(identifier.LocalName);

        // fast path for basic table lookup
        if (identifier.Schema == null)
            return localNames.Select(Identifier.CreateQualifiedIdentifier);

        var schemaNames = GetResolutionOrder(identifier.Schema);

        var database = identifier.Database != null && identifier.Database.Any(char.IsUpper)
            ? identifier.Database.ToLowerInvariant()
            : identifier.Database;

        var server = identifier.Server;

        return schemaNames
            .SelectMany(schema =>
                localNames.Select(localName =>
                    Identifier.CreateQualifiedIdentifier(server, database, schema, localName)));
    }

    private static IEnumerable<string> GetResolutionOrder(string identifierComponent)
    {
        var isLowerCase = identifierComponent.All(char.IsLower);
        if (!isLowerCase)
            yield return identifierComponent.ToLowerInvariant();

        yield return identifierComponent;
    }
}
