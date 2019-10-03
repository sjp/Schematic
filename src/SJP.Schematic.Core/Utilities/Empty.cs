using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Utilities
{
    public static class Empty
    {
        public static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> Checks { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(Array.Empty<IDatabaseCheckConstraint>());

        public static Task<IReadOnlyCollection<IDatabaseView>> Views { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseView>>(Array.Empty<IDatabaseView>());

        public static Task<IReadOnlyCollection<IDatabaseRoutine>> Routines { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutine>>(Array.Empty<IDatabaseRoutine>());

        public static Task<IReadOnlyCollection<IDatabaseViewComments>> ViewComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseViewComments>>(Array.Empty<IDatabaseViewComments>());

        public static Task<IReadOnlyCollection<IDatabaseRoutineComments>> RoutineComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutineComments>>(Array.Empty<IDatabaseRoutineComments>());

        public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();
    }
}
