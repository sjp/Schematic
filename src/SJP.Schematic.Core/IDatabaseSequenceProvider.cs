using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseSequenceProvider
    {
        OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default);
    }
}
