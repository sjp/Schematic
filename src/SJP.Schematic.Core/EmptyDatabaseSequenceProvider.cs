using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken)) => _emptySequences;

        private readonly static Task<IReadOnlyCollection<IDatabaseSequence>> _emptySequences = Task.FromResult<IReadOnlyCollection<IDatabaseSequence>>(Array.Empty<IDatabaseSequence>());

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return OptionAsync<IDatabaseSequence>.None;
        }
    }
}
