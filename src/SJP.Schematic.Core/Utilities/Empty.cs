using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// For internal use only. Stores pre-allocated 'empty' objects.
/// </summary>
public static class Empty
{
    /// <summary>
    /// Gets an empty comment lookup.
    /// </summary>
    /// <value>An empty comment lookup.</value>
    public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();
}