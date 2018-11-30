using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseSequenceProvider
    {
        Option<IDatabaseSequence> GetSequence(Identifier sequenceName);

        OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
