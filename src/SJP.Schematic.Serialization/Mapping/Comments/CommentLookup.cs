using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Converts comment lookups between the core and DTO representations.
/// </summary>
/// <remarks>
/// Comment lookups are keyed by names that are local to the object the comments are defined on,
/// e.g. a column name within a table. Only a local name is stored in the DTO representation, so a
/// qualified key cannot be represented and is rejected rather than silently truncated.
/// </remarks>
internal static class CommentLookup
{
    /// <summary>
    /// Converts a core comment lookup to its DTO representation.
    /// </summary>
    /// <param name="commentLookup">A comment lookup.</param>
    /// <param name="parameterName">The expression that <paramref name="commentLookup"/> was provided as.</param>
    /// <returns>A comment lookup keyed by local names.</returns>
    /// <exception cref="ArgumentException"><paramref name="commentLookup"/> contains a key qualified by a schema, database or server.</exception>
    public static IReadOnlyDictionary<string, string?> ToDto(
        IReadOnlyDictionary<Identifier, Option<string>> commentLookup,
        [CallerArgumentExpression(nameof(commentLookup))] string? parameterName = null
    )
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var kv in commentLookup)
        {
            if (kv.Key.Schema != null || kv.Key.Database != null || kv.Key.Server != null)
            {
                throw new ArgumentException(
                    $"Comment lookups must be keyed by a local name, but a key qualified by a schema, database or server was found for '{kv.Key.LocalName}'.",
                    parameterName
                );
            }

            result[kv.Key.LocalName] = kv.Value.MatchUnsafe(static c => c, (string?)null);
        }

        return result;
    }

    /// <summary>
    /// Converts a DTO comment lookup to its core representation.
    /// </summary>
    /// <param name="commentLookup">A comment lookup keyed by local names.</param>
    /// <returns>A comment lookup that compares keys in the same manner as the comment providers that create them.</returns>
    public static IReadOnlyDictionary<Identifier, Option<string>> ToCore(IReadOnlyDictionary<string, string?> commentLookup)
    {
        // matches the comparer used by the comment providers, ensuring lookups behave the same after a round-trip
        var result = new Dictionary<Identifier, Option<string>>(IdentifierComparer.Ordinal);

        foreach (var kv in commentLookup)
        {
            result[Identifier.CreateQualifiedIdentifier(kv.Key)] = kv.Value == null
                ? Option<string>.None
                : Option<string>.Some(kv.Value);
        }

        return result;
    }
}
