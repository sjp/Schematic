using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml;

internal static class IdentifierExtensions
{
    public static string ToVisibleName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var localName = identifier.LocalName.RemoveEnclosingQuotingCharacters();
        if (identifier.Schema == null)
            return localName;

        var builder = StringBuilderCache.Acquire();

        builder.Append(identifier.Schema.RemoveEnclosingQuotingCharacters())
            .Append('_')
            .Append(localName);

        return builder.GetStringAndRelease();
    }

    public static string ToDbmlName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        return identifier.ToVisibleName().ToDbmlIdentifier();
    }
}
