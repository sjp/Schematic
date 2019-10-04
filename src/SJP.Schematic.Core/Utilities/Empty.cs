using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Utilities
{
    public static class Empty
    {
        public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();
    }
}
