using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Utilities
{
    public static class Empty
    {
        public static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> Checks { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(Array.Empty<IDatabaseCheckConstraint>());

        public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();
    }
}
