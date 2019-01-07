using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities
{
    public static class Empty
    {
        public static Task<IReadOnlyCollection<IRelationalDatabaseTable>> Tables { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>(Array.Empty<IRelationalDatabaseTable>());

        public static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> Checks { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(Array.Empty<IDatabaseCheckConstraint>());

        public static Task<IReadOnlyCollection<IDatabaseView>> Views { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseView>>(Array.Empty<IDatabaseView>());

        public static Task<IReadOnlyCollection<IDatabaseSequence>> Sequences { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequence>>(Array.Empty<IDatabaseSequence>());

        public static Task<IReadOnlyCollection<IDatabaseSynonym>> Synonyms { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonym>>(Array.Empty<IDatabaseSynonym>());
    }
}
