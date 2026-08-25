using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for <see cref="Identifier"/>.
/// </summary>
public static class IdentifierExtensions
{
    /// <summary>
    /// Renders an identifier as a dotted qualified name, e.g. <c>main.film_actor</c>, omitting
    /// any parts the identifier does not carry.
    /// </summary>
    /// <param name="identifier">A database identifier.</param>
    /// <returns>The identifier's qualified name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null" />.</exception>
    /// <remarks>
    /// <see cref="Identifier.ToString"/> deliberately returns a debugger representation, so it
    /// must not be used anywhere a name is shown to a person or written to a data file. This is
    /// the method for those cases.
    /// </remarks>
    public static string ToQualifiedName(this Identifier identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        var builder = StringBuilderCache.Acquire();

        foreach (var part in GetParts(identifier))
        {
            if (builder.Length > 0)
                builder.Append('.');
            builder.Append(part);
        }

        return builder.GetStringAndRelease();
    }

    private static IEnumerable<string> GetParts(Identifier identifier)
    {
        if (identifier.Server != null)
            yield return identifier.Server;
        if (identifier.Database != null)
            yield return identifier.Database;
        if (identifier.Schema != null)
            yield return identifier.Schema;

        yield return identifier.LocalName;
    }
}
