using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Utilities
{
    public static class Empty
    {
        public static Task<IReadOnlyCollection<IRelationalDatabaseTable>> Tables { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>(Array.Empty<IRelationalDatabaseTable>());

        public static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> Checks { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(Array.Empty<IDatabaseCheckConstraint>());

        public static Task<IReadOnlyCollection<IDatabaseView>> Views { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseView>>(Array.Empty<IDatabaseView>());

        public static Task<IReadOnlyCollection<IDatabaseSequence>> Sequences { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequence>>(Array.Empty<IDatabaseSequence>());

        public static Task<IReadOnlyCollection<IDatabaseSynonym>> Synonyms { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonym>>(Array.Empty<IDatabaseSynonym>());

        public static Task<IReadOnlyCollection<IDatabaseRoutine>> Routines { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutine>>(Array.Empty<IDatabaseRoutine>());

        public static Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> TableComments { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTableComments>>(Array.Empty<IRelationalDatabaseTableComments>());

        public static Task<IReadOnlyCollection<IDatabaseViewComments>> ViewComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseViewComments>>(Array.Empty<IDatabaseViewComments>());

        public static Task<IReadOnlyCollection<IDatabaseSequenceComments>> SequenceComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequenceComments>>(Array.Empty<IDatabaseSequenceComments>());

        public static Task<IReadOnlyCollection<IDatabaseSynonymComments>> SynonymComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonymComments>>(Array.Empty<IDatabaseSynonymComments>());

        public static Task<IReadOnlyCollection<IDatabaseRoutineComments>> RoutineComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutineComments>>(Array.Empty<IDatabaseRoutineComments>());

        public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();
    }
}
