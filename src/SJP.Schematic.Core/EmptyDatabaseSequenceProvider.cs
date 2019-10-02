using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseSequence>();

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return OptionAsync<IDatabaseSequence>.None;
        }
    }
}
