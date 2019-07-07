using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface ISequenceRule : IRule
    {
        IEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences);

        Task<IEnumerable<IRuleMessage>> AnalyseSequencesAsync(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default);
    }
}
