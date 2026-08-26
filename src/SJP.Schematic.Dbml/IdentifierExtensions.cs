using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dbml;

internal static class IdentifierExtensions
{
    public static string ToVisibleName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return GetNameParts(identifier).Join(".");
    }

    public static string ToDbmlName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var parts = GetNameParts(identifier);
        var localName = parts[^1].ToDbmlIdentifier();
        if (parts.Count == 1)
            return localName;

        // DBML qualifies an object by at most one schema, so any server and database components
        // are folded into the schema component. Quoting each component separately keeps names
        // such as 'a_b'.'c' and 'a'.'b_c' distinct from one another.
        var schemaName = parts.GetRange(0, parts.Count - 1).Join(".").ToDbmlIdentifier();

        return schemaName + "." + localName;
    }

    public static string ToDbmlLocalName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return identifier.LocalName.RemoveEnclosingQuotingCharacters().ToDbmlIdentifier();
    }

    private static List<string> GetNameParts(Identifier identifier)
    {
        var parts = new List<string>(4);

        if (identifier.Server != null)
            parts.Add(identifier.Server.RemoveEnclosingQuotingCharacters());
        if (identifier.Database != null)
            parts.Add(identifier.Database.RemoveEnclosingQuotingCharacters());
        if (identifier.Schema != null)
            parts.Add(identifier.Schema.RemoveEnclosingQuotingCharacters());

        parts.Add(identifier.LocalName.RemoveEnclosingQuotingCharacters());

        return parts;
    }
}
